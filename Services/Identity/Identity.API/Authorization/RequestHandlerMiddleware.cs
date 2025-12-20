namespace BIVN.FixedStorage.Identity.API.Authorization
{
    /// <summary>
    /// Request handler for all request from inside itself identity service
    /// </summary>
    public class RequestHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IIdentityService> _logger;

        public RequestHandlerMiddleware(RequestDelegate next,
                            IConfiguration configuration,
                            ILogger<IIdentityService> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IIdentityService _identityService)
        {
            context.Response.ContentType = "application/json";
            try
            {
                if (context.Request.Path.ToString().Contains(@Constants.Endpoint.API_Identity_Login))
                {
                    await _next(context);
                    return;
                }

                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    var allowAnonymous = endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
                    if (allowAnonymous == true)
                    {
                        await _next(context);
                        return;
                    }

                    var allowBackgroundJob = endpoint.Metadata.GetMetadata<BackgroundJobAttribute>() != null;
                    if (allowBackgroundJob == true
                        && context.Request.Headers[Constants.HttpContextModel.ClientIdKey].FirstOrDefault()?.ToString() == _configuration[Constants.AppSettings.ClientId]
                        && context.Request.Headers[Constants.HttpContextModel.ClientSecretKey].FirstOrDefault()?.ToString() == _configuration[Constants.AppSettings.ClientSecret])
                    {
                        await _next(context);
                        return;
                    }
                    
                    var allowInternalService = endpoint.Metadata.GetMetadata<InternalServiceAttribute>() != null;
                    if (allowInternalService == true
                        && context.Request.Headers[Constants.HttpContextModel.ClientIdKey].FirstOrDefault()?.ToString() == _configuration[Constants.AppSettings.ClientId]
                        && context.Request.Headers[Constants.HttpContextModel.ClientSecretKey].FirstOrDefault()?.ToString() == _configuration[Constants.AppSettings.ClientSecret])
                    {
                        await _next(context);
                        return;
                    }                    
                }

                // If request is login, authorize token, lock users background job, remove expired token...
                // then in exception endpoint list inside itself identity service that don't require token validation
                if (Constants.Endpoint.ExceptionEndpointList_IdentityService_NoRequireTokenValidation.Contains(context.Request.Path.ToString()))
                {
                    await _next(context);
                    return;
                }
                
                #region Token validation for all internal api from inside identity service

                // External requests outside identity service will go to api/identity/authorize-token
                var tokenInfo = _identityService.GetTokenInfo(context);
                var check = _identityService.ValidateTokenInfoInput(tokenInfo);
                if (check.IsInvalid)
                {
                    context.Response.StatusCode = check.Code.Value;
                    var response = new ResponseModel<ValidateTokenErrorDto>
                    {
                        Code = check.Code.Value,
                        Message = check.Message,
                        Data = check.Data
                    };
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, JsonDefaults.CamelCasing));
                    context.Items["AuthorizationErrors"] = response;
                    return;
                }

                var result = await _identityService.ValidateJwtTokenAsync(tokenInfo.Token);
                if (result.Code != StatusCodes.Status200OK)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    var response = new ResponseModel<ValidateTokenErrorDto>
                    {
                        Code = result.Code,
                        Message = result.Message,
                        Data = check.Data
                    };
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, JsonDefaults.CamelCasing));
                    context.Items["AuthorizationErrors"] = response;
                    return;
                }

                var claimsIdentity = _identityService.CreateClaimsIdentity(result.Data);
                context.User.AddIdentity(claimsIdentity);

                #endregion Token validation for all internal api from inside identity service

                await _next(context);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {context.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new ResponseModel { Code = StatusCodes.Status500InternalServerError }, JsonDefaults.CamelCasing));
                return;
            }
        }
    }
}
