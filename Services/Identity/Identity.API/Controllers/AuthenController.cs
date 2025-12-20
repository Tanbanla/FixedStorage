using System.Net.WebSockets;
using Microsoft.AspNetCore.Authorization;

namespace BIVN.FixedStorage.Identity.API.Controllers
{
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IIdentityService _identityService;
        private readonly ILogger<IIdentityService> _logger;
        private readonly HttpContext _httpContext;

        public AuthenController(SignInManager<AppUser> signInManager
            , IIdentityService identityService
            , IHttpContextAccessor httpContextAccessor
            , ILogger<IIdentityService> logger
            )
        {
            _signInManager = signInManager;
            _identityService = identityService;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
        }

        /// <summary>
        /// API Đăng nhập
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(Constants.Endpoint.API_Identity_Login)]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<LoginResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<LoginErrorDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<LoginErrorDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<LoginErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<LoginErrorDto>))]
        public async Task<IActionResult> Login(LoginDto model)
        {
            try
            {
                var check = await _identityService.ValidateLoginInput(model);
                if (check.IsInvalid)
                    return new JsonResult(new ResponseModel<LoginErrorDto>(check.Code.Value, check.Message, check.Data));

                var login = await _signInManager.PasswordSignInAsync(model.Username.Trim(), model.Password.Trim(), true, lockoutOnFailure: false);
                if (!login.Succeeded)
                {
                    check.Message = check.Data.Password = "Mật khẩu hoặc tài khoản không đúng";
                    return new JsonResult(new ResponseModel<LoginErrorDto>((int)HttpStatusCodes.ThePasswordIsNotCorrect, check.Message, check.Data));
                    //return Unauthorized(new ResponseModel<LoginErrorDto>((int)HttpStatusCodes.ThePasswordIsNotCorrect, check.Message, check.Data));
                }

                bool isAccountLoggingOtherDevice = !string.IsNullOrEmpty(model.DeviceId) ? true : false;

                var checkUser = await _identityService.GetUserInfoFromLoginAsync(model, isAccountLoggingOtherDevice);
                if (checkUser.Validation.IsInvalid)
                    return new JsonResult(new ResponseModel<LoginErrorDto>(checkUser.Validation.Code.Value, checkUser.Validation.Message, checkUser.Validation.Data));

                _identityService.CheckPasswordExpiration(checkUser);
                if (checkUser.Validation.IsInvalid)
                    return new JsonResult(new ResponseModel<LoginErrorDto>(checkUser.Validation.Code.Value, checkUser.Validation.Message, checkUser.Validation.Data));


                var result = await _identityService.GenerateJwtTokenAsync(checkUser.UserInfo);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return Unauthorized(new ResponseModel<LoginErrorDto>(result.Code, result.Message));
                }

                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet(Constants.Endpoint.API_Identity_Authorize_Token)]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<ValidateTokenResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        public async Task<IActionResult> AuthorizeToken()
        {
            try
            {
                var tokenInfo = _identityService.GetTokenInfo(HttpContext);
                var check = _identityService.ValidateTokenInfoInput(tokenInfo);
                if (check.IsInvalid)
                    return new JsonResult(new ResponseModel<ValidateTokenErrorDto>(check.Code.Value, check.Message, check.Data));
                var result = await _identityService.ValidateJwtTokenAsync(tokenInfo.Token);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel<ValidateTokenErrorDto>(result.Code, result.Message));
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return new JsonResult(new ResponseModel<ValidateTokenErrorDto>() { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost(Constants.Endpoint.API_Identity_Logout)]
        [@Authorize]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<LogoutResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<LogoutResultDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<LogoutResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<LogoutResultDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<LogoutResultDto>))]
        public async Task<IActionResult> Logout(LogoutDto userId)
        {
            var check = _identityService.ValidateLogoutInput(userId);
            if (check.IsInvalid)
                return BadRequest(new ResponseModel<LogoutResultDto>() { Code = check.Code.Value, Data = check.Data, Message = check.Message });
            try
            {
                check = await _identityService.UpdateDeviceTokenLogoutAsync(userId);
                if (check.IsInvalid)
                    return BadRequest(new ResponseModel<LogoutResultDto>() { Code = check.Code.Value, Data = check.Data, Message = check.Message });
                return Ok(new ResponseModel<LogoutResultDto>() { Code = check.Code.Value, Data = check.Data, Message = check.Message });
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return StatusCode(check.Code.Value, new ResponseModel<ValidateTokenResultDto>() { Code = check.Code.Value, Message = check.Message });
            }
        }

        [HttpPost]
        [InternalService]
        [Route(Constants.Endpoint.API_Identity_ForceLogout)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<LogoutResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<LogoutResultDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<LogoutResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<LogoutResultDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<LogoutResultDto>))]
        public async Task<IActionResult> ForceLogout([FromBody] string roleId)
        {
            if (!Guid.TryParse(roleId, out var _invalidRoleId))
            {
                return BadRequest(new ResponseModel<LogoutResultDto>() { Code = StatusCodes.Status400BadRequest, Message = "Nhóm quyền không hợp lệ" });
            }
            try
            {
                var userIdList = await _identityService.GetUserIdListByRoleIdAsync(roleId);
                var check = new ValidateDto<LogoutResultDto>(new LogoutResultDto()) { IsInvalid = false };
                check = await _identityService.UpdateDeviceTokenLogoutAsync(userIdList);
                if (check.IsInvalid)
                    return BadRequest(new ResponseModel<LogoutResultDto>() { Code = check.Code.Value, Data = check.Data, Message = check.Message });
                return Ok(new ResponseModel<LogoutResultDto>() { Code = check.Code.Value, Data = check.Data, Message = check.Message });
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpDelete]
        [InternalService]
        [Route(Constants.Endpoint.API_Identity_ForceLogoutUsers)]
        public async Task<IActionResult> ForceLogoutListUser([FromBody] List<string> UserIds)
        {
            try
            {
                var validParams = UserIds.Any() && UserIds.All(x => Guid.TryParse(x, out _));
                UserIds = UserIds.Distinct().ToList();

                if (!validParams)
                {
                    BadRequest(new ResponseModel() { Code = StatusCodes.Status400BadRequest, Message = Constants.ResponseMessages.InValidValidationMessage });
                }

                Dictionary<string, string> errors = new();
                foreach (var userId in UserIds)
                {
                    var logOutDto = new LogoutDto { UserId = Guid.Parse(userId) };
                    var response = await _identityService.UpdateDeviceTokenLogoutAsync(logOutDto);

                    if(response.Code != StatusCodes.Status200OK)
                    {
                        errors.TryAdd(userId, response.Message);
                    }
                }

                if (errors.Any())
                    return BadRequest(new ResponseModel { Code = StatusCodes.Status400BadRequest, Data = errors, Message = Constants.ResponseMessages.InValidValidationMessage });

                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet(Constants.Endpoint.API_Identity_Change_Password)]
        [@Authorize]
        [SwaggerResponse(StatusCodes.Status200OK)]
        public IActionResult ChangePassword()
        {
            return Ok(new ResponseModel<ChangePasswordResultDto>() { Code = StatusCodes.Status200OK, Data = new ChangePasswordResultDto { Success = true } });
        }

        [HttpPost(Constants.Endpoint.API_Identity_Change_Password)]
        [@Authorize]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<ChangePasswordResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<ChangePasswordResultDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<ChangePasswordResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<ChangePasswordErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<ChangePasswordResultDto>))]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            try
            {
                var checkChangePassword = _identityService.ValidateChangePasswordInput(model);
                if (checkChangePassword.IsInvalid)
                    return BadRequest(new ResponseModel<ChangePasswordErrorDto>() { Code = checkChangePassword.Code.Value, Data = checkChangePassword.Data, Message = checkChangePassword.Message });

                model.CurrentUserId = _httpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;

                var result = await _identityService.ChangePasswordAsync(model);
                if (result.Code != StatusCodes.Status200OK && result.Data.Success == false)
                    return BadRequest(new ResponseModel<ChangePasswordResultDto>() { Code = result.Code, Data = result.Data, Message = result.Message });

                var logOut = new LogoutDto() { UserId = Guid.Parse(model.UserId) };
                var checkLogOut = _identityService.ValidateLogoutInput(logOut);
                if (checkLogOut.IsInvalid)
                    return BadRequest(new ResponseModel<LogoutResultDto>() { Code = checkLogOut.Code.Value, Data = checkLogOut.Data, Message = checkLogOut.Message });

                checkLogOut = await _identityService.UpdateDeviceTokenLogoutAsync(logOut);
                if (checkLogOut.IsInvalid)
                    return BadRequest(new ResponseModel<ChangePasswordResultDto>() { Code = checkLogOut.Code.Value, Message = checkLogOut.Message, Data = new ChangePasswordResultDto { Success = false } });
                return Ok(new ResponseModel<ChangePasswordResultDto>() { Code = result.Code, Data = result.Data, Message = result.Message });
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return new JsonResult(new ResponseModel<ChangePasswordResultDto>() { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpGet]
        [@Authorize]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<bool>))]
        [Route("api/identity/authorize-permission/{permissionName}")]
        public async Task<IActionResult> AuthorizePermission(string permissionName)
        {
            if (string.IsNullOrEmpty(permissionName))
            {
                ModelState.AddModelError("Permission", "Vui lòng cung cấp tên quyền truy cập cần xác thực");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng kiểm tra lại quyền",
                    Data = ModelState.ErrorMessages()
                });
            }

            var result = await _identityService.AuthorizePermission(permissionName);
            if (result?.Data == false)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpDelete(Constants.Endpoint.API_Identity_Remove_Expired_Token)]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<ValidateTokenResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        public async Task<IActionResult> RemoveExpiredToken()
        {
            await _identityService.RemoveExpiredToken();
            return Ok();
        }

        [HttpGet(Constants.Endpoint.API_Identity_Reset_Password)]
        [@Authorize]
        [Roles(Constants.Roles.Administrator)]
        [SwaggerResponse(StatusCodes.Status200OK)]
        public IActionResult ResetPassword()
        {
            return Ok(new ResponseModel<ChangePasswordResultDto>() { Code = StatusCodes.Status200OK, Data = new ChangePasswordResultDto { Success = true } });
        }

        [HttpPost(Constants.Endpoint.API_Identity_Reset_Password)]
        [@Authorize]
        [Roles(Constants.Roles.Administrator)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<ChangePasswordResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<ChangePasswordResultDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<ChangePasswordResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<ChangePasswordErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<ChangePasswordResultDto>))]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            try
            {
                var checkChangePassword = _identityService.ValidateResetPasswordInput(model);
                if (checkChangePassword.IsInvalid)
                    return BadRequest(new ResponseModel<ResetPasswordErrorDto>() { Code = checkChangePassword.Code.Value, Data = checkChangePassword.Data, Message = checkChangePassword.Message });
                model.CurrentUserId = _httpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;

                var result = await _identityService.ResetPasswordAsync(model);
                if (result.Code != StatusCodes.Status200OK && result.Data.Success == false)
                    return new JsonResult(new ResponseModel<ChangePasswordResultDto>() { Code = result.Code, Data = result.Data, Message = result.Message });

                var logOut = new LogoutDto() { UserId = Guid.Parse(model.UserId) };
                var checkLogOut = _identityService.ValidateLogoutInput(logOut);
                if (checkLogOut.IsInvalid)
                    return BadRequest(new ResponseModel<LogoutResultDto>() { Code = checkLogOut.Code.Value, Data = checkLogOut.Data, Message = checkLogOut.Message });

                checkLogOut = await _identityService.UpdateDeviceTokenLogoutAsync(logOut);
                if (checkLogOut.IsInvalid)
                    return BadRequest(new ResponseModel<ChangePasswordResultDto>() { Code = checkLogOut.Code.Value, Message = checkLogOut.Message, Data = new ChangePasswordResultDto { Success = false } });
                return Ok(new ResponseModel<ChangePasswordResultDto>() { Code = result.Code, Data = result.Data, Message = result.Message });
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return new JsonResult(new ResponseModel<ChangePasswordResultDto>() { Code = StatusCodes.Status500InternalServerError });
            }
        }
        [HttpPost(Constants.Endpoint.API_Identity_Refresh_Token)]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<ValidateTokenResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<ValidateTokenErrorDto>))]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshToken)
        {
            try
            {
                var tokenInfo = _identityService.GetTokenInfo(HttpContext);
                var check = _identityService.ValidateTokenInfoInput(tokenInfo);
                if (check.IsInvalid)
                    return new JsonResult(new ResponseModel<ValidateTokenErrorDto>(check.Code.Value, check.Message, check.Data));
                var result = await _identityService.ValidateJwtRefreshTokenAsync(tokenInfo.Token, refreshToken.RefreshToken);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel<ValidateTokenErrorDto>(result.Code, result.Message));
                }
                var newToken = await _identityService.RefreshTokenAsync(new RefreshTokenDto
                {
                    DeviceId = refreshToken.DeviceId,
                    OldToken = tokenInfo.Token,
                    RefreshToken = refreshToken.RefreshToken
                });

                return Ok(newToken);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContext.Request.Path} : {exMess}; {innerExMess}");
                return new JsonResult(new ResponseModel<ValidateTokenErrorDto>() { Code = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
