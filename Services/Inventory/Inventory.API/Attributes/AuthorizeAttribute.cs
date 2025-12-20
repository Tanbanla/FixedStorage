using BIVN.FixedStorage.Services.Common.API.Exceptions;

namespace BIVN.FixedStorage.Services.Inventory.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InternalServiceAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute
    { }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permission;

        public AuthorizeAttribute(params string[] permissions)
        {
            _permission = permissions;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
                var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
                if (allowAnonymous)
                    return;

                var allowInternalService = context.ActionDescriptor.EndpointMetadata.OfType<InternalServiceAttribute>().Any();
                if (allowInternalService == true
                    && context.HttpContext.Request.Headers[commonAPIConstant.HttpContextModel.ClientIdKey].FirstOrDefault()?.ToString() == configuration[commonAPIConstant.AppSettings.ClientId]
                    && context.HttpContext.Request.Headers[commonAPIConstant.HttpContextModel.ClientSecretKey].FirstOrDefault()?.ToString() == configuration[commonAPIConstant.AppSettings.ClientSecret])
                {
                    return;
                }

                ValidateTokenResultDto user = (ValidateTokenResultDto)context.HttpContext.Items[commonAPIConstant.HttpContextModel.UserKey];
                if (user == null) throw new InvalidAuthorizeException(StatusCodes.Status400BadRequest, message: "Không tìm thấy thông tin người dùng.");

                var roleClaims = user?.RoleClaims;

                //Nếu là xúc tiến inventory thì không check quyền gì
                var isInventoryPromotion = user?.InventoryLoggedInfo?.InventoryRoleType == (int)InventoryAccountRoleType.Promotion;
                if (isInventoryPromotion)
                {
                    return;
                }

                //Check website 
                var relatedWebsitePermission = _permission?.Where(x => Constants.Permissions.WebSitePermissionList.Contains(x));
                //if resource based on website permissions
                if (relatedWebsitePermission?.Any() == true)
                {
                    //First require WebsiteAccess
                    var allowWebsiteAccess = roleClaims?.Any(x => x.ClaimType == Constants.Permissions.WEBSITE_ACCESS);
                    if (allowWebsiteAccess == false) throw new InvalidAuthorizeException(StatusCodes.Status403Forbidden, message: "Không có quyền truy cập Website.");

                    //Next require permissions
                    var validPermission = relatedWebsitePermission?.Any(x => roleClaims?.Any(p => p.ClaimType == x) == true);
                    if (validPermission == false) throw new InvalidAuthorizeException(StatusCodes.Status403Forbidden, message: commonAPIConstant.ResponseMessages.UnAuthorized);
                }

                //Check mobile
                var relatedMobilePermission = _permission?.Where(x => Constants.Permissions.MobilePermissionList.Contains(x));
                if (relatedMobilePermission?.Any() == true)
                {
                    //First require WebsiteAccess
                    var allowMobileAccess = roleClaims?.Any(x => x.ClaimType == Constants.Permissions.MOBILE_ACCESS);
                    if (allowMobileAccess == false) throw new InvalidAuthorizeException(StatusCodes.Status403Forbidden, message: "Không có quyền truy cập Mobile.");

                    var DeviceId = context.HttpContext.Request.Headers[commonAPIConstant.HttpContextModel.DeviceId].LastOrDefault();
                    if (string.IsNullOrEmpty(DeviceId)) throw new InvalidAuthorizeException(StatusCodes.Status403Forbidden, message: "Quyền Mobile yêu cầu cung cấp Device Id");

                    //Next require permissions
                    var validPermission = relatedMobilePermission?.Any(x => roleClaims?.Any(p => p.ClaimType == x) == true);
                    if (validPermission == false) throw new InvalidAuthorizeException(StatusCodes.Status403Forbidden, message: commonAPIConstant.ResponseMessages.UnAuthorized);
                }

                var invalidPermission = !roleClaims.Any(p => _permission.Contains(p.ClaimType));
                if (invalidPermission) throw new InvalidAuthorizeException(StatusCodes.Status403Forbidden, message: commonAPIConstant.ResponseMessages.UnAuthorized);
            }
            catch(InvalidAuthorizeException e)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<AuthorizeAttribute>>();
                logger.LogError("Có Lỗi khi xác thực phân quyền.");
                logger.LogError(e.Message);


                context.HttpContext.Response.StatusCode = e.StatusCode;
                context.Result = new JsonResult(new ResponseModel
                {
                    Code = e.StatusCode,
                    Message = e.Message
                }, JsonDefaults.CamelCasing);
               
                return;
            }
        }
    }
}
