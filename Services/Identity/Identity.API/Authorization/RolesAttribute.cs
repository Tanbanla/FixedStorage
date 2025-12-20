namespace BIVN.FixedStorage.Identity.API.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RolesAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RolesAttribute(params string[] roles)
        {
            _roles = roles ?? new string[] { };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            // If request is logout user then jump into api logout
            if (context.HttpContext.Request.Path.ToString().ToLower() == Constants.Endpoint.WebApp_Logout.ToLower()
                || context.HttpContext.Request.Path.ToString().ToLower() == Constants.Endpoint.API_Identity_Logout.ToLower())
            {
                return;
            }

            var user = context.HttpContext.User.Identities.FirstOrDefault(x => x.AuthenticationType == Constants.HttpContextModel.AuthorizationKey);
            if (user == null)
            {
                try
                {
                    var authorizationError = (ResponseModel)context.HttpContext.Items["AuthorizationError"];
                    context.Result = new JsonResult(new ResponseModel
                    {
                        Code = authorizationError?.Code ?? StatusCodes.Status400BadRequest,
                        Message = authorizationError?.Message ?? string.Empty                    
                    }, JsonDefaults.CamelCasing)
                    { StatusCode = authorizationError?.Code ?? StatusCodes.Status400BadRequest };
                }
                catch
                {
                    context.Result = new JsonResult(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Không lấy được thông tin Tài khoản"
                    }, JsonDefaults.CamelCasing)
                    { StatusCode = StatusCodes.Status400BadRequest };
                }
            }
          
            // Website
            if (user.Claims.Any(x => x.Type == Constants.UserClaims.DeviceId && string.IsNullOrEmpty(x.Value)))
            {
                // does not have any role
                if (user.Claims.Any(x => x.Type == Constants.UserClaims.RoleId && string.IsNullOrEmpty(x.Value)))
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status401Unauthorized, Message = "Tài khoản không tồn tại quyền" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status401Unauthorized };
                }
                // doesn't have website access permission
                else if (!user.Claims.Any(x => x.Type == Constants.Permissions.WEBSITE_ACCESS))
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status401Unauthorized, Message = "Không có quyền truy cập Website" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status401Unauthorized };
                }

                // has website access permission
                // user does not have any roles that matches to API permission requirement
                else if (user.Claims.Any(x => x.Type == Constants.Permissions.WEBSITE_ACCESS) && _roles?.Any() == true && !user.Claims.Any(x => _roles.Select(x => x.ToLower()).Contains(x.Value.ToLower())))
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status401Unauthorized, Message = "Không có quyền truy cập" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status401Unauthorized };
                }
            }
        }
    }
}
