namespace BIVN.FixedStorage.Identity.API.Service
{
    public class RoleService : IRoleService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        private readonly IdentityContext _identityContext;
        private readonly DeviceTokenContext _deviceTokenContext;

        private readonly ILogger<RoleService> _logger;

        public RoleService(UserManager<AppUser> userManager,
                                RoleManager<AppRole> roleManager,

                                IdentityContext identityContext,
                                DeviceTokenContext deviceTokenContext,
                                ILogger<RoleService> logger
                                )
        {
            _userManager = userManager;
            _roleManager = roleManager;

            _identityContext = identityContext;
            _deviceTokenContext = deviceTokenContext;
            _logger = logger;

        }

        public async Task<ResponseModel<bool>> ExistRoleNameAsync(string roleName)
        {
            var existRoleName = await _roleManager.RoleExistsAsync(roleName);
            if (existRoleName)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = true,
                    Message = "Tên nhóm quyền đã tồn tại. Vui lòng thử lại."
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = false,
                Message = "Tên quyền hợp lệ"
            };
        }

        public async Task<ResponseModel<IEnumerable<RoleInfoModel>>> GetRolesAsync()
        {
            var roleClaims = (from r in _identityContext.Roles.AsNoTracking()
                              join cl in _identityContext.RoleClaims.AsNoTracking() on r.Id equals cl.RoleId into rclGroup
                              orderby r.CreatedAt.Value ascending
                              select new RoleInfoModel
                              {
                                  RoleId = r.Id.ToString(),
                                  Name = r.Name,
                                  Permissions = rclGroup != null ? (List<PermissionModel>)rclGroup.Select(p => new PermissionModel
                                  {
                                      Active = true,
                                      Category = p.ClaimType == Constants.Permissions.DEPARTMENT_DATA_INQUIRY
                                                  ? Constants.Permissions.DEPARTMENT_DATA_INQUIRY
                                                  : p.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY
                                                      ? Constants.Permissions.FACTORY_DATA_INQUIRY
                                                      : p.ClaimType == Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT
                                                      ? Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT
                                                      : p.ClaimType,
                                      Name = p.ClaimType == Constants.Permissions.DEPARTMENT_DATA_INQUIRY 
                                                            || p.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY 
                                                            || p.ClaimType == Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT
                                                  ? p.ClaimValue
                                                  : p.ClaimType
                                  }) : default
                              });

            if (roleClaims?.Any() == false)
            {
                return new ResponseModel<IEnumerable<RoleInfoModel>>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tìm thấy quyền nào"
                };
            }

            return new ResponseModel<IEnumerable<RoleInfoModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = roleClaims,
                Message = "Danh sách quyền"
            };
        }

        public async Task<ResponseModel<bool>> CreateAsync(CreateRoleModel createRoleModel)
        {
            var existNameResult = await ExistRoleNameAsync(createRoleModel?.Name);
            if (existNameResult.Data == true)
            {
                return existNameResult;
            }

            try
            {
                var createRole = new AppRole
                {
                    Name = createRoleModel.Name,
                    Description = createRoleModel.Name,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createRoleModel.UserId
                };

                await _roleManager.CreateAsync(createRole);

                foreach (var permission in createRoleModel.Permissions)
                {
                    Claim claim;
                    if (permission.Category == Constants.Permissions.DEPARTMENT_DATA_INQUIRY 
                        || permission.Category == Constants.Permissions.FACTORY_DATA_INQUIRY
                        || permission.Category == Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT)
                    {
                        claim = new Claim(permission.Category, permission.Name);
                    }
                    else
                    {
                        claim = new Claim(permission.Name, string.Empty);
                    }

                    if (permission?.Active == true)
                    {
                        await _roleManager.AddClaimAsync(createRole, claim);
                    }
                    else
                    {
                        await _roleManager.RemoveClaimAsync(createRole, claim);
                    }
                }

                await _identityContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện tạo quyền", ex);
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Có lỗi khi thực hiện lưu quyền"
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status400BadRequest,
                Data = true,
                Message = "Thêm nhóm quyền thành công."
            };
        }
        public async Task<ResponseModel<bool>> EditAsync(EditRoleModel editRoleModel)
        {
            //Check role name exist
            var getExistRole = await _roleManager.FindByNameAsync(editRoleModel.Name);
            if (getExistRole != null && getExistRole.Id != Guid.Parse(editRoleModel.RoleId))
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Tên nhóm quyền đã tồn tại. Vui lòng thử lại."
                };
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(editRoleModel.RoleId);
                //Update role name
                role.Name = editRoleModel.Name;
                role.UpdatedAt = DateTime.Now;
                role.UpdatedBy = editRoleModel.UserId;

                var roleClaims = await _roleManager.GetClaimsAsync(role);
               
                //Update permissions
                foreach (var permission in editRoleModel.Permissions)
                {
                    Claim claim;
                    if (permission.Category == Constants.Permissions.DEPARTMENT_DATA_INQUIRY 
                        || permission.Category == Constants.Permissions.FACTORY_DATA_INQUIRY
                        || permission.Category == Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT)
                    {
                        claim = new Claim(permission.Category, permission.Name.ToUpper());
                    }
                    else
                    {
                        claim = new Claim(permission.Name, "");
                    }

                    if (permission?.Active == true)
                    {
                        //If permission already exist => exit
                        if (roleClaims?.Any(x => x.Type == claim.Type && x.Value.ToUpper() == claim.Value.ToUpper()) == true) continue;

                        await _roleManager.AddClaimAsync(role, claim);
                    }
                    else
                    {
                        await _roleManager.RemoveClaimAsync(role, claim);
                    }
                }

                await _identityContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi cập nhật sửa quyền", ex);
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Có lỗi khi cập nhật sửa quyền"
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status400BadRequest,
                Data = true,
                Message = "Chỉnh sửa nhóm quyền thành công."
            };
        }

        public async Task<ResponseModel<bool>> PrepareDeleteAsync(string roleId)
        {
            var convertedRoleId = Guid.Parse(roleId);
            var anyUserInRole = await _identityContext.UserRoles.AnyAsync(x => x.RoleId == convertedRoleId);
            if (anyUserInRole)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Bạn không thể xóa vì nhóm quyền này đã được phân quyền cho nhân viên."
                };
            }


            var result = await _roleManager.FindByIdAsync(roleId);
            if (result == null)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = false,
                    Message = "Không tìm thấy quyền này"
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Quyền tồn tại, có thể thực hiện xóa"
            };
        }
        public async Task<ResponseModel<bool>> DeleteAsync(string roleId, string userId)
        {
            var prepareDeleteResult = await PrepareDeleteAsync(roleId);
            if (prepareDeleteResult.Data == false)
            {
                return prepareDeleteResult;
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = false,
                    Message = "Không tìm thấy quyền"
                };
            }

            //Update
            role.DeletedAt = DateTime.Now;
            role.DeletedBy = userId;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            await _roleManager.DeleteAsync(role);

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Xóa nhóm quyền thành công."
            };
        }

        public async Task<ResponseModel<bool>> AssignUserRoleAsync(string userId, string roleId, string updatedBy)
        {
            //Retrieve user
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser != null)
            {
                var userRoles = _identityContext.UserRoles.Where(x => x.UserId == Guid.Parse(userId));
                //Remove old roles
                if (userRoles?.Any() == true)
                {
                    _identityContext.UserRoles.RemoveRange(userRoles);
                }
                //Add new role
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role != null)
                {
                    var userRole = new IdentityUserRole<Guid>
                    {
                        UserId = Guid.Parse(userId),
                        RoleId = role.Id
                    };

                    _identityContext.UserRoles.Add(userRole);
                    await _identityContext.SaveChangesAsync();

                    //invalid token
                    var loggedInUser = _deviceTokenContext.DeviceTokens.Where(x => x.UserId == userId).ToList();
                    loggedInUser.Select(x =>
                     {
                         x.Status = false;
                         x.ForceLogoutCode = HttpStatusCodes.RoleChanged60;
                         return x;

                     });
                    _deviceTokenContext.DeviceTokens.UpdateRange(loggedInUser);

                    appUser.UpdatedBy = updatedBy;
                    appUser.UpdatedAt = DateTime.Now;
                    await _userManager.UpdateAsync(appUser);
                    await _deviceTokenContext.SaveChangesAsync();
                    return new ResponseModel<bool>
                    {
                        Code = StatusCodes.Status200OK,
                        Data = true,
                        Message = "Sửa quyền cho người dùng thành công."
                    };
                }
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status404NotFound,
                Data = false,
                Message = "Sửa quyền cho người dùng thất bại."
            };
        }

        
    }
}
