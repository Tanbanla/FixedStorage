namespace WebApp.Application.Security
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute
    { }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permission;

        public AuthorizeAttribute(params string[] permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var _logger = context.HttpContext.RequestServices.GetService<ILogger<AuthorizeAttribute>>();

            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous) return;

            if (commonAPIConstant.Endpoint.EndpointList_ValidateTokenIn_ItselfIdentityServiceRequestHandlerMiddleware.Contains(context.HttpContext.Request.Path.ToString()))
            {
                return;
            }

            var User = (ValidateTokenResultDto)context.HttpContext.Items[commonAPIConstant.HttpContextModel.UserKey];
            if (User == null)
            {
                context.Result = new RedirectToActionResult("Index", "Account", null);
                return;
            }
            if (_permission?.Any() == true)
            {
                var roleClaims = User?.RoleClaims;
                //Check website 
                var relatedWebsitePermission = _permission?.Where(x => commonAPIConstant.Permissions.WebSitePermissionList.Contains(x));
                //if resource based on website permissions
                if (relatedWebsitePermission?.Any() == true)
                {
                    //First require WebsiteAccess
                    var allowWebsiteAccess = roleClaims?.Any(x => x.ClaimType == commonAPIConstant.Permissions.WEBSITE_ACCESS);
                    if (allowWebsiteAccess == false)
                    {
                        context.Result = new RedirectToActionResult("PreventAccessPage", "Home", null);
                        return;
                    }
                    //Next require permissions
                    var validPermission = relatedWebsitePermission?.Any(x => roleClaims?.Any(p => p.ClaimType == x) == true);
                    if (validPermission == false)
                    {
                        context.Result = new RedirectToActionResult("PreventAccessPage", "Home", null);
                        return;
                    }
                }

                var invalidPermission = !roleClaims.Any(p => _permission.Contains(p.ClaimType));
                if (invalidPermission)
                {
                    context.Result = new RedirectToActionResult("PreventAccessPage", "Home", null);
                    return;
                };
            }
        }
    }
}
