using Identity.API.Service.Dtos;

namespace BIVN.FixedStorage.Identity.API.Service
{
    public partial class UserService : IUserService
    {

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IdentityContext _identityContext;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RestClientFactory _restClientFactory;
        private readonly HttpContext _httpContext;

        public UserService(IConfiguration configuration
            , IWebHostEnvironment environment
            , IdentityContext identityContext
            , RoleManager<AppRole> roleManager
            , UserManager<AppUser> userManager
            , RestClientFactory restClientFactory
            , IHttpContextAccessor httpContextAccessor
            )
        {
            _configuration = configuration;
            _environment = environment;
            _identityContext = identityContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _restClientFactory = restClientFactory;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<ResponseModel> GetUserInfo(string userId)
        {
            var getDepartment = await (from p in _identityContext.AppUsers.AsNoTracking()
                                       join s in _identityContext.Departments.AsNoTracking() on p.DepartmentId equals s.Id.ToString()
                                       where p.Id.ToString().Contains(userId)
                                       select s.Name).FirstOrDefaultAsync();
            //if (getDepartment == null)
            //{
            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status404NotFound,
            //        Message = "Không tìm thấy dữ liệu"
            //    };
            //}
            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);
            var avatarFullPath = $"{InitRootPath()}/{user.Avatar}";
            var pathExist = new FileInfo(avatarFullPath).Exists;

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Thông tin chi tiết người dùng",
                Data = new GetUserInfoDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Code = user.Code,
                    //password = user.PasswordHash,
                    Avatar = pathExist ? user.Avatar : string.Empty,
                    RoleName = userRoles.FirstOrDefault(),
                    Department = getDepartment != null ? getDepartment : string.Empty,
                }
            };
        }

        public async Task<ResponseModel> UpdateUserInfo(UpdateUserInfoDto UpdateUserInfo)
        {
            var getUserInfo = await _identityContext.AppUsers.FirstOrDefaultAsync(x => x.Id.ToString().Contains(UpdateUserInfo.userId));
            if (getUserInfo == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu"
                };
            }
            ////Tạo api Folder
            //var apiFolder = InitTypeDocumentPath(InitRootPath(), "api");

            ////Tạo identity Folder
            //var identityFolder = InitTypeDocumentPath(apiFolder, "identity");

            //Tạo Images Folder
            var imagesFolder = InitTypeDocumentPath(InitRootPath(), "images/identity");

            //Tạo CodeUser Folder
            var codeUserFolder = InitTypeDocumentPath(imagesFolder, getUserInfo?.Code);

            var avatar = $"{Guid.NewGuid()}{Path.GetExtension(UpdateUserInfo.file.FileName)}";

            // Luu duong dan duoi dang: wwwroot/api/identity/images/CodeUser/FileName
            var imagePath = Path.Combine(codeUserFolder, avatar);

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await UpdateUserInfo.file.CopyToAsync(stream);
                stream.Flush();
                stream.Close();
            }
            //Cap nhat DB truong Avatar:
            if (!string.IsNullOrEmpty(getUserInfo.Avatar))
            {
                var oldAvatarPath = $"{InitRootPath()}{getUserInfo.Avatar}";
                var oldAvatar = new FileInfo(oldAvatarPath);
                if (oldAvatar.Exists)
                {
                    oldAvatar.Delete();
                }

            }
            getUserInfo.Avatar = $"/images/identity/{getUserInfo?.Code}/{avatar}";
            getUserInfo.UpdatedAt = DateTime.Now;
            await _identityContext.SaveChangesAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Cập nhật thông tin người dùng thành công."
            };
        }

        public async Task<ResponseModel<IEnumerable<FilterListUserDto>>> FilterUser(FilterUseModel filterUseModel)
        {
            //query:
            var query = new StringBuilder();
            query.Append("select u.Id as 'userId', u.UserName as 'userName', u.FullName as 'fullName', u.Code as 'code', u.Status as 'status' , u.AccountType as 'accountType', d.Id as 'departmentId', d.Name as 'departmentName', r.Id as 'roleId', r.Name as 'roleName', u.CreatedAt as 'createAt', u.CreatedBy as 'createdBy', u.UpdatedAt as 'updatedAt', u.UpdatedBy as 'updateddBy'  from AppUsers as u" +
                                " left join Departments as d on u.DepartmentId = d.Id" +
                                " left join AppUserRoles as ur on u.Id = ur.UserId" +
                                " left join AppRoles as r on ur.RoleId = r.Id" +
                            $" where (1=1)");

            if (!filterUseModel.UserName.IsNullOrEmpty())
            {
                query.Append($" and u.UserName like '%{filterUseModel.UserName}%'");
            }
            if (!filterUseModel.FullName.IsNullOrEmpty())
            {
                query.Append($" and u.FullName like N'%{filterUseModel.FullName}%'");
            }
            if (!filterUseModel.Code.IsNullOrEmpty())
            {
                query.Append($" and u.Code like '%{filterUseModel.Code}%'");
            }

            //Danh sách phòng ban được phân quyền:
            var currUser = _httpContext.User;
            var departments = currUser.Claims.Where(x => x.Type == Constants.Permissions.DEPARTMENT_DATA_INQUIRY).Select(x => x.Value).ToList();

            //query departments:
            if (filterUseModel.DepartmentIds.Count() > 0)
            {
                var queryDepartment = filterUseModel.DepartmentIds.Select(x => $"'{x}'").ToList();
                query.Append($" and d.Id IN ({string.Join(",", queryDepartment)})");
            }
            else
            {
                var queryDepartment = departments.Select(x => $"'{x}'").ToList();
                query.Append($" and d.Id IN ({string.Join(",", queryDepartment)})");
            }

            //query roles:
            if (filterUseModel.RoleIds.Count() > 0)
            {
                var queryRole = filterUseModel.RoleIds.Select(x => $"'{x}'").ToList();
                query.Append($" and r.Id IN ({string.Join(",", queryRole)})");
            }

            //query status:
            if (filterUseModel.Status.Count() > 0)
            {
                var queryStatus = filterUseModel.Status.Select(x => $"'{x}'").ToList();
                query.Append($" and u.Status IN ({string.Join(",", queryStatus)})");
            }

            //query AccountType:
            if (filterUseModel.AccountTypes.Count() > 0)
            {
                var queryAccountType = filterUseModel.AccountTypes.Select(x => $"'{x}'").ToList();
                query.Append($" and u.AccountType IN ({string.Join(",", queryAccountType)})");
            }

            //Sắp xếp theo logic: tài khoản có UserName là Administrator lên đầu, còn lại sắp xếp theo ngày tạo sẽ giảm dần:
            query.Append($" order by case when u.Id like '%{Constants.Roles.ID_Administrator}%' then 0 else 1 end, u.CreatedAt desc");

            //using var conn = _identityContext.Database.GetDbConnection();

            using (var connection = _identityContext.Database.GetDbConnection())
            {
                var result = await connection.QueryAsync<FilterListUserDto>(query.ToString());
                await connection.DisposeAsync();
                if (result.Any() == false)
                {
                    return new ResponseModel<IEnumerable<FilterListUserDto>>
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Không tìm thấy người dùng",
                    };
                }
                return new ResponseModel<IEnumerable<FilterListUserDto>>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Danh sách người dùng.",
                    Data = result
                };

            }


        }

        public async Task<ResponseModel<IList<string>>> StatusUser()
        {
            var getAllStatus = EnumHelper<UserStatus>.GetDisplayValues(UserStatus.Active);
            if (getAllStatus == null)
            {
                return new ResponseModel<IList<string>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không có danh sách trạng thái",
                };
            }
            return new ResponseModel<IList<string>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Danh sách các trạng thái",
                Data = getAllStatus
            };
        }

        public string InitRootPath()
        {
            //root folder in docker 
            if (_environment.IsDevelopment())
            {
                var currPath = _environment.ContentRootPath;
                //var parentOfCurrPath = Directory.GetParent(currPath).FullName;

                var rootAPIPath = Path.Combine(currPath, "wwwroot");
                if (!Directory.Exists(Path.Combine(rootAPIPath)))
                {
                    Directory.CreateDirectory(rootAPIPath);
                }
                return rootAPIPath;
            }
            else
            {
                var rootFolder = _configuration["UploadPath"];
                return rootFolder;
            }

        }

        public string InitTypeDocumentPath(string previousPath, string typeDocument)
        {
            var path = Path.Combine(previousPath, typeDocument);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public async Task<ResponseModel<CreateUserDto>> CreateUserAsync()
        {
            var result = new ResponseModel<CreateUserDto>() { Data = new CreateUserDto() };
            var roleList = new List<CreateAppRoleDto>() { new CreateAppRoleDto { Id = string.Empty, Name = "Chọn nhóm quyền" } };
            var rolesInDb = await _roleManager.Roles.Select(x => new CreateAppRoleDto { Id = x.Id.ToString(), Name = x.Name }).ToListAsync();
            roleList.AddRange(rolesInDb);
            result.Data.RoleList = roleList;

            var departmentList = new List<DepartmentListDto>() { new DepartmentListDto { Id = string.Empty, Name = "Chọn phòng ban" } };
            var departmentsDb = await _identityContext.Departments.AsNoTracking().Select(x => new DepartmentListDto { Id = x.Id.ToString(), Name = x.Name }).ToListAsync();
            departmentList.AddRange(departmentsDb);
            result.Data.DepartmentList = departmentList;

            var userAccTypes = new List<int>();
            var userAccTypeDropDownList = new List<CreateAccountTypeDto>();
            foreach (var accType in (AccountType[])Enum.GetValues(typeof(AccountType)))
            {
                userAccTypes.Add((int)accType);
                userAccTypeDropDownList.Add(new CreateAccountTypeDto { Id = (int)accType, Name = EnumHelper<AccountType>.GetDisplayValue(accType) });
            }
            result.Data.AccountTypeList = userAccTypeDropDownList;

            var userStatuses = new List<int>();
            var userStatusDropDownList = new List<CreateUserStatusDto>();
            foreach (var item in (UserStatus[])Enum.GetValues(typeof(UserStatus)))
            {
                if (item == UserStatus.LockByExpiredPassword || item == UserStatus.LockByUnactive)
                {
                    continue;
                }

                var itemVal = (int)item;
                userStatuses.Add(itemVal);
                userStatusDropDownList.Add(new CreateUserStatusDto { Id = (int)item, Name = EnumHelper<UserStatus>.GetDisplayValue(item) });
            }
            result.Data.UserStatusList = userStatusDropDownList;
            result.Code = StatusCodes.Status200OK;
            return result;
        }

        public async Task<ValidateDto<CreateUserErrorDto>> ValidateCreateUserInputAsync(CreateUserDto model)
        {
            var result = new ValidateDto<CreateUserErrorDto>() { Data = new CreateUserErrorDto(), IsInvalid = false };
            if (string.IsNullOrEmpty(model?.UserId) || !string.IsNullOrEmpty(model.UserId) && Guid.TryParse(model.UserId.Trim(), out var _) == false)
            {
                result.Data.Username = "Id Người tạo không hợp lệ";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model?.Username))
            {
                result.Data.Username = "Vui lòng nhập Tên tài khoản đăng nhập";
                result.IsInvalid = true;
            }
            //else if (model.Username.Trim().Contains(' '))
            //{
            //    result.Data.Username = "Tên tài khoản đăng nhập có khoảng trắng là không hợp lệ";
            //    result.IsInvalid = true;
            //}
            else if (!StringHelper.HasOnlyNormalEnglishCharacters(model.Username.Trim()))
            {
                result.Data.Username = "Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.Username.Trim().Length > 15)
            {
                result.Data.Username = "Cho phép nhập Tên tài khoản đăng nhập tối đa 15 ký tự";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model?.Password))
            {
                result.Data.Password = "Vui lòng nhập Mật khẩu";
                result.IsInvalid = true;
            }
            else if (model.Password.Trim().Contains(' '))
            {
                result.Data.Password = " Mật khẩu có khoảng trắng là không hợp lệ";
                result.IsInvalid = true;
            }
            else if (!PasswordRegex().IsMatch(model.Password.Trim()))
            {
                result.Data.Password = " Mật khẩu không hợp lệ. Yêu cầu phải có ít nhất 1 ký tự chữ và 1 ký tự chữ số";
                result.IsInvalid = true;
            }
            else if (model.Password.Trim().Length < 8 || model.Password.Trim().Length > 15)
            {
                result.Data.Password = "Cho phép nhập Mật khẩu trong khoảng 8 đến 15 ký tự";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model?.FullName))
            {
                result.Data.FullName = "Vui lòng nhập Họ tên";
                result.IsInvalid = true;
            }
            else if (StringHelper.HasSpecialCharacters(model.FullName.Trim()))
            {
                result.Data.FullName = " Họ và tên không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.FullName.Trim().Length > 100)
            {
                result.Data.FullName = "Cho phép nhập Họ tên tối đa 100 ký tự";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model?.Code))
            {
                result.Data.Code = "Vui lòng nhập Mã nhân viên";
                result.IsInvalid = true;
            }
            else if (!CodeRegex().IsMatch(model.Code.Trim()))
            {
                result.Data.Code = "Mã nhân viên không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.Code.Trim().Length > 8)
            {
                result.Data.Code = "Cho phép nhập Mã nhân viên tối đa 8 ký tự";
                result.IsInvalid = true;
            }

            if (!string.IsNullOrEmpty(model?.RoleId) && Guid.TryParse(model.RoleId.Trim(), out var _) == false)
            {
                result.Data.RoleId = "Id Quyền không hợp lệ";
                result.IsInvalid = true;
            }

            if (!string.IsNullOrEmpty(model?.DepartmentId) && Guid.TryParse(model.DepartmentId.Trim(), out var _) == false)
            {
                result.Data.DepartmentId = "Id Phòng ban không hợp lệ";
                result.IsInvalid = true;
            }

            if (((AccountType[])Enum.GetValues(typeof(AccountType))).Any(x => x == (AccountType)model.AccountType) == false)
            {
                result.Data.AccountType = "Loại tài khoản không hợp lệ";
                result.IsInvalid = true;
            }

            if (!IsValidStatus(model.Status))
            {
                result.Data.Status = "Trạng thái không hợp lệ";
                result.IsInvalid = true;
            }

            if (model.LockPwdSetting && string.IsNullOrEmpty(model.LockPwdTime))
            {
                result.Data.LockPwTime = "Vui lòng nhập thời gian hiệu lực";
                result.IsInvalid = true;
            }
            else if (model.LockPwdSetting && !string.IsNullOrEmpty(model.LockPwdTime) && !Int32.TryParse(model.LockPwdTime, out var __))
            {
                result.Data.LockPwTime = "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }

            if (model.LockActSetting && string.IsNullOrEmpty(model.LockActTime))
            {
                result.Data.LockActTime = "Vui lòng nhập thời gian hiệu lực";
                result.IsInvalid = true;
            }
            else if (model.LockActSetting && !string.IsNullOrEmpty(model.LockActTime) && !Int32.TryParse(model.LockActTime, out var __))
            {
                result.Data.LockActTime = "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }

            if (result.IsInvalid)
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Dữ liệu không hợp lệ";
            }
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel<CreateUserResultDto>> CreateUserAsync(CreateUserDto model)
        {
            var result = new ResponseModel<CreateUserResultDto>() { Data = new CreateUserResultDto(), Code = StatusCodes.Status200OK };
            var usernameExists = await _userManager.FindByNameAsync(model.Username.Trim());
            if (usernameExists != null)
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Data.Username = "Tài khoản đăng nhập đã tồn tại. Vui lòng thử lại.";
            }
            //var codeExists = await _userManager.FindByUserCodeAsync(model.Code.Trim());
            //if (codeExists != null)
            //{
            //    result.Code = StatusCodes.Status400BadRequest;
            //    result.Data.Code = "Mã nhân viên đã tồn tại";
            //}

            var role = !string.IsNullOrEmpty(model?.RoleId?.Trim()) ? await _roleManager.FindByIdAsync(model?.RoleId?.Trim()) : null;
            //if (role == null)
            //{
            //    result.Code = StatusCodes.Status400BadRequest;
            //    result.Data.RoleId = "Quyền không còn tồn tại";
            //}

            var department = !string.IsNullOrEmpty(model?.DepartmentId?.Trim()) && Guid.TryParse(model.DepartmentId, out var _departmentId) ? await _identityContext.Departments.FindAsync(_departmentId) : null;
            //if (department == null)
            //{
            //    result.Code = StatusCodes.Status400BadRequest;
            //    result.Data.DepartmentId = "Phòng ban không còn tồn tại";
            //}

            if (result.Code != StatusCodes.Status200OK)
            {
                result.Message = "Tạo người dùng không thành công";
                return result;
            }

            //Check user có loại tài khoản là Tài khoản riêng, mã nhân viên sẽ không được trùng trong DB:
            //if (model.AccountType.Equals((int)AccountType.TaiKhoanRieng))
            //{
            //    var checkCodeUser = await _identityContext.AppUsers.FirstOrDefaultAsync(x => x.Code.Contains(model.Code));
            //    if (checkCodeUser != null)
            //    {
            //        result.Code = (int)HttpStatusCodes.CodeUserExisted;
            //        result.Message = "Mã nhân viên đã tồn tại. Vui lòng nhập lại.";
            //        return result;
            //    }
            //}

            // Remove redundant whitespaces from fullname
            if (model.FullName.Trim().Contains(" "))
            {
                var getFullNameParts = model.FullName.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (getFullNameParts?.Any() == true)
                {
                    model.FullName = string.Join(" ", getFullNameParts);
                }
            }

            var createRes = await _userManager.CreateAsync(new AppUser()
            {
                UserName = model.Username.Trim(),
                FullName = model.FullName,
                Code = model.Code.Trim(),
                DepartmentId = department != null ? department?.Id.ToString() : null,
                AccountType = (AccountType)model.AccountType,
                CreatedAt = DateTime.Now,
                Status = (UserStatus)model.Status,
                LockPwdSetting = model.LockPwdSetting,
                LockPwTime = !string.IsNullOrEmpty(model.LockPwdTime) ? Int32.Parse(model.LockPwdTime) : null,
                LockActSetting = model.LockActSetting,
                LockActTime = !string.IsNullOrEmpty(model.LockActTime) ? Int32.Parse(model.LockActTime) : null,
                CreatedBy = model.UserId,
                UpdatedPasswordAt = DateTime.Now
            }, model.Password.Trim());
            if (createRes != null)
            {
                var userExist = await _userManager.FindByNameAsync(model.Username.Trim());
                if (role != null && userExist != null)
                {
                    await _userManager.AddToRoleAsync(userExist, role?.Name);
                }
            }

            result.Data.Username = model.Username.Trim();
            result.Code = StatusCodes.Status200OK;
            result.Message = "Tạo tài khoản thành công";
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel<UserDetailDto>> GetUserDetailAsync(string id)
        {
            var result = new ResponseModel<UserDetailDto>(StatusCodes.Status200OK, new UserDetailDto());
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                result.Code = (int)HttpStatusCodes.TheAccountIsNotExisted;
                result.Message = result.Data.Username = "Tài khoản không tồn tại";
                return result;
            }
            result.Data.Id = user.Id.ToString();
            result.Data.Username = user.UserName;
            result.Data.Fullname = user.FullName;
            result.Data.Code = user.Code;
            result.Data.DepartmentId = user.DepartmentId?.ToString().ToUpper();
            result.Data.DepartmentList = await GetDepartmentListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles?.Any() == true)
            {
                var roleIdList = new List<string>();
                foreach (var roleName in userRoles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        roleIdList.Add(role.Id.ToString().ToUpper());
                    }
                }
                if (roleIdList.Any() == true)
                {
                    result.Data.RoleId = string.Join(",", roleIdList);
                }
            }
            result.Data.RoleList = await GetRoleListAsync();
            result.Data.AccountType = (int)user.AccountType;
            result.Data.AccountTypeList = await GetAccountTypeListAsync();
            result.Data.Status = (int)user.Status;
            result.Data.UserStatusList = await GetUserStatusListAsync();
            result.Data.LockPwdSetting = user.LockPwdSetting;
            result.Data.LockPwdTime = user.LockPwTime;
            result.Data.LockActSetting = user.LockActSetting;
            result.Data.LockActTime = user.LockActTime;
            result.Data.RoleName = userRoles.FirstOrDefault();
            result.Data.DepartmentName = !string.IsNullOrEmpty(user.DepartmentId) ? _identityContext?.Departments?.FirstOrDefault(x => x.Id == Guid.Parse(user.DepartmentId))?.Name : string.Empty;
            result.Data.StatusName = EnumHelper<UserStatus>.GetDisplayValue(user.Status);
            result.Data.AccountTypeName = EnumHelper<AccountType>.GetDisplayValue(user.AccountType.Value);
            result.Data.CreatedDate = user.CreatedAt.HasValue ? user?.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;
            result.Data.CreatedBy = user.CreatedBy.IsNullOrEmpty() ? string.Empty : _identityContext?.AppUsers?.FirstOrDefault(x => x.Id.ToString().ToLower().Contains(user.CreatedBy.ToLower()))?.FullName;
            result.Data.UpdatedDate = user.UpdatedAt.HasValue ? user?.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;
            result.Data.UpdatedBy = user.UpdatedBy.IsNullOrEmpty() ? string.Empty : _identityContext?.AppUsers?.FirstOrDefault(x => x.Id.ToString().ToLower().Contains(user.UpdatedBy.ToLower()))?.FullName;
            result.Data.LastLoginTime = user.LastActiveTime.HasValue ? user.LastActiveTime.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;
            return await Task.FromResult(result);
        }

        public async Task<List<DropDownListItem>> GetRoleListAsync()
        {
            var roleList = new List<DropDownListItem>() { new DropDownListItem { Id = string.Empty, Name = "Chưa chọn nhóm quyền" } };
            var rolesInDb = await _roleManager.Roles.Select(x => new DropDownListItem { Id = x.Id.ToString(), Name = x.Name }).ToListAsync();
            roleList.AddRange(rolesInDb);
            return roleList;
        }

        public async Task<List<DropDownListItem>> GetDepartmentListAsync()
        {
            var departmentList = new List<DropDownListItem>() { new DropDownListItem { Id = string.Empty, Name = "Chưa chọn phòng ban" } };
            var departmentsDb = await _identityContext.Departments.AsNoTracking().Select(x => new DropDownListItem { Id = x.Id.ToString(), Name = x.Name }).ToListAsync();
            departmentList.AddRange(departmentsDb);
            return departmentList;
        }

        public async Task<List<DropDownListItemInteger>> GetAccountTypeListAsync()
        {
            var userAccTypeDropDownList = new List<DropDownListItemInteger>();
            foreach (var accType in (AccountType[])Enum.GetValues(typeof(AccountType)))
            {
                userAccTypeDropDownList.Add(new DropDownListItemInteger { Id = (int)accType, Name = EnumHelper<AccountType>.GetDisplayValue(accType) });
            }
            return userAccTypeDropDownList;
        }

        public async Task<List<DropDownListItemInteger>> GetUserStatusListAsync()
        {
            var userStatusDrowDownList = new List<DropDownListItemInteger>();
            foreach (var status in (UserStatus[])Enum.GetValues(typeof(UserStatus)))
            {
                userStatusDrowDownList.Add(new DropDownListItemInteger { Id = (int)status, Name = EnumHelper<UserStatus>.GetDisplayValue(status) });
            }
            return userStatusDrowDownList;
        }

        public async Task<ValidateDto<UpdateUserErrorDto>> ValidateUpdateUserInputAsync(UpdateUserDto model)
        {
            var result = new ValidateDto<UpdateUserErrorDto>() { Data = new UpdateUserErrorDto(), IsInvalid = false };
            if (string.IsNullOrEmpty(model?.Id) || !string.IsNullOrEmpty(model.Id) && Guid.TryParse(model.Id.Trim(), out var _) == false)
            {
                result.Data.RoleId = "Id Người dùng không hợp lệ";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model?.Username))
            {
                result.Data.Username = "Vui lòng nhập tài khoản đăng nhập";
                result.IsInvalid = true;
            }
            else if (model.Username.Trim().Contains(' '))
            {
                result.Data.Username = "Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (!StringHelper.HasOnlyNormalEnglishCharacters(model.Username.Trim()))
            {
                result.Data.Username = "Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.Username.Trim().Length > 15)
            {
                result.Data.Username = "Cho phép nhập Tên tài khoản đăng nhập tối đa 15 ký tự";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model?.FullName))
            {
                result.Data.FullName = "Vui lòng nhập Họ tên";
                result.IsInvalid = true;
            }
            else if (StringHelper.HasSpecialCharacters(model.FullName.Trim()))
            {
                result.Data.FullName = " Họ và tên không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.FullName.Trim().Length > 100)
            {
                result.Data.FullName = "Cho phép nhập Họ tên tối đa 100 ký tự";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model?.Code))
            {
                result.Data.Code = "Vui lòng nhập Mã nhân viên";
                result.IsInvalid = true;
            }
            else if (!CodeRegex().IsMatch(model.Code.Trim()))
            {
                result.Data.Code = "Mã nhân viên không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.Code.Trim().Length > 8)
            {
                result.Data.Code = "Cho phép nhập Mã nhân viên tối đa 8 ký tự";
                result.IsInvalid = true;
            }

            if (!string.IsNullOrEmpty(model?.RoleId) && Guid.TryParse(model.RoleId.Trim(), out var _) == false)
            {
                result.Data.RoleId = "Id Quyền không hợp lệ";
                result.IsInvalid = true;
            }

            if (!string.IsNullOrEmpty(model?.DepartmentId) && Guid.TryParse(model.DepartmentId.Trim(), out var _) == false)
            {
                result.Data.DepartmentId = "Id Phòng ban không hợp lệ";
                result.IsInvalid = true;
            }

            if (((AccountType[])Enum.GetValues(typeof(AccountType))).Any(x => x == (AccountType)model.AccountType) == false)
            {
                result.Data.AccountType = "Loại tài khoản không hợp lệ";
                result.IsInvalid = true;
            }

            if (!IsValidStatus(model.Status))
            {
                result.Data.Status = "Trạng thái không hợp lệ";
                result.IsInvalid = true;
            }

            if (model.LockPwdSetting && string.IsNullOrEmpty(model.LockPwdTime))
            {
                result.Data.LockPwTime = "Vui lòng nhập thời gian hiệu lực";
                result.IsInvalid = true;
            }
            else if (model.LockPwdSetting && !string.IsNullOrEmpty(model.LockPwdTime) && !Int32.TryParse(model.LockPwdTime, out var __))
            {
                result.Data.LockPwTime = "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }

            if (model.LockActSetting && string.IsNullOrEmpty(model.LockActTime))
            {
                result.Data.LockActTime = "Vui lòng nhập thời gian hiệu lực";
                result.IsInvalid = true;
            }
            else if (model.LockActSetting && !string.IsNullOrEmpty(model.LockActTime) && !Int32.TryParse(model.LockActTime, out var __))
            {
                result.Data.LockActTime = "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }

            if (result.IsInvalid)
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Dữ liệu không hợp lệ";
            }
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel<UpdateUserDto>> UpdateUserAsync(UpdateUserDto model)
        {
            var result = new ResponseModel<UpdateUserDto>() { Data = new UpdateUserDto(), Code = StatusCodes.Status200OK };
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                result.Code = (int)HttpStatusCodes.TheAccountIsNotExisted;
                result.Message = result.Data.Username = "Tài khoản không tồn tại";
                return result;
            }

            var usernameExists = user.UserName.Trim().ToUpper() != model.Username.Trim().ToUpper() ? await _userManager.FindByNameAsync(model.Username.Trim()) : null;
            if (usernameExists != null)
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Data.Username = "Tài khoản đăng nhập đã tồn tại. Vui lòng thử lại.";
                return result;
            }
            //var codeExists = user.Code.Trim().ToUpper() != model.Code.Trim().ToUpper() ? await _userManager.FindByUserCodeAsync(model.Code.Trim()) : null;
            //if (codeExists != null)
            //{
            //    result.Code = StatusCodes.Status400BadRequest;
            //    result.Data.Code = "Mã nhân viên đã tồn tại. Vui lòng nhập lại.";
            //    return result;
            //}

            //Check user có loại tài khoản là Tài khoản riêng, mã nhân viên sẽ không được trùng trong DB:
            //if (!((int)user.AccountType).Equals(model.AccountType) || !user.Code.Contains(model.Code))
            //{
            //    if (model.AccountType.Equals((int)AccountType.TaiKhoanRieng))
            //    {
            //        var checkCodeUser = await _identityContext.AppUsers.FirstOrDefaultAsync(x => x.Code.Contains(model.Code));
            //        if (checkCodeUser != null)
            //        {
            //            result.Code = (int)HttpStatusCodes.CodeUserExisted;
            //            result.Message = "Mã nhân viên đã tồn tại. Vui lòng nhập lại.";
            //            return result;
            //        }
            //    }
            //}

            // role            
            var oldUserRoles = (from r in _identityContext.AppRoles
                                join ur in _identityContext.UserRoles
                                on r.Id equals ur.RoleId
                                //into gj
                                //from ur in gj.DefaultIfEmpty()
                                where ur.UserId == user.Id
                                select ur).ToArray();
            // change role
            if (!string.IsNullOrEmpty(model.RoleId?.Trim()))
            {
                var newRoleExists = await _roleManager.FindByIdAsync(model.RoleId?.Trim());
                if (newRoleExists != null)
                {
                    if (oldUserRoles.Any() == false)
                    {
                        var addRoleRes = await _userManager.AddToRoleAsync(user, newRoleExists.Name);
                        result.Data.RoleId = newRoleExists.Id.ToString();
                    }
                    else if (oldUserRoles.Any() == true && oldUserRoles.Any(x => x.RoleId.ToString().ToUpper() == model.RoleId?.Trim()?.ToUpper()) == false)
                    {
                        //var removeRes = await _userManager.RemoveFromRolesAsync(user, oldRoles);
                        _identityContext.UserRoles.RemoveRange(oldUserRoles);
                        var addRoleRes = await _userManager.AddToRoleAsync(user, newRoleExists.Name);
                        result.Data.RoleId = newRoleExists.Id.ToString();
                    }
                }
                else if (newRoleExists == null)
                {
                    result.Code = StatusCodes.Status400BadRequest;
                    result.Data.RoleId = "Quyền mới không còn tồn tại";
                }
            }
            // remove role            
            else if (string.IsNullOrEmpty(model.RoleId) && oldUserRoles.Any() == true)
            {
                _identityContext.UserRoles.RemoveRange(oldUserRoles);
                result.Data.RoleId = null;
                //var removeRes = await _userManager.RemoveFromRolesAsync(user, oldRoles);
                //if(removeRes.Succeeded == true)
                //{
                //    result.Data.RoleId = null;
                //}                
            }

            // department            
            // change department
            if (!string.IsNullOrEmpty(model.DepartmentId?.Trim()))
            {
                var newDepartmentExists = await _identityContext.Departments.FindAsync(Guid.Parse(model.DepartmentId?.Trim()));
                if (newDepartmentExists != null)
                {
                    result.Data.DepartmentId = user.DepartmentId = newDepartmentExists.Id.ToString();
                }
                else if (newDepartmentExists == null)
                {
                    result.Code = StatusCodes.Status400BadRequest;
                    result.Data.DepartmentId = "Phòng ban mới không còn tồn tại";
                }
            }
            // remove department
            else if (string.IsNullOrEmpty(model.DepartmentId))
            {
                result.Data.DepartmentId = user.DepartmentId = model.DepartmentId;
            }

            var oldUserName = user.UserName;

            user.UserName = model.Username?.Trim();
            user.Code = model.Code?.Trim();

            // Remove redundant whitespaces from fullname
            if (model.FullName.Trim().Contains(" "))
            {
                var getFullNameParts = model.FullName.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (getFullNameParts?.Any() == true)
                {
                    model.FullName = string.Join(" ", getFullNameParts);
                }
            }
            user.FullName = model.FullName;
            user.AccountType = (AccountType)model.AccountType;
            user.Status = (UserStatus)model.Status;

            user.LockPwdSetting = model.LockPwdSetting;
            user.LockPwTime = !string.IsNullOrEmpty(model.LockPwdTime) ? int.Parse(model.LockPwdTime) : null;
            user.LockActSetting = model.LockActSetting;
            user.LockActTime = !string.IsNullOrEmpty(model.LockActTime) ? int.Parse(model.LockActTime) : null;
            user.SecurityStamp = Guid.NewGuid().ToString();

            user.UpdatedAt = DateTime.Now;
            user.UpdatedBy = model.UpdatedBy;


            //Internal API để xóa tài khoản kiểm kê:
            if ((model.AccountType == (int)AccountType.TaiKhoanRieng || model.AccountType == (int)AccountType.TaiKhoanGiamSat) || model.Status != (int)UserStatus.Active)
            {
                var client = _restClientFactory.InventoryClient();
                var req = new RestRequest($"api/inventory/internal/inventory-account/{model.Id}");

                req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
                var response = await client.DeleteAsync(req);
            }

            //Internal API để cập nhật UserName tài khoản kiểm kê:
            if (oldUserName != model.Username)
            {
                var client = _restClientFactory.InventoryClient();
                var req = new RestRequest($"api/inventory/internal/inventory-account/{model.Id}/{model.Username}");

                req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
                var response = await client.PutAsync(req);
            }

            if (result.Code != StatusCodes.Status200OK)
            {
                result.Message = "Sửa người dùng không thành công";
                return result;
            }

            var updateRes = await _userManager.UpdateAsync(user);
            if (updateRes.Succeeded == false)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "Sửa người dùng không thành công";
                return result;
            }
            result.Code = StatusCodes.Status200OK;
            result.Message = "Cập nhật người dùng thành công";
            return await Task.FromResult(result);
        }
        public async Task<ResponseModel> UpdateAccountTypeUser(string userId, AccountType accountType, string updateBy)
        {
            var getUserInfo = _identityContext.Users.FirstOrDefault(x => x.Id.ToString().Contains(userId));
            if (getUserInfo == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy người dùng",
                };
            }
            getUserInfo.AccountType = accountType;
            getUserInfo.UpdatedAt = DateTime.Now;
            getUserInfo.UpdatedBy = updateBy;
            await _identityContext.SaveChangesAsync();

            //Internal API để xóa tài khoản kiểm kê:
            if (accountType == AccountType.TaiKhoanRieng || accountType == AccountType.TaiKhoanGiamSat)
            {
                var client = _restClientFactory.InventoryClient();
                var req = new RestRequest($"api/inventory/internal/inventory-account/{userId}");

                req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
                var result = await client.DeleteAsync(req);
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Cập nhật loại tài khoản thành công."
            };

        }

        public async Task<ResponseModel> UpdateStatusUser(string userId, UserStatus status, string updateBy)
        {
            var getUserInfo = _identityContext.Users.FirstOrDefault(x => x.Id.ToString().Contains(userId));
            if (getUserInfo == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy người dùng",
                };
            }
            getUserInfo.Status = status;
            getUserInfo.UpdatedAt = DateTime.Now;
            getUserInfo.UpdatedBy = updateBy;
            await _identityContext.SaveChangesAsync();

            //Internal API để xóa tài khoản kiểm kê:
            if (getUserInfo.AccountType == AccountType.TaiKhoanChung && status != UserStatus.Active)
            {
                var client = _restClientFactory.InventoryClient();
                var req = new RestRequest($"api/inventory/internal/inventory-account/{userId}");

                req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
                var result = await client.DeleteAsync(req);
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Cập nhật trạng thái thành công.",
            };
        }

        public bool IsValidStatus(int status)
        {
            var valueList = (UserStatus[])Enum.GetValues(typeof(UserStatus));
            if (valueList?.Any() == true)
            {
                foreach (var enumStatus in valueList)
                {
                    if (enumStatus == (UserStatus)status)
                    {
                        return enumStatus switch
                        {
                            UserStatus.Active => true,
                            UserStatus.LockByExpiredPassword => false,
                            UserStatus.LockByUnactive => false,
                            UserStatus.LockByAdmin => true,
                            UserStatus.Deleted => true,
                            _ => false,
                        };
                    }
                }
            }
            return false;
        }

        public async Task<ResponseModel> GetUserInfoMobileAfterLogin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.TheAccountIsNotExisted,
                    Message = "Tài khoản không tồn tại"
                };
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Thông tin chi tiết người dùng",
                Data = new UserInfoMobileAfterLoginDto
                {
                    Fullname = user?.FullName,
                    Code = user?.Code
                }
            };
        }

        public async Task<ResponseModel<LockUserListDto>> LockUsersByExpiredPasswordOrUnactiveAsync()
        {
            var result = new ResponseModel<LockUserListDto>(new LockUserListDto(new List<LockUserDto>()));
            var userListChanged = new List<AppUser>();
            var getUserList = _userManager.Users
                .Where(x => x.Status == UserStatus.Active && (x.LockPwdSetting || x.LockActSetting));
            if (getUserList?.Any() == true)
            {
                foreach (var user in getUserList)
                {
                    var lockRes = new LockUserDto() { UserId = user.Id.ToString().ToUpper(), OldStatus = (int)user.Status, OldStatusName = EnumHelper<UserStatus>.GetDisplayValue(user.Status) };
                    bool isExpiredPasswordByCheckTime = false, isUnactiveByCheckTime = false;
                    if (user.LockPwTime.HasValue && (double)user.LockPwTime.Value > 0 && (double)user.LockPwTime.Value <= 90)
                    {
                        if (user.UpdatedPasswordAt.HasValue)
                        {
                            isExpiredPasswordByCheckTime = (user.UpdatedPasswordAt.Value.AddDays((double)user.LockPwTime) <= DateTime.Now);
                        }
                        else
                        {
                            isExpiredPasswordByCheckTime = (user.UpdatedAt.HasValue ? user.UpdatedAt.Value.AddDays((double)user.LockPwTime) <= DateTime.Now : user.CreatedAt.Value.AddDays((double)user.LockPwTime) <= DateTime.Now);
                        }

                    }

                    if (user.LockActTime.HasValue && (double)user.LockActTime.Value > 0 && (double)user.LockActTime.Value <= 90)
                    {
                        if (user.LastActiveTime.HasValue)
                        {
                            isUnactiveByCheckTime = user.LastActiveTime.Value.AddDays((double)user.LockActTime) <= DateTime.Now;

                        }
                        else
                        {
                            isUnactiveByCheckTime = user.UpdatedAt.HasValue && user.UpdatedAt.Value.AddDays((double)user.LockActTime) <= DateTime.Now;
                        }
                    }

                    switch (user.Status)
                    {
                        case UserStatus.Active:
                            {
                                if (user.LockPwdSetting && isExpiredPasswordByCheckTime)
                                {
                                    lockRes.NewStatus = (int)UserStatus.LockByExpiredPassword;
                                    lockRes.NewStatusName = EnumHelper<UserStatus>.GetDisplayValue(UserStatus.LockByExpiredPassword);
                                    lockRes.Type = "LOCK";
                                    result.Data.LockUserListResult.Add(lockRes);
                                    user.Status = UserStatus.LockByExpiredPassword;
                                    userListChanged.Add(user);
                                }
                                else if (user.LockActSetting && isUnactiveByCheckTime)
                                {
                                    lockRes.NewStatus = (int)UserStatus.LockByUnactive;
                                    lockRes.NewStatusName = EnumHelper<UserStatus>.GetDisplayValue(UserStatus.LockByUnactive);
                                    lockRes.Type = "LOCK";
                                    result.Data.LockUserListResult.Add(lockRes);
                                    user.Status = UserStatus.LockByUnactive;
                                    userListChanged.Add(user);
                                }
                                break;
                            }
                    }
                }

                if (userListChanged.Any() == true)
                {
                    int lockCount = 0;
                    foreach (var user in userListChanged)
                    {
                        var updateUser = await _userManager.UpdateAsync(user);
                        if (updateUser.Succeeded)
                        {
                            var hasLockResult = result.Data.LockUserListResult.Any(x => x.UserId == user.Id.ToString().ToUpper());
                            if (hasLockResult)
                            {
                                var lockResult = result.Data.LockUserListResult.FirstOrDefault(x => x.UserId == user.Id.ToString().ToUpper());
                                lockResult.Success = true;
                                lockResult.UpdatedDate = DateTime.Now;
                                lockCount++;
                            }
                        }
                    }
                    result.Data.LockCount = lockCount;
                }
            }
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel<IEnumerable<FilterListUserDto>>> GetFilterUserListExport(UserListExportFilterDto filterModel)
        {
            //query:
            var query = new StringBuilder();
            query.Append("select u.Id as 'userId', u.UserName as 'userName', u.FullName as 'fullName', u.Code as 'code', u.Status as 'status' , u.AccountType as 'accountType', d.Id as 'departmentId', d.Name as 'departmentName', r.Id as 'roleId', r.Name as 'roleName', u.CreatedAt as 'createAt', " +
                "(select '(' + cr.UserName + ') ' + cr.FullName from AppUsers cr where cr.Id = u.CreatedBy) as 'createdBy', u.UpdatedAt as 'updatedAt', (select '(' + up.UserName + ') ' + up.FullName from AppUsers up where up.Id = u.CreatedBy) as 'updatedBy' from AppUsers as u" +
                                " left join AppUsers as cr on u.CreatedBy = cr.Id" +
                                " left join AppUsers as ud on u.UpdatedBy = ud.Id" +
                                " left join Departments as d on u.DepartmentId = d.Id" +
                                " left join AppUserRoles as ur on u.Id = ur.UserId" +
                                " left join AppRoles as r on ur.RoleId = r.Id" +
                            $" where (1=1)");

            if (!filterModel.UserName.IsNullOrEmpty())
            {
                query.Append($" and u.UserName like '%{filterModel.UserName}%'");
            }
            if (!filterModel.FullName.IsNullOrEmpty())
            {
                query.Append($" and u.FullName like '%{filterModel.FullName}%'");
            }
            if (!filterModel.Code.IsNullOrEmpty())
            {
                query.Append($" and u.Code like '%{filterModel.Code}%'");
            }

            //Danh sách phòng ban được phân quyền:
            var currUser = _httpContext.User;
            var departments = currUser.Claims.Where(x => x.Type == Constants.Permissions.DEPARTMENT_DATA_INQUIRY).Select(x => x.Value).ToList();

            //query departments:
            if (filterModel.DepartmentIds?.Any() == true)
            {
                var queryDepartment = filterModel.DepartmentIds.Select(x => $"'{x}'").ToList();
                query.Append($" and d.Id IN ({string.Join(",", queryDepartment)})");
            }
            else
            {
                var queryDepartment = departments.Select(x => $"'{x}'").ToList();
                query.Append($" and d.Id IN ({string.Join(",", queryDepartment)})");
            }

            //query roles:
            if (filterModel.RoleIds?.Any() == true)
            {
                var queryRole = filterModel.RoleIds.Select(x => $"'{x}'").ToList();
                query.Append($" and r.Id IN ({string.Join(",", queryRole)})");
            }

            //query status:
            if (filterModel.Status?.Any() == true)
            {
                var queryStatus = filterModel.Status.Select(x => $"'{x}'").ToList();
                query.Append($" and u.Status IN ({string.Join(",", queryStatus)})");
            }

            //query AccountType:
            if (filterModel.AccountTypes?.Any() == true)
            {
                var queryAccountType = filterModel.AccountTypes.Select(x => $"'{x}'").ToList();
                query.Append($" and u.AccountType IN ({string.Join(",", queryAccountType)})");
            }

            //Sắp xếp theo logic: tài khoản có UserName là Administrator lên đầu, còn lại sắp xếp theo ngày tạo sẽ giảm dần:
            query.Append($" order by case when u.Id like '%{Constants.Roles.ID_Administrator}%' then 0 else 1 end, u.CreatedAt desc");

            using var conn = _identityContext.Database.GetDbConnection();
            var result = await conn.QueryAsync<FilterListUserDto>(query.ToString());

            if (result.Any() == false)
            {
                return new ResponseModel<IEnumerable<FilterListUserDto>>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Không có bản ghi nào",
                };
            }
            return new ResponseModel<IEnumerable<FilterListUserDto>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Danh sách người dùng",
                Data = result
            };
        }

        [GeneratedRegex("^(?=.*[a-zA-Z])(?=.*[0-9]).+$")]
        private static partial Regex PasswordRegex();
        [GeneratedRegex("^[a-zA-Z0-9]+$")]
        private static partial Regex CodeRegex();
    }
}
