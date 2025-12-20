namespace WebApp.Application.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RolesAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RolesAttribute(params string[] roles)
        {
            _roles = roles;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            var User = (ValidateTokenResultDto)context.HttpContext.Items[commonAPIConstant.HttpContextModel.UserKey];
            if (User == null)
            {
                context.Result = new RedirectToActionResult("Index", "Account", null);
                return;
            }

            if (_roles?.Any() == true)
            {
                if (!_roles.Any(x => x.ToLower() == User.RoleName.ToLower() || x.ToUpper() == User.RoleName.ToUpper()))
                {
                    context.Result = new JsonResult(new ResponseModel { Code = StatusCodes.Status401Unauthorized, Message = "Tài khoản không có quyền truy cập." }, JsonDefaults.CamelCasing) { StatusCode = StatusCodes.Status401Unauthorized };
                    return;
                }
            }
        }
    }
}
