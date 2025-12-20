namespace BIVN.FixedStorage.Services.Inventory.API.Middlewares
{
    public class ValidateTokenMiddlware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IRestClient _restClient;
        private readonly ILogger<ValidateTokenMiddlware> _logger;

        public ValidateTokenMiddlware(RequestDelegate next,
                            IConfiguration configuration,
                            IRestClient restClient,
                            ILogger<ValidateTokenMiddlware> logger
                            )
        {
            _next = next;
            _configuration = configuration;
            _restClient = restClient;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var allowAnonymous = endpoint.Metadata.GetMetadata<BIVN.FixedStorage.Services.Inventory.API.Attributes.AllowAnonymousAttribute>() != null;
                if (allowAnonymous == true)
                {
                    await _next(context);
                    return;
                }

                var allowInternalService = endpoint.Metadata.GetMetadata<InternalServiceAttribute>() != null;
                if (allowInternalService == true
                    && context.Request.Headers[commonAPIConstant.HttpContextModel.ClientIdKey].FirstOrDefault()?.ToString() == _configuration[commonAPIConstant.AppSettings.ClientId]
                    && context.Request.Headers[commonAPIConstant.HttpContextModel.ClientSecretKey].FirstOrDefault()?.ToString() == _configuration[commonAPIConstant.AppSettings.ClientSecret])
                {
                    await _next(context);
                    return;
                }
            }

            var ClientId = context.Request.Headers[commonAPIConstant.HttpContextModel.ClientIdKey].FirstOrDefault()?.Split(" ").Last();
            var ClientSecret = context.Request.Headers[commonAPIConstant.HttpContextModel.ClientSecretKey].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret))
            {
                if (CheckInternalRequestHeader(ClientId, ClientSecret))
                {
                    await _next(context);
                    return;
                }
                else
                {
                    var reponseModel = new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = Constants.ResponseMessages.InvalidToken
                    };
                    _logger.LogError($"Validate token - Path: {context.Request.Path} ");
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(reponseModel, JsonDefaults.CamelCasing));
                    return;
                }
            }

            var token = context.Request.Headers[commonAPIConstant.HttpContextModel.AuthorizationKey].FirstOrDefault()?.Split(" ").Last();
            var deviceId = context.Request.Headers[commonAPIConstant.HttpContextModel.DeviceId].LastOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var reponseModel = new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = Constants.ResponseMessages.InvalidToken
                };

                _logger.LogError($"Validate token - Path: {context.Request.Path}");
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(reponseModel, JsonDefaults.CamelCasing));
                return;
            }

            var restRequest = new RestRequest(Constants.Endpoint.API_Identity_Authorize_Token);
            restRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"Bearer {token}");
            restRequest.AddHeader(commonAPIConstant.HttpContextModel.DeviceId, deviceId ?? "");

            try
            {
                var response = await _restClient.GetAsync(restRequest);
                var responseModel = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<ValidateTokenResultDto>>(response.Content, JsonDefaults.CamelCasing);
                if (responseModel?.Code != StatusCodes.Status200OK)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(responseModel, JsonDefaults.CamelCasing));
                    return;
                }

                //Token passed
                context.Items[commonAPIConstant.HttpContextModel.UserKey] = responseModel.Data;
                ValidateTokenResultDto User = responseModel.Data;
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
                    new Claim(commonAPIConstant.UserClaims.DeviceId, User.DeviceId ?? string.Empty)
                };
                claimIdentity.AddClaims(claim);
                claimIdentity.AddClaims(User?.RoleClaims.Select(x => new Claim(x.ClaimType, x.ClaimValue)));
                context.User.AddIdentity(claimIdentity);
                context.User = new ClaimsPrincipal(claimIdentity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Validate token - Path: {context.Request.Path} - Error: {ex.Message}");

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var reponseModel = new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = Constants.ResponseMessages.InvalidToken
};

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(reponseModel, JsonDefaults.CamelCasing));
                return;
            }

            await _next(context);
        }
        private bool CheckInternalRequestHeader(string clientId = "", string clientSecret = "")
        {
            string appClientIdValue = _configuration[commonAPIConstant.AppSettings.ClientId] ?? string.Empty;
            string appClientSecretValue = _configuration[commonAPIConstant.AppSettings.ClientSecret] ?? string.Empty;

            return clientId == appClientIdValue && clientSecret == appClientSecretValue;
        }
    }
}
