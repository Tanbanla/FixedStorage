using static BIVN.FixedStorage.Services.Common.API.Constants;

namespace WebApp.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AccountService(IHttpContextAccessor httpContextAccessor,
                            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        #region Login
        public ValidateDto<LoginErrorDto> ValidateLoginInput(LoginDto model)
        {
            var result = new ValidateDto<LoginErrorDto>(new LoginErrorDto());
            // Validate model
            if (string.IsNullOrEmpty(model.Username))
            {
                result.Data.Username = "Vui lòng nhập Tên tài khoản đăng nhập";
                result.Data.Password = string.Empty;
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.Username.Trim().Contains(' '))
            {
                result.Data.Username = "Tên tài khoản đăng nhập có khoảng trắng là không hợp lệ";
                result.Data.Password = string.Empty;
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.Username.Trim().Length < 1 || model.Username.Trim().Length > 15)
            {
                result.Data.Username = "Cho phép nhập Tên tài khoản đăng nhập trong khoảng 1 đến 15 ký tự";
                result.Data.Password = string.Empty;
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                result.Data.Password = "Vui lòng nhập Mật khẩu";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.Password.Trim().Contains(' '))
            {
                result.Data.Password = "Mật khẩu có khoảng trắng là không hợp lệ";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.Password.Trim().Length < 8 || model.Password.Trim().Length > 15)
            {
                result.Data.Password = "Cho phép nhập Mật khẩu trong khoảng 8 đến 15 ký tự";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (!string.IsNullOrEmpty(model.DeviceId) && model.DeviceId.Trim().Length > 254)
            {
                result.Data.Username = result.DeviceId = "Cho phép nhập Id thiết bị nhỏ hơn hoặc bằng 254 ký tự";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (!string.IsNullOrEmpty(result.Data.Username) || !string.IsNullOrEmpty(result.Data.Password) || !string.IsNullOrEmpty(result.DeviceId))
            {
                result.IsInvalid = true;
                result.Message = "Dữ liệu không hợp lệ";
                return result;
            }
            result.IsInvalid = false;
            return result;
        }
        #endregion

        public ValidateDto<ChangePasswordErrorDto> ValidateChangePasswordInput(ChangePasswordDto model)
        {
            var result = new ValidateDto<ChangePasswordErrorDto>(new ChangePasswordErrorDto()) { IsInvalid = false };
            if (string.IsNullOrEmpty(model?.UserId) || !Guid.TryParse(model.UserId, out var userId))
            {
                result.Data.OldPassword = "Id Tài khoản không hợp lệ.";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (string.IsNullOrEmpty(model?.OldPassword))
            {
                result.Data.OldPassword = "Vui lòng nhập mật khẩu cũ.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.OldPassword.Trim().Contains(' '))
            {
                result.Data.OldPassword = "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (!Regex.IsMatch(model.OldPassword.Trim(), @"^(?=.*[a-zA-Z])(?=.*[0-9]).+$"))
            {
                result.Data.OldPassword = "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.OldPassword.Trim().Length < 8 || model.OldPassword.Trim().Length > 15)
            {
                result.Data.OldPassword = "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (string.IsNullOrEmpty(model?.NewPassword))
            {
                result.Data.NewPassword = "Vui lòng nhập mật khẩu mới.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.NewPassword.Trim().Contains(' '))
            {
                result.Data.NewPassword = "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (!Regex.IsMatch(model.NewPassword.Trim(), @"^(?=.*[a-zA-Z])(?=.*[0-9]).+$"))
            {
                result.Data.NewPassword = " Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.NewPassword.Trim().Length < 8 || model.NewPassword.Trim().Length > 15)
            {
                result.Data.NewPassword = "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (string.IsNullOrEmpty(model?.NewPasswordConfirm))
            {
                result.Data.NewPasswordConfirm = "Vui lòng nhập xác nhận mật khẩu mới.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.NewPasswordConfirm.Trim().Contains(' '))
            {
                result.Data.NewPasswordConfirm = "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (!Regex.IsMatch(model.NewPasswordConfirm.Trim(), @"^(?=.*[a-zA-Z])(?=.*[0-9]).+$"))
            {
                result.Data.NewPasswordConfirm = " Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.NewPasswordConfirm.Trim().Length < 8 || model.NewPasswordConfirm.Trim().Length > 15)
            {
                result.Data.NewPasswordConfirm = "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (model.OldPassword == model.NewPassword)
            {
                result.Message = "Mật khẩu mới không được trùng mật với mật khẩu hiện tại của bạn.";
                result.Code = (int)HttpStatusCodes.OldPasswordDuplicateWithNewPassword;
                result.IsInvalid = true;
            }
            else if (model.NewPassword != model.NewPasswordConfirm)
            {
                result.Data.NewPasswordConfirm = "Mật khẩu mới và xác nhận mật khẩu mới phải trùng khớp với nhau.";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (!string.IsNullOrEmpty(result.Data.OldPassword) || !string.IsNullOrEmpty(result.Data.NewPassword) || !string.IsNullOrEmpty(result.Data.NewPasswordConfirm))
            {
                result.IsInvalid = true;
            }
            return result;
        }

        public void SetUserCookies(LoginResultDto userInfo)
        {
            var options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(2); // Cookie expiration date
            options.IsEssential = true;
            options.Secure = false; // The cookie is only sent over HTTPS
            options.HttpOnly = true;
            options.Domain = "";
            TryAddCookie(UserClaims.Token, userInfo?.Token ?? string.Empty, options);
            TryAddCookie(UserClaims.UserId, userInfo?.UserId?.ToString() ?? string.Empty, options);
            TryAddCookie(UserClaims.SecurityStamp, userInfo?.SecuriryStamp ?? string.Empty, options);
            TryAddCookie(UserClaims.Username, userInfo?.Username ?? string.Empty, options);
            TryAddCookie(UserClaims.FullName, userInfo?.FullName ?? string.Empty, options);
            TryAddCookie(UserClaims.RoleName, userInfo?.RoleName ?? string.Empty, options);
            TryAddCookie(UserClaims.DepartmentName, userInfo?.DepartmentName ?? string.Empty, options);
            TryAddCookie(UserClaims.Avatar, userInfo?.Avatar ?? string.Empty, options);
        }

        public bool TryAddCookie(string key, string value, CookieOptions options)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            //if (string.IsNullOrEmpty(value))
            //{
            //    return false;
            //}

            var cookieValue = _httpContextAccessor.HttpContext.Request.Cookies[key];
            //Nếu cookie bằng null
            if (string.IsNullOrEmpty(cookieValue))
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, options);
            }
            else
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(key, new CookieOptions { Expires = DateTime.Now.AddMinutes(-1) });
                _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, options);
            }
            return true;
        }

        public void DeleteUserCookies()
        {
            var options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(-1); // Cookie expiration date
            options.IsEssential = true;
            options.Secure = false; // The cookie is only sent over HTTPS
            options.HttpOnly = true;
            options.Domain = "";

            TryDeleteCookie(UserClaims.Token, options);
            TryDeleteCookie(UserClaims.UserId, options);
            TryDeleteCookie(UserClaims.SecurityStamp, options);
            TryDeleteCookie(UserClaims.Username, options);
            TryDeleteCookie(UserClaims.FullName, options);
            TryDeleteCookie(UserClaims.DepartmentName, options);
            TryDeleteCookie(UserClaims.RoleName, options);
            TryDeleteCookie(UserClaims.Avatar, options);
            TryDeleteCookie(UserClaims.DeviceId, options);

            //TryDeleteCookie(UserClaims.Code, options);
            //TryDeleteCookie(UserClaims.Email, options);
            //TryDeleteCookie(UserClaims.Phone, options);
            //TryDeleteCookie(UserClaims.AccountType, options);
            //TryDeleteCookie(UserClaims.DepartmentId, options);
            //TryDeleteCookie(UserClaims.RoleId, options);
            //TryDeleteCookie(UserClaims.ExpiredDate, options);            
        }

        public bool TryDeleteCookie(string key, CookieOptions options)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            //if (string.IsNullOrEmpty(value))
            //{
            //    return false;
            //}

            _httpContextAccessor.HttpContext.Response.Cookies.Delete(key, new CookieOptions { Expires = DateTime.Now.AddMinutes(-1) });
            return true;
        }

        public ValidateDto<ResetPasswordErrorDto> ValidateResetPasswordInput(ResetPasswordDto model)
        {
            var result = new ValidateDto<ResetPasswordErrorDto>(new ResetPasswordErrorDto()) { IsInvalid = false };
            if (string.IsNullOrEmpty(model?.UserId) || !Guid.TryParse(model.UserId, out var userId))
            {
                result.Data.NewPassword = "Id Tài khoản không hợp lệ.";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (string.IsNullOrEmpty(model?.NewPassword))
            {
                result.Data.NewPassword = "Vui lòng nhập Mật khẩu mới.";
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (model.NewPassword.Trim().Contains(' ')
                || !Regex.IsMatch(model.NewPassword.Trim(), @"^(?=.*[a-zA-Z])(?=.*[0-9]).+$")
                || model.NewPassword.Trim().Length < 8
                || model.NewPassword.Trim().Length > 15)
            {
                result.Data.NewPassword = "Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
                result.Code = StatusCodes.Status400BadRequest;
            }

            if (!string.IsNullOrEmpty(result.Data.NewPassword))
            {
                result.IsInvalid = true;
            }
            return result;
        }
    }
}
