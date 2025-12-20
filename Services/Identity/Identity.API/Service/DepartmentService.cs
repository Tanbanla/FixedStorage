namespace BIVN.FixedStorage.Identity.API.Service
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ILogger<DepartmentService> _logger;
        private readonly IdentityContext _identityContext;
        private readonly HttpContext _httpContext;

        public DepartmentService(ILogger<DepartmentService> logger,
                                IdentityContext identityContext
                                )
        {
            _logger = logger;
            _identityContext = identityContext;
        }

        public async Task<ResponseModel<IEnumerable<DepartmentDto>>> GetAllDepartmentAsync()
        {
            var departments = await (from d in _identityContext.Departments.Select(x => new { Id = x.Id.ToString(), x.Name, x.ManagerId, CreateAt = x.CreatedAt })
                                     join u in _identityContext.AppUsers.Select(x => new { Id = x.Id.ToString(), x.FullName, x.DepartmentId })
                                         on d.Id equals u.DepartmentId into duGroup
                                     join m in _identityContext.AppUsers.Select(x => new { Id = x.Id.ToString(), x.FullName, x.DepartmentId })
                                         on d.ManagerId equals m.Id into mdGroup
                                     from manager in mdGroup.DefaultIfEmpty()
                                     orderby d.CreateAt ascending
                                     select new DepartmentDto
                                     {
                                         Id = d.Id,
                                         Name = d.Name,
                                         ManagerId = manager.Id,
                                         ManagerName = manager.FullName,
                                         Members = duGroup.Count(),
                                         CreateAt = d.CreateAt
                                     }).ToListAsync();

            if (departments?.Any() == false)
            {
                return new ResponseModel<IEnumerable<DepartmentDto>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu."
                };
            }

            return new ResponseModel<IEnumerable<DepartmentDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = departments
            };
        }
        //For Moblie
        public async Task<ResponseModel<DepartmentDto>> GetDepartmentInfo(Guid Id)
        {
            var result = await _identityContext.Departments.FirstOrDefaultAsync(x => x.Id == Id);
            if (result == null)
            {
                return new ResponseModel<DepartmentDto>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy phòng ban."
                };
            }
            return new ResponseModel<DepartmentDto>
            {
                Code = StatusCodes.Status200OK,
                Data = new DepartmentDto { ManagerId = result.ManagerId.ToString(), Name = result.Name, Id = result.Id.ToString() }
            };
        }
        public async Task<ResponseModel<IEnumerable<SelectUserDepartmentViewModel>>> UserListAsync()
        {
            var users = await _identityContext.AppUsers.Where(x => x.Status == UserStatus.Active).Select(x => new SelectUserDepartmentViewModel
            {
                Id = x.Id.ToString(),
                Name = x.FullName,
            }).ToListAsync();

            if (users?.Any() == false)
            {
                return new ResponseModel<IEnumerable<SelectUserDepartmentViewModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu."
                };
            }

            return new ResponseModel<IEnumerable<SelectUserDepartmentViewModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = users
            };
        }
        public async Task<ResponseModel<bool>> CheckExistDepartmentNameAsync(string name)
        {
            var convertedName = name.TrimBetWeen().ToLower();
            var departments = await _identityContext.Departments.Select(x => x.Name).ToListAsync();
            var existDepartment = departments?.Any(x => x.TrimBetWeen().ToLower() == convertedName);
            if (existDepartment != null && existDepartment == true)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status200OK,
                    Data = true,
                    Message = "Phòng ban này đã tồn tại, vui lòng nhập lại."
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = false,
                Message = "Tên phòng ban hợp lệ"
            };
        }
        public async Task<ResponseModel<bool>> CheckExistEditNameAsync(string departmentId, string name)
        {
            var convertedDepartmentId = Guid.Parse(departmentId);
            var editDepartment = await _identityContext.Departments.FirstOrDefaultAsync(x => x.Id == convertedDepartmentId);

            if (string.Equals(editDepartment?.Name.TrimBetWeen(), name.TrimBetWeen(), StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseModel<bool>
                {
                    Data = false,
                    Code = StatusCodes.Status200OK,
                    Message = "Tên chỉnh sửa hợp lệ"
                };
            }
            else
            {
                var existNameResult = await CheckExistDepartmentNameAsync(name);
                return existNameResult;
            }
        }
        public async Task<ResponseModel<bool>> CheckEmptyDepartmentAsync(string departmentId)
        {
            var convertedDepartmentId = departmentId.ToLower();
            var anyDepartment = await _identityContext.AppUsers.AnyAsync(x => x.DepartmentId.ToLower() == convertedDepartmentId);
            if (!anyDepartment)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status200OK,
                    Data = true,
                    Message = "Phòng ban trống."
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status400BadRequest,
                Data = false,
                Message = "Bạn không thể xóa phòng ban này do vẫn còn nhân viên."
            };
        }

        public async Task<ResponseModel<bool>> CreateAsync(string departmentName, string creatorId,

#nullable enable
            string? userId
#nullable disable

            )
        {
            var resultExistName = await CheckExistDepartmentNameAsync(departmentName);
            if (resultExistName?.Data == true)
            {
                return resultExistName;
            }

            Department department = new Department
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Name = departmentName,
                CreatedBy = creatorId
            };

            if (!string.IsNullOrEmpty(userId))
            {
                department.ManagerId = userId;
            }

            try
            {
                _identityContext.Departments.Add(department);
                await _identityContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Có lỗi khi thực hiện lưu phòng ban", ex.Message);

                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Có lỗi khi thực hiện lưu phòng ban"
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Thêm mới phòng ban thành công."
            };
        }
        public async Task<ResponseModel<bool>> EditAsync(string departmentId, string departmentName, string creatorId, string userId)
        {
            var convertedDepartmentId = Guid.Parse(departmentId);
            var Department = await _identityContext.Departments.FirstOrDefaultAsync(x => x.Id == convertedDepartmentId);
            //Check null
            if (Department == null)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Không tìm thấy phòng ban"
                };
            }

            try
            {
                Department.Name = departmentName;
                Department.UpdatedAt = DateTime.Now;
                Department.UpdatedBy = creatorId;

                if (!string.IsNullOrEmpty(userId))
                {
                    Department.ManagerId = userId;
                }

                await _identityContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Có lỗi khi thực hiện lưu phòng ban", ex.Message);

                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Có lỗi khi thực hiện lưu phòng ban"
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Chỉnh sửa phòng ban thành công."
            };
        }
        public async Task<ResponseModel<bool>> DeleteAsync(string departmentId, string userId)
        {
            var convertedDepartmentId = Guid.Parse(departmentId);
            var Department = await _identityContext.Departments.FirstOrDefaultAsync(x => x.Id == convertedDepartmentId);

            if (Department == null)
            {
                return new ResponseModel<bool>
                {
                    Data = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tìm thấy phòng ban",
                };
            }

            try
            {
                //Xóa phòng ban
                Department.DeletedAt = DateTime.Now;
                Department.DeletedBy = userId;
                //Soft delete
                Department.IsDeleted = true;

                #region side effect: Khi xóa phòng ban thì xóa các role xem dữ liệu theo id phòng ban bị xóa này
                var findClaims = await _identityContext.RoleClaims.Where(x => x.ClaimType == Constants.Permissions.DEPARTMENT_DATA_INQUIRY
                                                                       && x.ClaimValue.ToLower() == departmentId.ToLower()).ToListAsync();
                _identityContext.RoleClaims.RemoveRange(findClaims);
                #endregion

                await _identityContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện xóa phòng ban", ex);

                return new ResponseModel<bool>
                {
                    Data = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Có lỗi khi thực hiện xóa phòng ban",
                };
            }

            return new ResponseModel<bool>
            {
                Data = true,
                Code = StatusCodes.Status200OK,
                Message = "Xóa phòng ban thành công.",
            };
        }

        public async Task<ResponseModel<bool>> AssignUserToDepartmentAsync(string departmentId, string userId,string updatedBy)
        {
            var convertedUserId = Guid.Parse(userId);
            var getUser = await _identityContext.Users.FirstOrDefaultAsync(x => x.Id == convertedUserId);
            if(getUser != null)
            {
                if (getUser.DepartmentId != departmentId)
                {
                    getUser.DepartmentId = departmentId;
                    getUser.UpdatedAt= DateTime.Now;
                    getUser.UpdatedBy = updatedBy;
                    await _identityContext.SaveChangesAsync();

                    return new ResponseModel<bool>
                    {
                        Code = StatusCodes.Status200OK,
                        Data = true,
                        Message = "Chỉnh sửa phòng ban thành công."
                    };
                }
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status404NotFound,
                Data = true,
                Message = "Không tìm thấy thông tin người dùng"
            };
        }
    }
}
