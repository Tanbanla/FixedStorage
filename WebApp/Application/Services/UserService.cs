namespace WebApp.Application.Services
{
    public partial class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<UserService> _logger;

        public UserService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnvironment, ILogger<UserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public ValidateDto<CreateUserErrorDto> ValidateCreateUserInput(CreateUserDto model)
        {
            var result = new ValidateDto<CreateUserErrorDto>(new CreateUserErrorDto()) { Data = new CreateUserErrorDto(), IsInvalid = false };
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
            // else if (model.Username.Trim().Contains(' '))
            // {
            // result.Data.Username = "Tên tài khoản đăng nhập có khoảng trắng là không hợp lệ";
            // result.IsInvalid = true;
            // }

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
                result.Data.Password = "Vui lòng nhập  Mật khẩu";
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
                result.Data.FullName = "Cho phép nhập Họ tên cũ tối đa 100 ký tự";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model?.Code))
            {
                result.Data.Code = "Vui lòng nhập Mã nhân viên";
                result.IsInvalid = true;
            }
            else if (!CodeRegex().IsMatch(model.Code.Trim()))
            {
                result.Data.Code = " Mã nhân viên không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.Code.Trim().Length > 8)
            {
                result.Data.Code = "Cho phép nhập Mã nhân viên tối đa 8 ký tự";
                result.IsInvalid = true;
            }

            if (!string.IsNullOrEmpty(model?.RoleId) && Guid.TryParse(model.RoleId.Trim(), out var _) == false)
            {
                result.Data.RoleId = "Id Quyền không hợp lệ.";
                result.IsInvalid = true;
            }

            if (!string.IsNullOrEmpty(model?.DepartmentId) && Guid.TryParse(model.DepartmentId.Trim(), out var _) == false)
            {
                result.Data.DepartmentId = "Id Phòng ban không hợp lệ.";
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
            else if (model.LockPwdSetting && !string.IsNullOrEmpty(model.LockPwdTime) && !int.TryParse(model.LockPwdTime, out var __))
            {
                result.Data.LockPwTime = "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.LockPwdSetting && !string.IsNullOrEmpty(model.LockPwdTime) && int.TryParse(model.LockPwdTime, out var pwTime))
            {
                if (pwTime > 90)
                {
                    result.Data.LockPwTime = "Vui lòng nhập không quá 90 ngày.";
                    result.IsInvalid = true;
                }

            }
            if (model.LockActSetting && string.IsNullOrEmpty(model.LockActTime))
            {
                result.Data.LockActTime = "Vui lòng nhập thời gian hiệu lực";
                result.IsInvalid = true;
            }
            else if (model.LockActSetting && !string.IsNullOrEmpty(model.LockActTime) && !int.TryParse(model.LockActTime, out var __))
            {
                result.Data.LockActTime = "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.LockActSetting && !string.IsNullOrEmpty(model.LockActTime) && int.TryParse(model.LockActTime, out var actTime))
            {
                if (actTime > 90)
                {
                    result.Data.LockActTime = "Vui lòng nhập không quá 90 ngày.";
                    result.IsInvalid = true;
                }

            }
            if (result.IsInvalid)
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Dữ liệu không hợp lệ";
            }
            return result;
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
            // else if (model.Username.Trim().Contains(' '))
            // {
            // result.Data.Username = "Tên tài khoản đăng nhập cũ có khoảng trắng là không hợp lệ";
            // result.IsInvalid = true;
            // }
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
                result.Data.FullName = "Họ và tên không đúng định dạng. Vui lòng thử lại.";
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
                result.Data.Code = " Mã nhân viên không đúng định dạng. Vui lòng thử lại.";
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
            else if (model.LockPwdSetting && !string.IsNullOrEmpty(model.LockPwdTime) && !int.TryParse(model.LockPwdTime, out var __))
            {
                result.Data.LockPwTime = "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.LockPwdSetting && !string.IsNullOrEmpty(model.LockPwdTime) && int.TryParse(model.LockPwdTime, out var pwTime))
            {
                if (pwTime > 90)
                {
                    result.Data.LockActTime = "Vui lòng nhập không quá 90 ngày.";
                    result.IsInvalid = true;
                }

            }

            if (model.LockActSetting && string.IsNullOrEmpty(model.LockActTime))
            {
                result.Data.LockActTime = "Vui lòng nhập thời gian hiệu lực";
                result.IsInvalid = true;
            }
            else if (model.LockActSetting && !string.IsNullOrEmpty(model.LockActTime) && !int.TryParse(model.LockActTime, out var __))
            {
                result.Data.LockActTime = "Thời gian hiệu lực không đúng định dạng. Vui lòng thử lại.";
                result.IsInvalid = true;
            }
            else if (model.LockActSetting && !string.IsNullOrEmpty(model.LockActTime) && int.TryParse(model.LockActTime, out var actTime))
            {
                if (actTime > 90)
                {
                    result.Data.LockActTime = "Vui lòng nhập không quá 90 ngày.";
                    result.IsInvalid = true;
                }

            }
            if (result.IsInvalid)
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Dữ liệu không hợp lệ";
            }
            return await Task.FromResult(result);
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
                        switch (enumStatus)
                        {
                            case UserStatus.Active:
                                return true;

                            case UserStatus.LockByExpiredPassword:
                                return false;

                            case UserStatus.LockByUnactive:
                                return false;

                            case UserStatus.LockByAdmin:
                                return true;

                            case UserStatus.Deleted:
                                return true;

                            default:
                                return false;
                        }
                    }
                }
            }
            return false;
        }

        public async Task<ValidateDto<UserListExportErrorResultDto>> ValidateExportUserListFilterAsync(UserListExportFilterDto model)
        {
            var result = new ValidateDto<UserListExportErrorResultDto>(new UserListExportErrorResultDto()) { IsInvalid = false };
            if (model == null)
            {
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Dữ liệu không hợp lệ";
                return await Task.FromResult(result);
            }

            if (!string.IsNullOrEmpty(model.AllDepartments) && int.TryParse(model.AllDepartments, out var _) == false
                ||
                int.TryParse(model.AllDepartments, out var allDpt) == true && allDpt != -1
                || model.DepartmentIds?.Any(x => Guid.TryParse(x, out var _)) == false
                )
            {
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
                result.Data.Departments = "Id Phòng ban không hợp lệ";
            }

            if (!string.IsNullOrEmpty(model.AllRoles) && int.TryParse(model.AllRoles, out var _) == false
                ||
                int.TryParse(model.AllRoles, out var allRoles) == true && allRoles != -1
                || model.RoleIds?.Any(x => Guid.TryParse(x, out var _)) == false)
            {
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
                result.Data.Roles = "Id Quyền không hợp lệ";
            }

            if (model.Status?.Any() == true
                && model.Status.Any(x => int.TryParse(x, out var status1) == false
                || int.TryParse(x, out var status2) && IsValidStatus(status2) == false))
            {
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
                result.Data.Roles = "Trạng thái không hợp lệ";
            }

            if (model.AccountTypes?.Any() == true
                && model.AccountTypes.Any(x => int.TryParse(x, out var accType1) == false
                || int.TryParse(x, out var accType2)
                &&
                ((AccountType[])Enum.GetValues(typeof(AccountType))).Any(x => x == (AccountType)accType2) == false
                ))
            {
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
                result.Data.Roles = "Loại tài khoản không hợp lệ";
            }
            return await Task.FromResult(result);
        }

        public async Task<byte[]> ExportFilteredUserListAsync(List<FilterListUserDto> dataModel)
        {
            try
            {
                var filePath = _hostingEnvironment.WebRootPath + @$"/FileExcelTemplate/TemplateExportUserList.xlsx";
                using (var memStream = new MemoryStream())
                {

                    using var source = File.Open(filePath, FileMode.Open);
                    source.CopyTo(memStream);

                    using (var package = new ExcelPackage(memStream))
                    {
                        var workSheet = package.Workbook.Worksheets.FirstOrDefault();
                        //workSheet.Cells.Style.WrapText = false;
                        //workSheet.Cells[1, 1, 1, 12].Style.Font.Bold = true;
                        var row = 3;
                        var index = 1;
                        dataModel.ForEach(x =>
                        {
                            if (x.UserId != default)
                                workSheet.Cells[row, 1].Value = index;

                            workSheet.Cells[row, 2].Value = x.UserName;
                            workSheet.Cells[row, 3].Value = x.FullName;
                            workSheet.Cells[row, 4].Value = x.Code;
                            workSheet.Cells[row, 5].Value = EnumHelper<UserStatus>.GetDisplayValue((UserStatus)x.Status);
                            workSheet.Cells[row, 6].Value = EnumHelper<AccountType>.GetDisplayValue((AccountType)x.AccountType);
                            workSheet.Cells[row, 7].Value = x.DepartmentName;
                            workSheet.Cells[row, 8].Value = x.RoleName;
                            workSheet.Cells[row, 9].Value = x.CreatedAt.HasValue ? x.CreatedAt.Value.ToString("dd-MM-yyyy") : string.Empty;
                            workSheet.Cells[row, 10].Value = x.CreatedBy;
                            workSheet.Cells[row, 11].Value = x.UpdatedAt.HasValue ? x.UpdatedAt.Value.ToString("dd-MM-yyyy") : string.Empty;
                            workSheet.Cells[row, 12].Value = x.UpdatedBy;
                            row++;
                            index++;
                        });
                        return await Task.FromResult(package.GetAsByteArray());
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error:{0};{1}", ex.Message, ex.InnerException?.Message);
                return null;
            }
        }

        [GeneratedRegex("^(?=.*[a-zA-Z])(?=.*[0-9]).+$")]
        private static partial Regex PasswordRegex();
        [GeneratedRegex("^[a-zA-Z0-9]+$")]
        private static partial Regex CodeRegex();
    }
}
