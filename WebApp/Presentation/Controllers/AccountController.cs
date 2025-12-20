using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace WebApp.Presentation.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IRestClient _restClient;
        private readonly IAccountService _accountService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(ILogger<AccountController> logger,
                              IOptions<APIGateWay> APIGateWayOptions,
                              IRestClient restClient,
                              IAccountService accountService,
                              IHttpContextAccessor httpContextAccessor
                              )
        {
            _logger = logger;
            _restClient = restClient;
            _accountService = accountService;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("login")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var defaultCultures = new List<CultureInfo>()
            {
                new CultureInfo("vi-VN"),
                new CultureInfo("en-US"),
            };

            CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var cultureItems = cinfo.Where(x => defaultCultures.Contains(x))
                .Select(c => new SelectListItem { Value = c.Name, Text = c.DisplayName })
                .ToList();
            ViewBag.Cultures = cultureItems;
            var isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginDto model)
        {
            var check = _accountService.ValidateLoginInput(model);
            if (check.IsInvalid)
                return BadRequest(new ResponseModel<LoginErrorDto>(check.Code.Value, check.Message, check.Data));
            try
            {
                var allowOverrideLoginPersonalAccount = _httpContextAccessor.HttpContext.Request.Headers["AllowOverrideLoginPersonalAccount"].FirstOrDefault()?.ToString() ?? string.Empty;
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Login);
                request.AddHeader("AllowOverrideLoginPersonalAccount", allowOverrideLoginPersonalAccount);
                request.AddBody(model);
                //var response = await _restClient.PostAsync(request);
                var response = await _restClient.ExecutePostAsync(request);

                if (!response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var invalidResult = JsonConvert.DeserializeObject<ResponseModel<LoginResultDto>>(response.Content);
                    return BadRequest(invalidResult);
                }

                if (response?.IsSuccessful == false || string.IsNullOrEmpty(response.Content))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<LoginResultDto>>(response?.Content);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                if (result.Code != StatusCodes.Status200OK)
                {
                    return Unauthorized(new ResponseModel<LoginResultDto>(result.Code, result?.Message, result?.Data));
                }
                TempData["ExpirePasswordNotification"] = !string.IsNullOrEmpty(result.Data.NotificationBeforePasswordExpire) ? result.Data.NotificationBeforePasswordExpire : string.Empty;
                _accountService.SetUserCookies(result.Data);
                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.WebApp_Logout)]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.UserId];
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return BadRequest(new ResponseModel<LogoutResultDto>(StatusCodes.Status400BadRequest, commonAPIConstant.ResponseMessages.InValidValidationMessage, new LogoutResultDto() { Fail = userIdStr }));
            }

            var tokenFromCookie = HttpContext.Request.Cookies[commonAPIConstant.HttpContextModel.TokenKey];
            var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Logout);
            var requestParams = new LogoutDto { UserId = userId };

            request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{tokenFromCookie}");
            request.AddBody(requestParams);

            var response = await _restClient.ExecutePostAsync(request);
            if (response?.IsSuccessful == true)
            {
                var logoutResult = JsonConvert.DeserializeObject<ResponseModel<LogoutResultDto>>(response?.Content ?? string.Empty);
                if (logoutResult != null)
                {
                    _accountService.DeleteUserCookies();
                    return RedirectToAction("Index", "Home");
                }
            }

            _accountService.DeleteUserCookies();
            return RedirectToAction("Index", "Home");

            //var errorModel = new LogoutResultDto() { Fail = userIdStr };
            //return View(errorModel);
        }

        [HttpGet(commonAPIConstant.Endpoint.WebApp_Change_Password)]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync()
        {
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Change_Password);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                var response = await _restClient.GetAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<ChangePasswordResultDto>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                else if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel<ChangePasswordResultDto>() { Code = result.Code, Message = result.Message, Data = new ChangePasswordResultDto { Success = false } });
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.WebApp_Change_Password)]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto model)
        {
            try
            {
                var check = _accountService.ValidateChangePasswordInput(model);
                if (check.IsInvalid)
                    return BadRequest(new ResponseModel<ChangePasswordErrorDto>(check.Code.Value, check.Message, check.Data));

                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Change_Password);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                request.AddBody(model);
                var response = await _restClient.ExecutePostAsync(request);
                if (response?.IsSuccessful == false && string.IsNullOrEmpty(response.Content))
                {
                    return BadRequest(new ResponseModel<ChangePasswordResultDto>() { Code = StatusCodes.Status500InternalServerError, Message = "Đăng nhập không thành công" });
                }

                var result = JsonConvert.DeserializeObject<ResponseModel<ChangePasswordResultDto>>(response?.Content);
                if (result == null)
                {
                    return BadRequest(new ResponseModel<ChangePasswordResultDto>() { Code = StatusCodes.Status500InternalServerError, Message = "Đăng nhập không thành công" });
                }
                if (result.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(new ResponseModel<ChangePasswordErrorDto>() { Code = result.Code, Message = result.Message });
                }
                _accountService.DeleteUserCookies();
                return Ok(result);
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.WebApp_Reset_Password)]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        [Roles(commonAPIConstant.Roles.Administrator)]
        public async Task<IActionResult> ResetPasswordAsync()
        {
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Reset_Password);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                var response = await _restClient.ExecuteGetAsync(request);
                if (response?.IsSuccessful == false && string.IsNullOrEmpty(response.Content))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<ChangePasswordResultDto>>(response?.Content);
                if (result == null || result.Code != StatusCodes.Status200OK)
                {
                    return Unauthorized(new ResponseModel<ChangePasswordResultDto>() { Code = result.Code, Message = result.Message });
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);

                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.WebApp_Reset_Password)]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        [Roles(commonAPIConstant.Roles.Administrator)]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            try
            {
                var check = _accountService.ValidateResetPasswordInput(model);
                if (check.IsInvalid)
                    return BadRequest(new ResponseModel<ResetPasswordErrorDto>(check.Code.Value, check.Message, check.Data));

                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Reset_Password);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                request.AddBody(model);
                var response = await _restClient.ExecutePostAsync(request);
                if (response?.IsSuccessful == false && string.IsNullOrEmpty(response.Content))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<ChangePasswordResultDto>>(response?.Content);
                if (result == null || result.Code != StatusCodes.Status200OK)
                {
                    return Unauthorized(new ResponseModel<ChangePasswordResultDto>() { Code = result.Code, Message = result.Message });
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
