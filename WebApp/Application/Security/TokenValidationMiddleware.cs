namespace WebApp.Application.Security
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;
        private readonly IRestClient _restClient;

        public TokenValidationMiddleware(RequestDelegate next,
                                            ILogger<TokenValidationMiddleware> logger,
                                            IRestClient restClient)
        {
            _next = next;
            _logger = logger;
            _restClient = restClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var loginRoute = Path.Join(Application.Constants.GlobalRoutePrefix, commonAPIConstant.AppSettings.LoginRoute);

            var tokenFromCookie = context.Request.Cookies[commonAPIConstant.HttpContextModel.TokenKey];
            var tokenFromHeader = context.Request.Headers[commonAPIConstant.HttpContextModel.AuthorizationKey].FirstOrDefault()?.Split(" ").Last();

            var endpoint = context.GetEndpoint();
            var isAllowAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;
            if (isAllowAnonymous)
            {
                //Get user if exist signed in user
                if (!string.IsNullOrEmpty(tokenFromCookie))
                {
                    await AttachSignedInUser(context, tokenFromCookie);
                }

                await _next(context);
                return;
            }

            if (string.IsNullOrEmpty(tokenFromCookie))
            {
                context.Response.Redirect(loginRoute);
                return;
            }

            //Only call API when endpoint is page, not static files
            if (endpoint != null)
            {
                var restRequest = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Authorize_Token);
                restRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{tokenFromCookie ?? tokenFromHeader}");

                try
                {
                    var response = await _restClient.GetAsync(restRequest);
                    _logger.LogInformation("Gọi API check token");

                    var responseModel = JsonSerializer.Deserialize<ResponseModel<ValidateTokenResultDto>>(response?.Content, JsonDefaults.CamelCaseOtions);
                    if (responseModel?.Code != StatusCodes.Status200OK)
                    {
                        context.Response.Redirect(loginRoute);
                        return;
                    }

                    var User = responseModel.Data;
                    if (User == null)
                    {
                        context.Response.Redirect(loginRoute);
                        return;
                    }

                    context.Items[commonAPIConstant.HttpContextModel.UserKey] = User;
                    var claimIdentity = new ClaimsIdentity(authenticationType: commonAPIConstant.HttpContextModel.UserKey);
                    var claim = new[]
                    {
                        new Claim(commonAPIConstant.UserClaims.UserId, User.UserId ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Username, User.Username ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.FullName, User.Fullname ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.DepartmentId, User.DepartmentId ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.DepartmentName, User.DepartmentName ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.RoleId, User.RoleId ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.RoleName, User.RoleName ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Avatar, User.Avatar ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.AccountType, User.AccountType ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Email, User.Email ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Phone, User.Phone ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Code, User.UserCode ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.DeviceId, User.DeviceId ?? string.Empty),
                         //Inventory info
                        new Claim(commonAPIConstant.UserClaims.InventoryId, User?.InventoryLoggedInfo?.InventoryModel?.InventoryId.ToString() ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.InventoryDate, JsonSerializer.Serialize(User?.InventoryLoggedInfo?.InventoryModel?.InventoryDate) ?? string.Empty),
                    };

                    claimIdentity.AddClaims(claim);
                    claimIdentity.AddClaims(User?.RoleClaims.Select(x => new Claim(x.ClaimType, x.ClaimValue)));
                    context.User.AddIdentity(claimIdentity);
                    context.User = new ClaimsPrincipal(claimIdentity);

                    _logger.LogInformation("Đính user thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Có lỗi khi xác thực token WebApp. Xác thực thất bại");
                    _logger.LogError($"Error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        _logger.LogError($"With inner exception: {ex.InnerException.Message}");
                    }

                    context.Response.Redirect(loginRoute);
                    return;
                }
            }

            await _next(context);
        }

        private async Task AttachSignedInUser(HttpContext context, string token)
        {
            var restRequest = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Authorize_Token);
            restRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");

            try
            {
                var response = await _restClient.GetAsync(restRequest);
                var responseModel = JsonSerializer.Deserialize<ResponseModel<ValidateTokenResultDto>>(response?.Content, JsonDefaults.CamelCaseOtions);
                var User = responseModel?.Data;
                if (User == null)
                {
                    return;
                }
                else
                {
                    context.Items[commonAPIConstant.HttpContextModel.UserKey] = User;
                    var claimIdentity = new ClaimsIdentity(authenticationType: commonAPIConstant.HttpContextModel.UserKey);
                    var claim = new[]
                    {
                        new Claim(commonAPIConstant.UserClaims.UserId, User.UserId ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Username, User.Username ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.FullName, User.Fullname ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.DepartmentId, User.DepartmentId ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.DepartmentName, User.DepartmentName ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.RoleId, User.RoleId ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.RoleName, User.RoleName ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Avatar, User.Avatar ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.AccountType, User.AccountType ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Email, User.Email ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Phone, User.Phone ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.Code, User.UserCode ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.DeviceId, User.DeviceId ?? string.Empty),
                        //Inventory info
                        new Claim(commonAPIConstant.UserClaims.InventoryId, User?.InventoryLoggedInfo?.InventoryModel?.InventoryId.ToString() ?? string.Empty),
                        new Claim(commonAPIConstant.UserClaims.InventoryDate, JsonSerializer.Serialize(User?.InventoryLoggedInfo?.InventoryModel?.InventoryDate) ?? string.Empty),
                    };

                    claimIdentity.AddClaims(claim);
                    claimIdentity.AddClaims(User?.RoleClaims.Select(x => new Claim(x.ClaimType, x.ClaimValue)));
                    context.User.AddIdentity(claimIdentity);
                    context.User = new ClaimsPrincipal(claimIdentity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Không lấy được thông tin người dùng đã đăng nhập");
                _logger.LogError(ex.Message);
            }
        }
    }
}
