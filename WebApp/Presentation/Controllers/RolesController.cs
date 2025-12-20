using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace WebApp.Presentation.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly IRestClient _restClient;
        private readonly ILogger<RolesController> _logger;
        private readonly HttpContext _httpContext;

        public RolesController(IRestClient restClient,
                                ILogger<RolesController> logger,
                                IHttpContextAccessor httpContextAccessor
                                )
        {
            _restClient = restClient;
            _logger = logger;
            _httpContext = httpContextAccessor.HttpContext;
        }
        [RouteDisplayValue(RouteDisplay.ROLE)]
        [Route("roles")]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        [Authorize(commonAPIConstant.Permissions.ROLE_MANAGEMENT)]
        public async Task<IActionResult> Index()
        {
            var departmentReq = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Department);
            var factoryReq = new RestRequest(commonAPIConstant.Endpoint.api_Storage_Factory);
            var roleReq = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Role);

            var token = Request.TokenFromCookie();

            departmentReq.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
            factoryReq.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
            roleReq.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

            try
            {
                var departmentResult = await _restClient.ExecuteGetAsync(departmentReq);
                var convertedDepartmentResult = JsonSerializer.Deserialize<ResponseModel<IEnumerable<DepartmentDto>>>(departmentResult?.Content, JsonDefaults.CamelCaseOtions);
                ViewBag.DepartmentList = convertedDepartmentResult?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi get dữ liệu màn hình role", ex);
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            try
            {
                var factoryResult = await _restClient.ExecuteGetAsync(factoryReq);
                var convertedFactoryResult = JsonSerializer.Deserialize<ResponseModel<IEnumerable<FactoryInfoModel>>>(factoryResult?.Content, JsonDefaults.CamelCaseOtions);
                ViewBag.FactoryList = convertedFactoryResult?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi get dữ liệu màn hình role", ex);
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            try
            {
                var roleResult = await _restClient.ExecuteGetAsync(roleReq);
                var convertedRoleResult = JsonSerializer.Deserialize<ResponseModel<IEnumerable<RoleInfoModel>>>(roleResult?.Content, JsonDefaults.CamelCaseOtions);
                ViewBag.RoleList = convertedRoleResult?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi get dữ liệu màn hình role", ex);
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            try
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
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi lấy dữ liệu đa ngôn ngữ", ex);
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            return View();
        }


        [HttpPut("role/edit")]
        public async Task<IActionResult> EditRole([FromBody] EditRoleModel editRoleModel)
        {
            var token = Request.TokenFromCookie();
            var request = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Edit_Role, Method.Put);
            request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"Bearer {token}");
            request.AddHeader(commonAPIConstant.HttpContextModel.ContentTypeKey, commonAPIConstant.HttpContextModel.ApplicationJson);
            request.AddJsonBody(editRoleModel);

            var response = await _restClient.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                TempData["RoleResponseModel"] = response?.Content ?? "";
                var responseModel = JsonSerializer.Deserialize<ResponseModel>(response?.Content ?? "", JsonDefaults.CamelCasing);
                return Ok(responseModel);
            }
            else
            {
                var responseModel = JsonSerializer.Deserialize<ResponseModel>(response?.Content ?? "", JsonDefaults.CamelCasing);
                return BadRequest(responseModel);
            }
        }

        [HttpPost("role/create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleModel createRoleModel)
        {
            var token = Request.TokenFromCookie();
            var request = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Role_Create, Method.Post);
            request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"Bearer {token}");
            request.AddHeader(commonAPIConstant.HttpContextModel.ContentTypeKey, commonAPIConstant.HttpContextModel.ApplicationJson);
            request.AddJsonBody(createRoleModel);

            var response = await _restClient.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                TempData["RoleResponseModel"] = response?.Content ?? "";
                var responseModel = JsonSerializer.Deserialize<ResponseModel>(response?.Content ?? "", JsonDefaults.CamelCasing);
                return Ok(responseModel);
            }
            else
            {
                var responseModel = JsonSerializer.Deserialize<ResponseModel>(response?.Content ?? "", JsonDefaults.CamelCasing);
                return BadRequest(responseModel);
            }
        }

        [HttpDelete("role/delete/{roleId}/{userId}")]
        public async Task<IActionResult> DeleteRole(string roleId, string userId)
        {
            var token = Request.TokenFromCookie();
            var request = new RestRequest($"api/identity/role/delete/{roleId}/{userId}", Method.Delete);
            request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"Bearer {token}");
            request.AddHeader(commonAPIConstant.HttpContextModel.ContentTypeKey, commonAPIConstant.HttpContextModel.ApplicationJson);

            var response = await _restClient.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                TempData["RoleResponseModel"] = response?.Content ?? "";
                var responseModel = JsonSerializer.Deserialize<ResponseModel>(response?.Content ?? "", JsonDefaults.CamelCasing);
                return Ok(responseModel);
            }
            else
            {
                var responseModel = JsonSerializer.Deserialize<ResponseModel>(response?.Content ?? "", JsonDefaults.CamelCasing);
                return BadRequest(responseModel);
            }
        }

        [HttpGet("user/refresh-info")]
        public async Task<IActionResult> NewestUserInfo()
        {
            var token = Request.Headers[commonAPIConstant.HttpContextModel.AuthorizationKey];
            var restRequest = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Authorize_Token);

            restRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
            try
            {
                var response = await _restClient.GetAsync(restRequest);
                var responseModel = JsonSerializer.Deserialize<ResponseModel<ValidateTokenResultDto>>(response?.Content, JsonDefaults.CamelCaseOtions);
                var User = responseModel?.Data;
                
                HttpContext.Items[commonAPIConstant.HttpContextModel.UserKey] = User;
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
                HttpContext.User.AddIdentity(claimIdentity);
                HttpContext.User = new ClaimsPrincipal(claimIdentity);

                _logger.LogInformation("Đính user thành công");

                return Ok(User);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi xác thực token WebApp. Xác thực thất bại");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            return BadRequest();
        }
    }
}
