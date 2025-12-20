using System.Security.Cryptography;
using BIVN.FixedStorage.Services.Common.API;
using Microsoft.OpenApi.Extensions;

namespace BIVN.FixedStorage.Identity.API.Service
{
    public partial class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IdentityContext _context;
        private readonly DeviceTokenContext _deviceTokenContext;
        private readonly HttpContext _httpContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentityService> _logger;
        private readonly RestClientFactory _restClientFactory;

        public IdentityService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IWebHostEnvironment hostingEnvironment, IdentityContext context, IConfiguration configuration
             , DeviceTokenContext deviceTokenContext, IHttpContextAccessor httpContextAccessor, ILogger<IdentityService> logger,
            RestClientFactory restClientFactory
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _hostingEnvironment = hostingEnvironment;
            _context = context;
            _configuration = configuration;
            _deviceTokenContext = deviceTokenContext;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _restClientFactory = restClientFactory;
        }

        #region Login

        public async Task<ValidateDto<LoginErrorDto>> ValidateLoginInput(LoginDto model)
        {
            var result = new ValidateDto<LoginErrorDto>(new LoginErrorDto());
            if (model == null)
            {
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Dữ liệu không hợp lệ";
                return await Task.FromResult(result);
            }

            // Validate model
            if (string.IsNullOrEmpty(model.Username))
            {
                result.Data.Username = "Vui lòng nhập tài khoản đăng nhập";
                result.Data.Password = string.Empty;
                result.Code = StatusCodes.Status400BadRequest;
            }
            else if (!StringHelper.HasOnlyNormalVietnameseCharacters(model.Username.Trim()))
            {
                result.Data.Username = "Tài khoản đăng nhập không đúng định dạng. Vui lòng thử lại.";
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
            else if (!PasswordRegex().IsMatch(model.Password.Trim()))
            {
                result.Data.Password = " Mật khẩu không hợp lệ. Yêu cầu phải có ít nhất 1 ký tự chữ và 1 ký tự chữ số";
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
                return await Task.FromResult(result);
            }
            result.IsInvalid = false;
            return await Task.FromResult(result);
        }

        public async Task<CheckUserInfoDto> GetUserInfoFromLoginAsync(LoginDto model, bool isForRefreshToken = false)
        {
            var result = new CheckUserInfoDto(new ValidateDto<LoginErrorDto>(new LoginErrorDto()), new UserInfoFromLoginDto(model.DeviceId));
            var user = await _userManager.FindByUserNameAsync(model.Username);
            if (user == null)
            {
                result.Validation.IsInvalid = true;
                result.Validation.Code = (int)HttpStatusCodes.TheAccountIsNotExisted;
                result.Validation.Message = result.Validation.Data.Username = "Tài khoản không tồn tại";
                return await Task.FromResult(result);
            }

            #region Check user status

            switch (user.Status)
            {
                case UserStatus.Active:
                    break;

                case UserStatus.LockByExpiredPassword:
                    {
                        result.Validation.IsInvalid = true;
                        result.Validation.Message = result.Validation.Data.Username = "Tài khoản ở trạng thái bị khóa vì mật khẩu hết hạn";
                        result.Validation.Code = (int)HttpStatusCodes.LockByNotUpdatePassword;
                        return await Task.FromResult(result);
                    }

                case UserStatus.LockByUnactive:
                    {
                        result.Validation.IsInvalid = true;
                        result.Validation.Message = result.Validation.Data.Username = "Tài khoản ở trạng thái bị khóa vì không tương tác";
                        result.Validation.Code = (int)HttpStatusCodes.LockByUnactive;
                        return await Task.FromResult(result);
                    }

                case UserStatus.LockByAdmin:
                    {
                        result.Validation.IsInvalid = true;
                        result.Validation.Message = result.Validation.Data.Username = "Tài khoản ở trạng thái bị khóa bởi Admin";
                        result.Validation.Code = (int)HttpStatusCodes.LockByAdmin;
                        return await Task.FromResult(result);
                    }

                case UserStatus.Deleted:
                    {
                        result.Validation.IsInvalid = true;
                        result.Validation.Message = result.Validation.Data.Username = "Tài khoản ở trạng thái đã bị xóa";
                        result.Validation.Code = (int)HttpStatusCodes.ThisAccountHasBeenDeleted;
                        return await Task.FromResult(result);
                    }
            }

            if (user.AccountType.HasValue)
            {
                switch (user.AccountType.Value)
                {
                    case AccountType.TaiKhoanChung:
                        result.UserInfo.AccountType = nameof(AccountType.TaiKhoanChung);
                        break;

                    case AccountType.TaiKhoanRieng:
                        result.UserInfo.AccountType = nameof(AccountType.TaiKhoanRieng);
                        break;

                    case AccountType.TaiKhoanGiamSat:
                        result.UserInfo.AccountType = nameof(AccountType.TaiKhoanGiamSat);
                        break;
                }
            }

            if (user.AccountType == AccountType.TaiKhoanRieng || user.AccountType == AccountType.TaiKhoanGiamSat)
            {
                var AllowOverrideLoginPersonalAccount = _httpContext.Request.Headers["AllowOverrideLoginPersonalAccount"].FirstOrDefault()?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(AllowOverrideLoginPersonalAccount) && AllowOverrideLoginPersonalAccount == "YES")
                {
                    var deviceTokenExists = await _deviceTokenContext.DeviceTokens?.FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.UserId) && x.UserId.ToUpper() == user.Id.ToString().ToUpper());
                    if (deviceTokenExists != null)
                    {
                        _deviceTokenContext.DeviceTokens.Remove(deviceTokenExists);
                        await _deviceTokenContext.SaveChangesAsync();
                    }
                }
                else if (await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.UserId.ToUpper() == user.Id.ToString().ToUpper()
                                            && x.Status == true && x.TokenExpireTime > DateTime.Now) && !isForRefreshToken)
                {
                    result.Validation.IsInvalid = true;
                    result.Validation.Message = result.Validation.Data.Username = "Tài khoản đã được đăng nhập ở thiết bị khác";
                    result.Validation.Code = (int)HttpStatusCodes.ThisAccountHasBeenLoggedIntoOtherDeviceBefore;
                    return await Task.FromResult(result);
                }
            }

            #endregion Check user status

            #region role, permissions

            var roleIdList = new List<string>();
            var roleNameList = new List<string>();
            var roleNames = await _userManager.GetRolesAsync(user);
            if (roleNames?.Any() == true)
            {
                foreach (var roleName in roleNames)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role == null)
                        continue;
                    roleIdList.Add(role.Id.ToString().ToUpper());
                    roleNameList.Add(role.Name);
                    //userInfo.RoleNames = role.Id.ToString();
                    var getRoleClaims = _context.RoleClaims.Where(x => x.RoleId == role.Id);
                    if (getRoleClaims?.Any() == true)
                    {
                        result.UserInfo.RoleClaims = getRoleClaims.Select(x => new RoleClaimDto
                        {
                            RoleId = role.Id.ToString(),
                            RoleName = role.Name,
                            ClaimType = x.ClaimType,
                            ClaimValue = x.ClaimValue
                        })?.ToList();
                    }
                }
            }

            // User still can login system even though does not have any role or does not any permission
            if (roleIdList?.Any() == false)
            {
                result.Validation.IsInvalid = true;
                result.Validation.Code = (int)HttpStatusCodes.HasNotAnyRoles;
                result.Validation.Message = result.Validation.Data.Username = "Tài khoản không tồn tại quyền";
                return result;
            }
            if (result.UserInfo.RoleClaims?.Any() == false)
            {
                result.Validation.IsInvalid = true;
                result.Validation.Code = (int)HttpStatusCodes.HasNotAnyRolePermissions;
                result.Validation.Message = result.Validation.Data.Username = "Tài khoản không tồn tại quyền chi tiết";
                return result;
            }

            // User has role and has permissions
            result.UserInfo.RoleId = roleIdList?.Any() == true ? string.Join(",", roleIdList) : string.Empty;
            result.UserInfo.RoleName = roleNameList?.Any() == true ? string.Join(",", roleNameList) : string.Empty;

            if (!string.IsNullOrEmpty(user.DepartmentId))
            {
                if (Guid.TryParse(user.DepartmentId, out var _validDepartmentIdValue))
                {
                    var department = await _context.Departments.FindAsync(_validDepartmentIdValue);
                    result.UserInfo.DepartmentName = department != null ? department.Name : string.Empty;
                }
            }

            // Login by mobile
            if (!string.IsNullOrEmpty(model.DeviceId))
            {
                // User does not have 'Mobile access' permission
                if (result.UserInfo.RoleClaims?.Any(x => x.RoleId.ToUpper() == result.UserInfo.RoleId.ToUpper() && x.ClaimType == Constants.Permissions.MOBILE_ACCESS) == false)
                {
                    result.Validation.IsInvalid = true;
                    result.Validation.Code = StatusCodes.Status403Forbidden;
                    result.Validation.Message = result.Validation.Data.Username = "Không có quyền truy cập App Mobile";
                    return await Task.FromResult(result);
                }
                // User has 'Mobile access' permission but not has 'MC' or 'PBC' permission
                else if (result.UserInfo.RoleClaims?.Any(x => x.RoleId.ToUpper() == result.UserInfo.RoleId.ToUpper() && Constants.Permissions.MobilePermissionList.Contains(x.ClaimType)) == false)
                {
                    result.Validation.IsInvalid = true;
                    result.Validation.Code = StatusCodes.Status403Forbidden;
                    result.Validation.Message = result.Validation.Data.Username = "Không có quyền truy cập của nghiệp vụ MC hoặc PCB";
                    return await Task.FromResult(result);
                }

            }

            // Login by Website
            else
            {
                // User does not have 'Website access' permission
                if (result.UserInfo.RoleClaims?.Any(x => x.RoleId.ToUpper() == result.UserInfo.RoleId && x.ClaimType == Constants.Permissions.WEBSITE_ACCESS) == false)
                {
                    result.Validation.IsInvalid = true;
                    result.Validation.Code = StatusCodes.Status403Forbidden;
                    result.Validation.Message = result.Validation.Data.Username = "Tài khoản không có quyền truy cập hệ thống.";
                    return await Task.FromResult(result);
                }
                result.UserInfo.DeviceId = model.DeviceId;
            }

            #endregion role, permissions

            result.UserInfo.User = user;
            return await Task.FromResult(result);
        }

        public void CheckPasswordExpiration(CheckUserInfoDto checkUser)
        {
            #region remaining days before password expire notification
            if (checkUser.UserInfo.User.LockPwdSetting && checkUser.UserInfo.User.LockPwTime.HasValue)
            {
                if (checkUser.UserInfo.User.LastActiveTime.HasValue)
                {
                    // the specific date that is password expiration date

                    var passwordExpirationDate = checkUser.UserInfo.User.UpdatedPasswordAt.HasValue
                        ? checkUser.UserInfo.User.UpdatedPasswordAt.Value.AddDays(checkUser.UserInfo.User.LockPwTime.Value)
                        : checkUser.UserInfo.User.CreatedAt.Value.AddDays(checkUser.UserInfo.User.LockPwTime.Value);

                    // get remaining days before password expire                    
                    var remainingDaysBeforePasswordExpire = (passwordExpirationDate.Date - DateTime.Now.Date).TotalDays;
                    // get date diff of current login and last login
                    var dateDiff_FromLastLoginToCurrentLogin = (DateTime.Now.Date - checkUser.UserInfo.User.LastActiveTime.Value.Date).TotalDays;

                    // if remaining days equals or less than 10 days
                    // current login is the first login in each days of remaining days, current login must be next day of last login date
                    if (remainingDaysBeforePasswordExpire >= 0 && remainingDaysBeforePasswordExpire <= 10 && dateDiff_FromLastLoginToCurrentLogin == 1)
                    {
                        checkUser.UserInfo.ExpiredPasswordNotification = $"Mật khẩu sắp hết hạn trong vòng {remainingDaysBeforePasswordExpire} ngày tới. Vui lòng thay đổi mật khẩu để không bị khóa tài khoản.";
                    }
                    // password is expired, user can not change password
                    else if (remainingDaysBeforePasswordExpire < 0)
                    {
                        checkUser.Validation.IsInvalid = true;
                        checkUser.Validation.Code = (int)HttpStatusCodes.LockByNotUpdatePassword;
                        checkUser.Validation.Message = $"Tài khoản ở trạng thái bị khóa vì mật khẩu hết hạn";
                    }
                }
                // new user still does not have a first login
                else
                {
                    // the specific date that is password expiration date
                    var passwordExpirationDate = checkUser.UserInfo.User.UpdatedPasswordAt.HasValue
                        ? checkUser.UserInfo.User.UpdatedPasswordAt.Value.AddDays(checkUser.UserInfo.User.LockPwTime.Value)
                        : checkUser.UserInfo.User.CreatedAt.Value.AddDays(checkUser.UserInfo.User.LockPwTime.Value);

                    // get remaining days before password expire
                    var remainingDaysBeforePasswordExpire = (passwordExpirationDate.Date - DateTime.Now.Date).TotalDays;

                    // if remaining days equals or less than 10 days
                    // current login is the first login in each days of remaining days, current login must be next day of last login date
                    if (remainingDaysBeforePasswordExpire >= 0 && remainingDaysBeforePasswordExpire <= 10)
                    {
                        checkUser.UserInfo.ExpiredPasswordNotification = $"Mật khẩu sắp hết hạn trong vòng {remainingDaysBeforePasswordExpire} ngày tới. Vui lòng thay đổi mật khẩu để không bị khóa tài khoản.";
                    }
                    // password is expired, user can not change password
                    else if (remainingDaysBeforePasswordExpire < 0)
                    {
                        checkUser.Validation.IsInvalid = true;
                        checkUser.Validation.Code = (int)HttpStatusCodes.LockByNotUpdatePassword;
                        checkUser.Validation.Message = $"Tài khoản ở trạng thái bị khóa vì mật khẩu hết hạn";
                    }
                }
            }
            #endregion remaining days before password expire notification                  
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<ResponseModel<LoginResultDto>> GenerateJwtTokenAsync(UserInfoFromLoginDto userInfo)
        {
            //var expiredDate = !string.IsNullOrEmpty(userInfo.DeviceId) ? DateTime.Now.AddDays(5) : !string.IsNullOrEmpty(_configuration["JwtTokens:ExpiredDate"]) && Int32.TryParse(_configuration["JwtTokens:ExpiredDate"], out int days) ? DateTime.Now.AddDays(days) : DateTime.Now.AddDays(5);
            var expiredDate = !string.IsNullOrEmpty(userInfo.DeviceId) ? DateTime.Now.AddDays(10) : !string.IsNullOrEmpty(_configuration["JwtTokens:ExpiredDate"]) && Int32.TryParse(_configuration["JwtTokens:ExpiredDate"], out int days) ? DateTime.Now.AddDays(days) : DateTime.Now.AddDays(5);
            var result = new ResponseModel<LoginResultDto>()
            {
                Data = new LoginResultDto
                {
                    DeviceId = userInfo.DeviceId,
                    RoleId = userInfo.RoleId,
                    RoleName = userInfo.RoleName,
                    AccountType = userInfo.AccountType,
                    UserId = userInfo.User != null ? userInfo.User.Id.ToString().ToUpper() : string.Empty,
                    Username = userInfo.User.UserName,
                    DepartmentId = userInfo.User != null ? userInfo.User.DepartmentId : string.Empty,
                    DepartmentName = userInfo.DepartmentName,
                    SecuriryStamp = userInfo.User != null ? userInfo.User.SecurityStamp : string.Empty,
                    Avatar = userInfo.User != null ? userInfo.User.Avatar : string.Empty,
                    UserCode = userInfo.User != null ? userInfo.User.Code : string.Empty,
                    Email = userInfo.User.Email,
                    Phone = userInfo.User.PhoneNumber,
                    FullName = userInfo.User != null ? userInfo.User.FullName : string.Empty,
                    Status = userInfo.User != null ? (int)userInfo.User.Status : null,
                    ExpiredDate = expiredDate
                }
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Encoding.UTF8.GetBytes(_configuration["JwtTokens:SecretKey"]);
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(UserClaims.UserId, userInfo.User.Id.ToString().Normalize() ?? string.Empty),
                new Claim(UserClaims.SecurityStamp, result.Data.SecuriryStamp ?? string.Empty),
                new Claim(UserClaims.DeviceId, result.Data.DeviceId ?? string.Empty),
                new Claim(UserClaims.Username, userInfo?.User?.UserName ?? string.Empty),
                new Claim(UserClaims.DepartmentName, userInfo?.DepartmentName ?? string.Empty),
                new Claim(UserClaims.RoleName, userInfo?.RoleName ?? string.Empty),
                new Claim(UserClaims.FullName, userInfo?.User?.FullName ?? string.Empty),                
                //new Claim(UserClaims.RoleId, userInfo?.RoleId ?? string.Empty),
                //new Claim(UserClaims.AccountType, result.Data.AccountType ?? string.Empty),
                //new Claim(UserClaims.DepartmentId, userInfo?.User?.DepartmentId ?? string.Empty),
                //new Claim(UserClaims.Avatar, userInfo?.User.Avatar ?? string.Empty),
                //new Claim(UserClaims.Status, result.Data.Status.ToString() ?? string.Empty),
                //new Claim(UserClaims.Code, userInfo?.User?.Code ?? string.Empty),
                //new Claim(UserClaims.ExpiredDate, result?.Data?.ExpiredDate.Value.ToString("dd-MM-yyyy") ?? string.Empty),
                //new Claim(UserClaims.Email, userInfo?.User?.Email ?? string.Empty),
                //new Claim(UserClaims.Phone, userInfo?.User?.PhoneNumber ?? string.Empty),
            });
            if (userInfo.RoleClaims?.Any() == true)
            {
                if (userInfo.RoleClaims?.Any(x => x.ClaimType == Permissions.PCB_BUSINESS) == true)
                {
                    result.Data.MobileAccess = "PCB";
                }
                else if (userInfo.RoleClaims?.Any(x => x.ClaimType == Permissions.MC_BUSINESS) == true)
                {
                    result.Data.MobileAccess = "MC";
                }
                //foreach (var roleClaim in userInfo.RoleClaims)
                //{
                //    claims.AddClaim(new Claim($"{roleClaim.ClaimType}", $"{roleClaim?.ClaimValue ?? string.Empty}"));
                //    //claims.AddClaim(new Claim($"permission[{roleClaim.RoleId}]:{roleClaim.ClaimType}", $"{roleClaim?.ClaimValue ?? string.Empty}"));
                //    result.Data.RoleClaims.Add(new RoleClaimDto { RoleId = result.Data.RoleId, RoleName = result.Data.RoleName, ClaimType = roleClaim.ClaimType, ClaimValue = roleClaim.ClaimValue });
                //}
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = expiredDate,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            try
            {
                var token = tokenHandler.CreateToken(tokenDescriptor);
                result.Data.Token = tokenHandler.WriteToken(token);
                result.Data.RefreshToken = GenerateRefreshToken();
            }
            // If secret key so short, you must use a long string
            catch (ArgumentOutOfRangeException ex)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                _logger.LogError($"Secret Key là một chuỗi ký tự quá ngắn, yêu cầu cài đặt Secret Key là một chuỗi ký tự dài");
                Serilog.Log.Error(ex, "Secret Key là một chuỗi ký tự quá ngắn, yêu cầu cài đặt Secret Key là một chuỗi ký tự dài");
                return await Task.FromResult(result);
            }

            var accountTypes = new List<string>
            {
                nameof(AccountType.TaiKhoanRieng),
                nameof(AccountType.TaiKhoanGiamSat)
            };

            if (accountTypes.Contains(result.Data.AccountType))
            {
                await _deviceTokenContext.AddAsync(new DeviceToken()
                {
                    Id = Guid.NewGuid(),
                    DeviceId = result.Data.DeviceId,
                    Token = result.Data.Token,
                    TokenExpireTime = result.Data.ExpiredDate.Value,
                    UserId = result.Data.UserId.ToUpper(),
                    Status = true
                });
                var saveCount = await _deviceTokenContext.SaveChangesAsync();
                if (saveCount == 0)
                {
                    result.Code = StatusCodes.Status500InternalServerError;
                    result.Message = result.Data.Username = "Lưu Token của thiết bị không thành công";
                    return await Task.FromResult(result);
                }
            }

            userInfo.User.LastActiveTime = DateTime.Now;
            userInfo.User.RefreshToken = result.Data.RefreshToken;
            var updateResult = await _userManager.UpdateAsync(userInfo.User);

            if (!updateResult.Succeeded)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = result.Data.Username = "Cập nhật thời gian tương tác tài khoản thất bại";
                return await Task.FromResult(result);
            }

            if (string.IsNullOrEmpty(result.Data.Token))
            {
                result.Code = (int)HttpStatusCodes.GenerateTokenFailed;
                result.Message = result.Data.Username = "Tạo Token không thành công.";
                return await Task.FromResult(result);
            }

            //Trả về role kiểm kê
            try
            {
                var inventoryInfoResult = await GetInternalInventoryAccount(userInfo.User.Id);
                if (inventoryInfoResult?.Code == StatusCodes.Status200OK)
                {
                    result.Data.InventoryLoggedInfo = inventoryInfoResult.Data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi lấy thông tin tài khoản kiểm kê");
                _logger.LogHttpContext(_httpContext, ex.Message);
            }

            //Nếu tài khoản chung nhưng chưa gán vai trò
            if (result.Data.AccountType == nameof(AccountType.TaiKhoanChung))
            {
                if (result.Data.InventoryLoggedInfo == null || result.Data.InventoryLoggedInfo.HasRoleType == false)
                {
                    result.Code = (int)HttpStatusCodes.NotAssignInventoryRole;
                    result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.NotAssignInventoryRole);
                    return await Task.FromResult(result);
                }
            }

            //Nếu loại tài khoản giám sát chưa được gán vai trò khi kiểm kê
            if (result.Data.AccountType == nameof(AccountType.TaiKhoanGiamSat))
            {
                var isAssignLocationResult = await CheckAuditAccountAssignLocation(result.Data.UserId);
                if (!isAssignLocationResult.Data)
                {
                    result.Code = (int)HttpStatusCodes.AuditAccountNotAssignLocation;
                    result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.AuditAccountNotAssignLocation);
                    return await Task.FromResult(result);
                }
            }

            result.Code = StatusCodes.Status200OK;
            return await Task.FromResult(result);
        }

        #endregion Login

        #region Authorize token

        public ValidateTokenDto GetTokenInfo(HttpContext httpContext)
        {
            var tokenInfo = new ValidateTokenDto()
            {
                Token = httpContext.Request.Headers[Constants.HttpContextModel.AuthorizationKey].FirstOrDefault()?.Split(" ").Last(),
                ClientId = httpContext.Request.Headers[Constants.HttpContextModel.ClientIdKey].FirstOrDefault()?.ToString() ?? _configuration[Constants.AppSettings.ClientId],
                ClientSecret = httpContext.Request.Headers[Constants.HttpContextModel.ClientSecretKey].FirstOrDefault()?.ToString() ?? _configuration[Constants.AppSettings.ClientSecret],
                DeviceId = httpContext.Request.Headers[Constants.HttpContextModel.DeviceId].FirstOrDefault()?.ToString()
            };
            return tokenInfo;
        }

        public ValidateDto<ValidateTokenErrorDto> ValidateTokenInfoInput(ValidateTokenDto model)
        {
            var result = new ValidateDto<ValidateTokenErrorDto>(new ValidateTokenErrorDto()) { IsInvalid = false };
            if (model == null)
            {
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Dữ liệu không hợp lệ";
                return result;
            }
            if (string.IsNullOrEmpty(model.Token) || model.Token.Trim().Contains(" "))
            {
                result.IsInvalid = true;
                result.Data.Token = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.TokenIsInvalid);
                result.Code = StatusCodes.Status400BadRequest;
            }
            if (string.IsNullOrEmpty(model.ClientId) || model.ClientId.Trim().Contains(" "))
            {
                result.IsInvalid = true;
                result.Data.ClientId = "Client Id không hợp lệ.";
                result.Data.ClientSecret = string.Empty;
                result.Code = (int)HttpStatusCodes.InvalidClientId;
            }
            else if (model.ClientId.Trim() != _configuration[Constants.AppSettings.ClientId])
            {
                result.IsInvalid = true;
                result.Data.ClientId = "Client Id không hợp lệ.";
                result.Data.ClientSecret = string.Empty;
                result.Code = (int)HttpStatusCodes.InvalidClientId;
            }
            if (string.IsNullOrEmpty(model.ClientSecret) || model.ClientSecret.Trim().Contains(" "))
            {
                result.IsInvalid = true;
                result.Data.ClientSecret = "Client Secret không hợp lệ.";
                result.Code = (int)HttpStatusCodes.InvalidClientSecret;
            }
            else if (model.ClientSecret.Trim() != _configuration[Constants.AppSettings.ClientSecret])
            {
                result.IsInvalid = true;
                result.Data.ClientSecret = "Client Secret không hợp lệ.";
                result.Code = (int)HttpStatusCodes.InvalidClientSecret;
            }
            if (result.IsInvalid)
            {
                result.Message = "Dữ liệu không hợp lệ";
            }
            return result;
        }

        public async Task<ResponseModel<ValidateTokenResultDto>> ValidateJwtTokenAsync(string jwtToken, bool isForRefreshToken = false)
        {
            var result = new ResponseModel<ValidateTokenResultDto>() { Data = new ValidateTokenResultDto() };
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtTokens:SecretKey"]));
            //var secretKey = Encoding.UTF8.GetBytes(_configuration["JwtTokens:SecretKey"]);

            // Create token validation parameters
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                //ValidateIssuerSigningKey = false,
                ValidateIssuer = false,
                //ValidIssuer = "your_issuer",
                ValidateAudience = false,
                //ValidAudience = "your_audience",
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero,
                // ClockSkew = TimeSpan.FromMinutes(5),
                IssuerSigningKey = secretKey
            };

            try
            {
                // Validate the token
                ClaimsPrincipal validatedJwtToken = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out SecurityToken validatedToken);

                // Token is valid, and the claims are available in the 'validatedJwtToken' variable
                result.Data.UserId = validatedJwtToken.Claims.FirstOrDefault(x => x.Type == UserClaims.UserId)?.Value ?? string.Empty;
                result.Data.SecurityStamp = validatedJwtToken.Claims.FirstOrDefault(x => x.Type == UserClaims.SecurityStamp)?.Value ?? string.Empty;
                result.Data.DeviceId = validatedJwtToken.Claims.FirstOrDefault(x => x.Type == UserClaims.DeviceId)?.Value ?? string.Empty;
            }

            // If the token has expired as the "exp" claim has passed
            // I.e, Lifetime validation failed. The token is expired. ValidTo: '10/8/2023 3:55:56 AM', Current time: '10/8/2023 3:56:22 AM'
            catch (SecurityTokenExpiredException ex)
            {
                result.Code = (int)HttpStatusCodes.TokenHasExpired;
                result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.TokenHasExpired);
                return await Task.FromResult(result);
            }

            // If the token's signature is invalid, i.e secret key is not valid or security algorithm is not valid
            catch (SecurityTokenInvalidSignatureException ex)
            {
                result.Code = (int)HttpStatusCodes.TokenSignatureIsInvalid;
                result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.TokenSignatureIsInvalid);
                return await Task.FromResult(result);
            }

            // If the token is not yet valid when 'nbf' claim is not yet valid
            // I.e, Lifetime validation failed. The NotBefore: '10/8/2023 3:48:23 AM' is after Expires: '10/8/2023 3:48:22 AM'
            catch (SecurityTokenNotYetValidException ex)
            {
                result.Code = (int)HttpStatusCodes.TokenIsInvalid;
                result.Message = "Token chưa hợp lệ";
                return await Task.FromResult(result);
            }

            //  For other validation issues that don't fall into the above categories
            catch (SecurityTokenValidationException ex)
            {
                result.Code = (int)HttpStatusCodes.TokenIsInvalid;
                result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.TokenIsInvalid);
                return await Task.FromResult(result);
            }

            var user = await _userManager.FindByIdAsync(result.Data.UserId ?? Guid.Empty.ToString());
            if (user == null)
            {
                result.Code = (int)HttpStatusCodes.TheAccountIsNotExisted;
                result.Message = "Tài khoản không còn tồn tại";
                return await Task.FromResult(result);
            }

            // Check security stamp in case as change user password
            // If security stamp in claim not equals to security stamp of current user in database then force logout user
            var securityStamp = await _userManager.GetSecurityStampAsync(user);
            if (securityStamp != result.Data.SecurityStamp)
            {
                result.Code = (int)HttpStatusCodes.InvalidSecurityStampAfterChangePassword;
                result.Message = result.Data.UserId = "Security Stamp không hợp lệ sau khi thay đổi mật khẩu thành công. Cần Logout để lấy ra Security Stamp mới nhất.";
                return await Task.FromResult(result);
            }

            //force logout when role changed ...
            var userId = user.Id.ToString().ToUpper();

            var isForceLogout = await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.UserId == userId && x.ForceLogoutCode == HttpStatusCodes.RoleChanged60);
            if (isForceLogout)
            {
                result.Message = HttpStatusCodes.RoleChanged60.GetDisplayName();
                result.Code = (int)HttpStatusCodes.RoleChanged60;
                return await Task.FromResult(result);
            }

            #region Check account type

            switch (user.AccountType.Value)
            {
                case AccountType.TaiKhoanRieng:
                    result.Data.AccountType = nameof(AccountType.TaiKhoanRieng);
                    break;

                case AccountType.TaiKhoanChung:
                    result.Data.AccountType = nameof(AccountType.TaiKhoanChung);
                    break;

                case AccountType.TaiKhoanGiamSat:
                    result.Data.AccountType = nameof(AccountType.TaiKhoanGiamSat);
                    break;


                default:
                    {
                        result.Data.AccountType = "Loại tài khoản không hợp lệ";
                        result.Code = StatusCodes.Status400BadRequest;
                        result.Message = "Dữ liệu không hợp lệ";
                        return await Task.FromResult(result);
                    }
            }

            #endregion Check account type

            #region Check user status

            switch (user.Status)
            {
                case UserStatus.Active:
                    break;

                case UserStatus.LockByExpiredPassword:
                    {
                        result.Message = "Tài khoản ở trạng thái bị khóa vì mật khẩu hết hạn";
                        result.Code = (int)HttpStatusCodes.LockByNotUpdatePassword;
                        return await Task.FromResult(result);
                    }

                case UserStatus.LockByUnactive:
                    {
                        result.Message = "Tài khoản ở trạng thái bị khóa vì không tương tác";
                        result.Code = (int)HttpStatusCodes.LockByUnactive;
                        return await Task.FromResult(result);
                    }

                case UserStatus.LockByAdmin:
                    {
                        result.Message = "Tài khoản ở trạng thái bị khóa bởi Admin";
                        result.Code = (int)HttpStatusCodes.LockByAdmin;
                        return await Task.FromResult(result);
                    }

                case UserStatus.Deleted:
                    {
                        result.Message = "Tài khoản ở trạng thái đã bị xóa";
                        result.Code = (int)HttpStatusCodes.ThisAccountHasBeenDeleted;
                        return await Task.FromResult(result);
                    }

                default:
                    break;
            }

            #endregion Check user status

            #region Logout no need check device token, token expire time

            // If request is logout user then no need check device token, token expire time
            string webAppLogoutEndpoint = Constants.Endpoint.WebApp_Logout, apiLogoutEndpoint = Constants.Endpoint.API_Identity_Logout;
            if (_httpContext.Request.Path.ToString().ToLower() == webAppLogoutEndpoint.ToLower()
                || _httpContext.Request.Path.ToString().ToLower() == apiLogoutEndpoint.ToLower())
            {
                result.Code = StatusCodes.Status200OK;
                return await Task.FromResult(result);
            }

            #endregion Logout no need check device token, token expire time

            #region check personal account device token

            if (user.AccountType.Value == AccountType.TaiKhoanRieng || user.AccountType.Value == AccountType.TaiKhoanGiamSat)
            {
                var deviceId = result.Data.DeviceId?.ToUpper() ?? string.Empty;

                // Mobile
                if (!string.IsNullOrEmpty(deviceId))
                {
                    //if (await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.DeviceId.ToUpper() != deviceId && x.UserId.ToUpper() == userId && x.Status == true && x.TokenExpireTime >= DateTime.Now))
                    //{
                    //    result.Message = "Tài khoản đã được đăng nhập ở thiết bị khác";
                    //    result.Code = (int)HttpStatusCodes.ThisAccountHasBeenLoggedIntoOtherDeviceBefore;
                    //    return await Task.FromResult(result);
                    //}
                    //else 
                    if (await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.DeviceId.ToUpper() == deviceId && x.UserId.ToUpper() == userId && x.Status == true && x.TokenExpireTime < DateTime.Now))
                    {
                        result.Message = "Token hết hạn sử dụng";
                        result.Code = (int)HttpStatusCodes.TokenHasExpired;
                        return await Task.FromResult(result);
                    }
                    else if (await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.DeviceId.ToUpper() == deviceId && x.UserId.ToUpper() == userId && x.Status == false))
                    {
                        result.Message = "Token không còn hiệu lực";
                        result.Code = (int)HttpStatusCodes.TokenIsInvalid;
                        return await Task.FromResult(result);
                    }
                    else if (!await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.DeviceId.ToUpper() == deviceId && x.UserId.ToUpper() == userId))
                    {
                        result.Message = "Token không hợp lệ";
                        result.Code = (int)HttpStatusCodes.TokenIsInvalid;
                        return await Task.FromResult(result);
                    }
                }

                // Website
                else
                {
                    if (await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.UserId.ToUpper() == userId && x.Token != jwtToken && x.Status == true && x.TokenExpireTime >= DateTime.Now) && !isForRefreshToken)
                    {
                        result.Message = "Tài khoản đã được đăng nhập ở thiết bị khác";
                        result.Code = (int)HttpStatusCodes.ThisAccountHasBeenLoggedIntoOtherDeviceBefore;
                        return await Task.FromResult(result);
                    }
                    else if (await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.UserId.ToUpper() == userId && x.Token == jwtToken && x.Status == true && x.TokenExpireTime < DateTime.Now))
                    {
                        result.Message = "Token hết hạn sử dụng";
                        result.Code = (int)HttpStatusCodes.TokenHasExpired;
                        return await Task.FromResult(result);
                    }
                    else if (await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.UserId.ToUpper() == userId && x.Token == jwtToken && x.Status == false))
                    {
                        result.Message = "Token không còn hiệu lực";
                        result.Code = (int)HttpStatusCodes.TokenIsInvalid;
                        return await Task.FromResult(result);
                    }
                    else if (!await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.UserId.ToUpper() == userId && x.Token == jwtToken))
                    {
                        result.Message = "Token không hợp lệ";
                        result.Code = (int)HttpStatusCodes.TokenIsInvalid;
                        return await Task.FromResult(result);
                    }
                }
            }

            #endregion check personal account device token

            result.Data.Username = user.UserName;
            result.Data.Fullname = user.FullName;
            result.Data.Avatar = user.Avatar;
            result.Data.UserCode = user.Code;
            result.Data.Email = user.Email;
            result.Data.Phone = user.PhoneNumber;
            if (!string.IsNullOrEmpty(user.DepartmentId) && Guid.TryParse(user.DepartmentId, out var _departmentId))
            {
                var department = await _context.Departments.FindAsync(_departmentId);
                result.Data.DepartmentId = department != null ? department?.Id.ToString() : string.Empty;
                result.Data.DepartmentName = department != null ? department?.Name : string.Empty;
            }
            var roleName = (await _userManager.GetRolesAsync(user))?.FirstOrDefault();
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                result.Data.RoleId = role?.Id.ToString();
                result.Data.RoleName = role?.Name;
                result.Data.RoleClaims = new List<RoleClaimDto?>();
                var permissionList = Constants.Permissions.PermissionsList.Select(x => x.Item1);
                var claims = await _roleManager.GetClaimsAsync(role);
                var roleClaims = (claims)?.Where(x => permissionList.Contains(x.Type));
                if (roleClaims?.Any() == true)
                {
                    foreach (var roleClaim in roleClaims)
                    {
                        result.Data.RoleClaims.Add(new RoleClaimDto
                        {
                            RoleId = result.Data.RoleId,
                            RoleName = result.Data.RoleName,
                            ClaimType = roleClaim.Type,
                            ClaimValue = roleClaim.Value
                        });
                        //string roleNamePattern = @"(\[).*(\])";
                        //var getRoleNameStr = Regex.Matches(roleClaim.Type, roleNamePattern, RegexOptions.IgnoreCase)?.FirstOrDefault()?.Value;
                        //if (!string.IsNullOrEmpty(getRoleNameStr))
                        //{
                        //    var roleName = getRoleNameStr.Replace("[", string.Empty).Replace("]", string.Empty);
                        //    userInfo.RoleClaims.Add(new RoleClaimDto
                        //    {
                        //        RoleId = roleName,
                        //        ClaimType = roleClaim.Type,
                        //        ClaimValue = roleClaim.Value
                        //    });
                        //}
                    }
                }
            }

            #region update last active time

            user.LastActiveTime = DateTime.Now;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = result.Data.UserId = "Cập nhật thời gian tương tác tài khoản thất bại";
                return await Task.FromResult(result);
            }

            #endregion update last active time

            //Trả về role kiểm kê
            try
            {
                var inventoryInfoResult = await GetInternalInventoryAccount(user.Id);
                if (inventoryInfoResult?.Code == StatusCodes.Status200OK)
                {
                    result.Data.InventoryLoggedInfo = inventoryInfoResult.Data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi lấy thông tin tài khoản kiểm kê", ex.Message);
            }

            //Nếu tài khoản chung nhưng chưa gán vai trò
            if (result.Data.AccountType == nameof(AccountType.TaiKhoanChung))
            {
                if (result.Data.InventoryLoggedInfo == null || result.Data.InventoryLoggedInfo.HasRoleType == false)
                {
                    result.Code = (int)HttpStatusCodes.NotAssignInventoryRole;
                    result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.NotAssignInventoryRole);
                    return await Task.FromResult(result);
                }
            }

            result.Code = StatusCodes.Status200OK;
            result.Message = "Token hợp lệ";
            return await Task.FromResult(result);
        }

        public ClaimsIdentity CreateClaimsIdentity(ValidateTokenResultDto userInfo)
        {
            var claimsIdentity = new ClaimsIdentity(Constants.HttpContextModel.AuthorizationKey);
            claimsIdentity.AddClaims(new[]
            {
                new Claim(UserClaims.UserId, userInfo?.UserId?.ToString() ?? string.Empty),
                new Claim(UserClaims.SecurityStamp, userInfo?.SecurityStamp ?? string.Empty),
                new Claim(UserClaims.Code, userInfo?.UserCode ?? string.Empty),
                new Claim(UserClaims.Username, userInfo?.Username ?? string.Empty),
                new Claim(UserClaims.Email, userInfo?.Email ?? string.Empty),
                new Claim(UserClaims.Phone, userInfo?.Phone ?? string.Empty),
                new Claim(UserClaims.AccountType, userInfo?.AccountType ?? string.Empty),
                new Claim(UserClaims.FullName, userInfo?.Fullname ?? string.Empty),
                new Claim(UserClaims.DepartmentId, userInfo?.DepartmentId ?? string.Empty),
                new Claim(UserClaims.DepartmentName, userInfo?.DepartmentName ?? string.Empty),
                new Claim(UserClaims.DeviceId, userInfo?.DeviceId ?? string.Empty),
                new Claim(UserClaims.RoleId, userInfo?.RoleId ?? string.Empty),
                new Claim(UserClaims.RoleName, userInfo?.RoleName ?? string.Empty),
                new Claim(UserClaims.Avatar, userInfo?.Avatar ?? string.Empty),
                new Claim(UserClaims.ExpiredDate, userInfo?.ExpiredDate.ToString() ?? string.Empty)
            });

            if (userInfo?.RoleClaims?.Any() == true)
            {
                foreach (var claim in userInfo.RoleClaims)
                {
                    claimsIdentity.AddClaim(new Claim(claim?.ClaimType ?? "DEFAULT_PERMISSION", claim?.ClaimValue ?? string.Empty));
                }
            }
            return claimsIdentity;
        }

        #endregion Authorize token

        #region Logout

        public ValidateDto<LogoutResultDto> ValidateLogoutInput(LogoutDto logoutUser)
        {
            var result = new ValidateDto<LogoutResultDto>(new LogoutResultDto()) { IsInvalid = false };
            if (Guid.TryParse(logoutUser.UserId.ToString(), out var _) == false)
            {
                result.Data.Fail = "Id Tài khoản không hợp lệ";
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
            }
            return result;
        }

        public async Task<ValidateDto<LogoutResultDto>> UpdateDeviceTokenLogoutAsync(LogoutDto logoutUser)
        {
            var result = new ValidateDto<LogoutResultDto>(new LogoutResultDto()) { IsInvalid = false };
            var normalizeUserId = logoutUser.UserId.ToString().ToUpper();

            string successfulUserId = string.Empty, failedUserId = string.Empty, notExistUserId = string.Empty;
            var userClaimPrincipal = _httpContext.User.Identities.FirstOrDefault(x => x.AuthenticationType == Constants.HttpContextModel.AuthorizationKey);
            // var user = await _userManager.FindByIdAsync(normalizeUserId);
            var accountType = userClaimPrincipal?.Claims?.FirstOrDefault(x => x.Type == UserClaims.AccountType)?.Value ?? string.Empty;
            if (accountType == nameof(AccountType.TaiKhoanRieng) || accountType == nameof(AccountType.TaiKhoanGiamSat))
            {
                var userId = userClaimPrincipal.Claims.FirstOrDefault(x => x.Type == UserClaims.UserId)?.Value?.ToUpper();
                var deviceTokens = _deviceTokenContext.DeviceTokens.Where(x => x.Status == true && normalizeUserId == x.UserId.ToUpper());

                if (await deviceTokens.AnyAsync(x => x.UserId.ToUpper() == normalizeUserId))
                {
                    var deviceToken = deviceTokens.FirstOrDefault(x => x.UserId.ToUpper() == normalizeUserId);
                    if (deviceToken != null)
                    {
                        //deviceToken.Status = false;
                        //deviceToken.ForceLogoutCode = HttpStatusCodes.RoleChanged60;
                        //_deviceTokenContext.Update(deviceToken);
                        _deviceTokenContext.DeviceTokens.Remove(deviceToken);
                        await _deviceTokenContext.SaveChangesAsync();
                    }
                }
                successfulUserId = userClaimPrincipal.Claims.FirstOrDefault(x => x.Type == UserClaims.UserId)?.Value;
            }
            else if (userClaimPrincipal != null && userClaimPrincipal.Claims.FirstOrDefault(x => x.Type == UserClaims.AccountType)?.Value == nameof(AccountType.TaiKhoanChung))
            {
                successfulUserId = userClaimPrincipal.Claims.FirstOrDefault(x => x.Type == UserClaims.UserId)?.Value;
            }
            else
            {
                notExistUserId = userClaimPrincipal.Claims.FirstOrDefault(x => x.Type == UserClaims.UserId)?.Value;
            }

            if (!string.IsNullOrEmpty(successfulUserId))
            {
                result.Code = StatusCodes.Status200OK;
                result.Message = "Đăng xuất thành công. Cần xóa token ở header của request.";
                result.Data.Success = successfulUserId;
            }
            else
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Đăng xuất không thành công. Tài khoản không còn token hợp lệ hoặc Tài khoản không còn tồn tại.";
                result.IsInvalid = true;
            }
            if (!string.IsNullOrEmpty(failedUserId))
            {
                result.Data.Fail = failedUserId;
            }
            if (!string.IsNullOrEmpty(notExistUserId))
            {
                result.Data.NotExists = notExistUserId;
            }
            return result;
        }

        public ValidateDto<LogoutResultDto> ValidateLogoutInput(LogoutMultipleDto userIdList)
        {
            var result = new ValidateDto<LogoutResultDto>(new LogoutResultDto()) { IsInvalid = false };
            if (userIdList?.UserIdList?.Any(x => x.UserId.ToString().Length != 36) == true)
            {
                result.Data.Fail = "Id Tài khoản không hợp lệ";
                result.IsInvalid = true;
                result.Code = StatusCodes.Status400BadRequest;
            }
            return result;
        }

        public async Task<ValidateDto<LogoutResultDto>> UpdateDeviceTokenLogoutAsync(LogoutMultipleDto userIdList)
        {
            var result = new ValidateDto<LogoutResultDto>(new LogoutResultDto()) { IsInvalid = false };
            var normalizeUserIds = userIdList.UserIdList.Select(x => x.UserId.ToString().ToUpper()).ToList();
            var deviceTokens = _deviceTokenContext.DeviceTokens
                .Where(x => x.Status == true && normalizeUserIds.Contains(x.UserId.ToUpper()));

            List<string> successfulUsers = new(), failedUsers = new(), notExistUserIds = new();
            if (userIdList.UserIdList?.Any() == true)
            {
                foreach (var logoutUser in userIdList.UserIdList)
                {
                    var userId = logoutUser.UserId.ToString().ToUpper();
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null && (user.AccountType == AccountType.TaiKhoanRieng || user.AccountType == AccountType.TaiKhoanGiamSat))
                    {
                        if (await deviceTokens.AnyAsync(x => x.UserId.ToUpper() == userId))
                        {
                            var deviceToken = deviceTokens.FirstOrDefault(x => x.UserId.ToUpper() == userId);
                            if (deviceToken != null)
                            {
                                deviceToken.Status = false;
                                _deviceTokenContext.Update(deviceToken);
                                var sucess = await _deviceTokenContext.SaveChangesAsync();
                                if (sucess > 0)
                                {
                                    successfulUsers.Add(user.UserName);
                                }
                            }
                        }
                        else
                        {
                            failedUsers.Add(user.UserName);
                        }
                    }
                    else if (user != null && user.AccountType == AccountType.TaiKhoanChung)
                    {
                        successfulUsers.Add(user.UserName);
                    }
                    else
                    {
                        notExistUserIds.Add(userId);
                    }
                }
            }

            if (successfulUsers.Any() == true)
            {
                result.Code = StatusCodes.Status200OK;
                result.Message = "Đăng xuất thành công. Cần xóa token ở header của request.";
                result.Data.Success = string.Join(",", successfulUsers);
            }
            else
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Đăng xuất không thành công. Tài khoản không còn token hợp lệ hoặc Tài khoản không còn tồn tại.";
                result.IsInvalid = true;
            }
            if (failedUsers.Any() == true)
            {
                result.Data.Fail = string.Join(",", failedUsers);
            }
            if (notExistUserIds.Any() == true)
            {
                result.Data.NotExists = string.Join(",", notExistUserIds);
            }
            return result;
        }

        #endregion Logout

        #region Change password

        public ValidateDto<ChangePasswordErrorDto> ValidateChangePasswordInput(ChangePasswordDto model)
        {
            var result = new ValidateDto<ChangePasswordErrorDto>(new ChangePasswordErrorDto()) { IsInvalid = false };
            if (string.IsNullOrEmpty(model?.UserId) || !Guid.TryParse(model.UserId, out _))
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
            else if (!PasswordRegex().IsMatch(model.OldPassword.Trim()))
            {
                result.Data.OldPassword = " Mật khẩu phải có độ dài từ 8 - 15 ký tự, bao gồm cả ký tự chữ và số, không chứa ký tự khoảng trắng.";
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
            else if (!PasswordRegex().IsMatch(model.NewPassword.Trim()))
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
            else if (!PasswordRegex().IsMatch(model.NewPasswordConfirm.Trim()))
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
                result.Data.NewPassword = "Mật khẩu mới không được trùng mật với mật khẩu hiện tại của bạn.";
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

        public async Task<ResponseModel<ChangePasswordResultDto>> ChangePasswordAsync(ChangePasswordDto model)
        {
            var result = new ResponseModel<ChangePasswordResultDto>(new ChangePasswordResultDto());
            var user = await _userManager.FindByIdAsync(model.UserId);
            var currentLoggedIn = await _userManager.FindByIdAsync(model.CurrentUserId);

            var userManagementClaimsQuery = from u in _context.Users
                                            join ur in _context.UserRoles on u.Id equals ur.UserId
                                            join r in _context.Roles on ur.RoleId equals r.Id
                                            join rc in _context.RoleClaims on r.Id equals rc.RoleId
                                            where (u.Id == currentLoggedIn.Id) && rc.ClaimType == Permissions.USER_MANAGEMENT
                                            select rc.Id;

            if (!userManagementClaimsQuery.Any() && model.UserId.ToLower() != model.CurrentUserId.ToLower())
            {
                result.Code = StatusCodes.Status403Forbidden;
                result.Message = "Không có quyền thay đổi mật khẩu của tài khoản khác.";
                return await Task.FromResult(result);
            }

            if (user == null)
            {
                result.Code = (int)HttpStatusCodes.TheAccountIsNotExisted;
                result.Message = "Tài khoản không tồn tại.";
                return await Task.FromResult(result);
            }
            var changePassRes = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePassRes.Succeeded)
            {
                if (changePassRes.Errors?.Any() == true)
                {
                    foreach (var error in changePassRes.Errors)
                    {
                        if (error.Code == "PasswordMismatch")
                        {
                            result.Code = (int)HttpStatusCodes.InvalidOldPassword;
                            result.Message = "Thông tin đăng nhập không đúng. Vui lòng liên hệ với quản lý để được trợ giúp.";
                            return await Task.FromResult(result);
                        }
                    }
                }
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "Thay đổi mật khẩu không thành công.";
                return await Task.FromResult(result);
            }

            // Password change successful; update the security stamp
            var updateSecurityStampRes = await _userManager.UpdateSecurityStampAsync(user);
            if (!updateSecurityStampRes.Succeeded)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "Thay đổi mật khẩu không thành công.";
                return await Task.FromResult(result);
            }
            user.UpdatedPasswordAt = DateTime.Now;
            user.LockPwdSetting = false;
            user.LockPwTime = null;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                return await Task.FromResult(result);
            }
            result.Code = StatusCodes.Status200OK;
            result.Message = "Thay đổi mật khẩu thành công.";
            result.Data.Success = true;
            return await Task.FromResult(result);
        }

        #endregion Change password

        public async Task<ResponseModel<bool>> AuthorizePermission(string permission)
        {
            var userClaims = _httpContext.User.Claims;

            var validPermission = userClaims?.Any(p => p.Type == permission);

            if (validPermission == false)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = false,
                    Message = "Bạn không có quyền này."
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Được phép truy cập với quyền này."
            };
        }

        [GeneratedRegex("^(?=.*[a-zA-Z])(?=.*[0-9]).+$")]
        private static partial Regex PasswordRegex();

        public async Task RemoveExpiredToken()
        {
            var expired = _deviceTokenContext.DeviceTokens.Where(x => x.TokenExpireTime < DateTime.Now);
            _deviceTokenContext.DeviceTokens.RemoveRange(expired);
            await _deviceTokenContext.SaveChangesAsync();
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

        public async Task<ResponseModel<ChangePasswordResultDto>> ResetPasswordAsync(ResetPasswordDto model)
        {
            var result = new ResponseModel<ChangePasswordResultDto>(new ChangePasswordResultDto());
            var user = await _userManager.FindByIdAsync(model.UserId);
            var currentUserHasAdminRoleLoggedIn = await _userManager.FindByIdAsync(model.CurrentUserId);

            var userManagementClaimsQuery = from u in _context.Users
                                            join ur in _context.UserRoles on u.Id equals ur.UserId
                                            join r in _context.Roles on ur.RoleId equals r.Id
                                            join rc in _context.RoleClaims on r.Id equals rc.RoleId
                                            where (u.Id == currentUserHasAdminRoleLoggedIn.Id) && rc.ClaimType == Permissions.USER_MANAGEMENT
                                            select rc.Id;

            if (!userManagementClaimsQuery.Any() && model.UserId.ToLower() != model.CurrentUserId.ToLower())
            {
                result.Code = StatusCodes.Status403Forbidden;
                result.Message = "Không có quyền thay đổi mật khẩu của tài khoản khác.";
                return await Task.FromResult(result);
            }

            if (user == null)
            {
                result.Code = (int)HttpStatusCodes.TheAccountIsNotExisted;
                result.Message = "Tài khoản không tồn tại.";
                return await Task.FromResult(result);
            }

            var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrEmpty(resetPasswordToken))
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "Tạo Token để Reset mật khẩu không thành công.";
                return await Task.FromResult(result);
            }

            var changePassRes = await _userManager.ResetPasswordAsync(user, resetPasswordToken, model.NewPassword);
            if (!changePassRes.Succeeded)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "Thay đổi mật khẩu không thành công.";
                return await Task.FromResult(result);
            }

            // Password change successful; update the security stamp
            var updateSecurityStampRes = await _userManager.UpdateSecurityStampAsync(user);
            if (!updateSecurityStampRes.Succeeded)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "Thay đổi mật khẩu không thành công.";
                return await Task.FromResult(result);
            }
            user.UpdatedPasswordAt = DateTime.Now;
            user.LockPwdSetting = false;
            user.LockPwTime = null;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                return await Task.FromResult(result);
            }
            result.Code = StatusCodes.Status200OK;
            result.Message = "Thay đổi mật khẩu thành công.";
            result.Data.Success = true;
            return await Task.FromResult(result);
        }

        public async Task<LogoutMultipleDto> GetUserIdListByRoleIdAsync(string roleId)
        {
            var users = new LogoutMultipleDto() { UserIdList = new List<LogoutDto>() };
            users.UserIdList = await _context.UserRoles?.Where(x => x.RoleId == Guid.Parse(roleId))?.Select(x => new LogoutDto { UserId = x.UserId })?.ToListAsync();
            return users;
        }
        private async Task<ResponseModel<InventoryLoggedInfo>> GetInternalInventoryAccount(Guid userId)
        {
            var client = _restClientFactory.InventoryClient();
            var req = new RestRequest($"api/inventory/internal/account/{userId}");

            req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
            var result = await client.ExecuteGetAsync(req);
            Log.Information("GetInternalInventoryAccount: {0}", result?.Content);
            var convertedResult = new ResponseModel<InventoryLoggedInfo>();
            if (!string.IsNullOrEmpty(result?.Content))
            {

                convertedResult = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<InventoryLoggedInfo>>(result?.Content ?? "", JsonDefaults.CamelCasing);
            }

            return convertedResult;
        }

        private async Task<ResponseModel<InventoryLoggedInfo>> AuditA(Guid userId)
        {
            var client = _restClientFactory.InventoryClient();
            var req = new RestRequest($"api/inventory/internal/account/{userId}");

            req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
            var result = await client.ExecuteGetAsync(req);
            var convertedResult = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<InventoryLoggedInfo>>(result?.Content ?? "", JsonDefaults.CamelCasing);
            return convertedResult;
        }

        private async Task<ResponseModel<bool>> CheckAuditAccountAssignLocation(string userId)
        {
            var client = _restClientFactory.InventoryClient();
            var req = new RestRequest($"api/inventory/internal/account/{userId}/check-assignlocation");

            req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
            var result = await client.ExecuteGetAsync(req);
            Log.Error("GetInternalInventoryAccount: {0}", result?.Content);
            var convertedResult = new ResponseModel<bool>();
            if (!string.IsNullOrEmpty(result?.Content))
                convertedResult = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<bool>>(result?.Content ?? "", JsonDefaults.CamelCasing);

            return convertedResult;
        }

        public async Task<ResponseModel<RefreshTokenResultDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var appUser = await _context.AppUsers.FirstOrDefaultAsync(x => x.RefreshToken == refreshTokenDto.RefreshToken);

            if (appUser != null)
            {
                var loginDto = new LoginDto { Username = appUser.UserName, DeviceId = refreshTokenDto.DeviceId };
                var userInfo = await GetUserInfoFromLoginAsync(loginDto, true);
                var result = await GenerateJwtTokenAsync(userInfo.UserInfo);

                var oldToken = _deviceTokenContext.DeviceTokens.FirstOrDefault(x => Guid.Parse(x.UserId) == userInfo.UserInfo.User.Id && x.Token == refreshTokenDto.OldToken);
                if (oldToken != null)
                {
                    _deviceTokenContext.Remove(oldToken);
                    await _deviceTokenContext.SaveChangesAsync();
                }

                return new ResponseModel<RefreshTokenResultDto>
                {
                    Code = StatusCodes.Status200OK,
                    Data = new RefreshTokenResultDto
                    {
                        Token = result.Data.Token,
                        RefreshToken = result.Data.RefreshToken,
                        ExpiredDate = result.Data.ExpiredDate,
                        DeviceId = refreshTokenDto.DeviceId,
                        UserId = userInfo.UserInfo.User.Id
                    }
                };

            }
            return new ResponseModel<RefreshTokenResultDto>
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Refresh token không hợp lệ"
            };
        }

        public async Task<ResponseModel<ValidateTokenResultDto>> ValidateJwtRefreshTokenAsync(string jwtToken, string refreshToken, bool isForRefreshToken = false)
        {
            var result = new ResponseModel<ValidateTokenResultDto>() { Data = new ValidateTokenResultDto() };
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtTokens:SecretKey"]));
            //var secretKey = Encoding.UTF8.GetBytes(_configuration["JwtTokens:SecretKey"]);

            //// Create token validation parameters
            //var tokenValidationParameters = new TokenValidationParameters
            //{
            //    ValidateIssuerSigningKey = true,
            //    //ValidateIssuerSigningKey = false,
            //    ValidateIssuer = false,
            //    //ValidIssuer = "your_issuer",
            //    ValidateAudience = false,
            //    //ValidAudience = "your_audience",
            //    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
            //    ClockSkew = TimeSpan.Zero,
            //    // ClockSkew = TimeSpan.FromMinutes(5),
            //    IssuerSigningKey = secretKey
            //};

            //try
            //{
            //    // Validate the token
            //    ClaimsPrincipal validatedJwtToken = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out SecurityToken validatedToken);

            //    // Token is valid, and the claims are available in the 'validatedJwtToken' variable
            //    result.Data.UserId = validatedJwtToken.Claims.FirstOrDefault(x => x.Type == UserClaims.UserId)?.Value ?? string.Empty;
            //    result.Data.SecurityStamp = validatedJwtToken.Claims.FirstOrDefault(x => x.Type == UserClaims.SecurityStamp)?.Value ?? string.Empty;
            //    result.Data.DeviceId = validatedJwtToken.Claims.FirstOrDefault(x => x.Type == UserClaims.DeviceId)?.Value ?? string.Empty;
            //}

            //// If the token has expired as the "exp" claim has passed
            //// I.e, Lifetime validation failed. The token is expired. ValidTo: '10/8/2023 3:55:56 AM', Current time: '10/8/2023 3:56:22 AM'
            //catch (SecurityTokenExpiredException ex)
            //{
            //    result.Code = (int)HttpStatusCodes.TokenHasExpired;
            //    result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.TokenHasExpired);
            //    return await Task.FromResult(result);
            //}

            //// If the token's signature is invalid, i.e secret key is not valid or security algorithm is not valid
            //catch (SecurityTokenInvalidSignatureException ex)
            //{
            //    result.Code = (int)HttpStatusCodes.TokenSignatureIsInvalid;
            //    result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.TokenSignatureIsInvalid);
            //    return await Task.FromResult(result);
            //}

            //// If the token is not yet valid when 'nbf' claim is not yet valid
            //// I.e, Lifetime validation failed. The NotBefore: '10/8/2023 3:48:23 AM' is after Expires: '10/8/2023 3:48:22 AM'
            //catch (SecurityTokenNotYetValidException ex)
            //{
            //    result.Code = (int)HttpStatusCodes.TokenIsInvalid;
            //    result.Message = "Token chưa hợp lệ";
            //    return await Task.FromResult(result);
            //}

            ////  For other validation issues that don't fall into the above categories
            //catch (SecurityTokenValidationException ex)
            //{
            //    result.Code = (int)HttpStatusCodes.TokenIsInvalid;
            //    result.Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.TokenIsInvalid);
            //    return await Task.FromResult(result);
            //}
            try
            {
                var jwtTokens = tokenHandler.ReadJwtToken(jwtToken);
                // Token is valid, and the claims are available in the 'validatedJwtToken' variable
                result.Data.UserId = jwtTokens.Claims.FirstOrDefault(x => x.Type == UserClaims.UserId)?.Value ?? string.Empty;
                result.Data.SecurityStamp = jwtTokens.Claims.FirstOrDefault(x => x.Type == UserClaims.SecurityStamp)?.Value ?? string.Empty;
                result.Data.DeviceId = jwtTokens.Claims.FirstOrDefault(x => x.Type == UserClaims.DeviceId)?.Value ?? string.Empty;
            }
            catch (Exception ex)
            {

                throw;
            }


            var user = await _userManager.FindByIdAsync(result.Data.UserId ?? Guid.Empty.ToString());
            if (user == null)
            {
                result.Code = (int)HttpStatusCodes.TheAccountIsNotExisted;
                result.Message = "Tài khoản không còn tồn tại";
                return await Task.FromResult(result);
            }


            // Check security stamp in case as change user password
            // If security stamp in claim not equals to security stamp of current user in database then force logout user
            var securityStamp = await _userManager.GetSecurityStampAsync(user);
            if (securityStamp != result.Data.SecurityStamp)
            {
                result.Code = (int)HttpStatusCodes.InvalidSecurityStampAfterChangePassword;
                result.Message = result.Data.UserId = "Security Stamp không hợp lệ sau khi thay đổi mật khẩu thành công. Cần Logout để lấy ra Security Stamp mới nhất.";
                return await Task.FromResult(result);
            }

            //force logout when role changed ...
            var userId = user.Id.ToString().ToUpper();

            var isForceLogout = await _deviceTokenContext.DeviceTokens?.AnyAsync(x => x.UserId == userId && x.ForceLogoutCode == HttpStatusCodes.RoleChanged60);
            if (isForceLogout)
            {
                result.Message = HttpStatusCodes.RoleChanged60.GetDisplayName();
                result.Code = (int)HttpStatusCodes.RoleChanged60;
                return await Task.FromResult(result);
            }


            result.Code = StatusCodes.Status200OK;
            result.Message = "Token hợp lệ";
            return await Task.FromResult(result);
        }


    }
}
