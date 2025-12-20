namespace BIVN.FixedStorage.Identity.API.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AllowAnonymousAttribute : Attribute
    { }

    /// <summary>
    /// Authorize for only all requests from itself identity service
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;

        public AuthorizeAttribute(params string[] permissions)
        {
            _permissions = permissions ?? new string[] { };
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
                        //Code = StatusCodes.Status400BadRequest,
                        //Message = "Không lấy được thông tin Tài khoản"
                    }, JsonDefaults.CamelCasing) { StatusCode = authorizationError?.Code ?? StatusCodes.Status400BadRequest };
                }
                catch
                {                    
                    context.Result = new JsonResult(new ResponseModel
                    {                      
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Không lấy được thông tin Tài khoản"
                    }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status400BadRequest };
                }               
            }     
            
            // Mobile
            if (user.Claims.Any(x => x.Type == Constants.UserClaims.DeviceId && !string.IsNullOrEmpty(x.Value)))
            {
                // does not have any role
                if (user.Claims.Any(x => x.Type == Constants.UserClaims.RoleId && string.IsNullOrEmpty(x.Value))){
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status401Unauthorized, Message = "Tài khoản không tồn tại quyền" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status401Unauthorized };
                }
                // doesn't have mobile access permission
                else if (!user.Claims.Any(x => x.Type == Constants.Permissions.MOBILE_ACCESS))
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status403Forbidden, Message = "Không có quyền truy cập App Mobile" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status403Forbidden };
                }
                // User has 'Mobile access' permission but not has 'MC' or 'PBC' permission
                else if (user.Claims.Any(x => x.Type == Constants.Permissions.MOBILE_ACCESS)
                        && _permissions?.Any() == true && !user.Claims.Any(x => _permissions.Contains(x.Type)))                        
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status403Forbidden, Message = "Không có quyền truy cập của nghiệp vụ MC hoặc PCB" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status403Forbidden };                   
                }                
            }

            // Website
            else if (user.Claims.Any(x => x.Type == Constants.UserClaims.DeviceId && string.IsNullOrEmpty(x.Value)))
            {
                // does not have any role
                if (user.Claims.Any(x => x.Type == Constants.UserClaims.RoleId && string.IsNullOrEmpty(x.Value)))
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status401Unauthorized, Message = "Tài khoản không tồn tại quyền" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status401Unauthorized };
                }
                // doesn't have website access permission
                else if (!user.Claims.Any(x => x.Type == Constants.Permissions.WEBSITE_ACCESS))
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status403Forbidden, Message = "Không có quyền truy cập Website" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status403Forbidden };
                }

                // has website access permission
                // API required at least one specific permission of Website permissions list and user does not have any permission that matches to permission of API
                else if (user.Claims.Any(x => x.Type == Constants.Permissions.WEBSITE_ACCESS) 
                    && _permissions?.Any() == true && !user.Claims.Any(x => _permissions.Contains(x.Type)))
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status403Forbidden, Message = "Không có quyền truy cập" }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status403Forbidden };                    
                }                
            }
        }
    }
}
