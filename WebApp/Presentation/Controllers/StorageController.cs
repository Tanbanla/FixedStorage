namespace WebApp.Presentation.Controllers
{
    [Authorize]
    public class StorageController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IRestClient _restClient;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;

        public StorageController(
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<UsersController> logger,
                              IRestClient restClient,
                              IConfiguration configuration
                                )
        {
            _logger = logger;
            _restClient = restClient;
            _configuration = configuration;
            _httpContext = httpContextAccessor.HttpContext;
        }

        [RouteDisplayValue(RouteDisplay.STORAGE)]
        [Authorize(commonAPIConstant.Permissions.INPUT)]
        [HttpGet("storage")]
        public async Task<IActionResult> Index()
        {
            var factoryReq = new RestRequest(commonAPIConstant.Endpoint.api_Storage_Factory);
            var token = Request.TokenFromCookie();
            factoryReq.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"Bearer {token}");

            var factoryResult = await _restClient.ExecuteGetAsync(factoryReq);
            var responseModel = JsonSerializer.Deserialize<ResponseModel<IEnumerable<FactoryInfoModel>>>(factoryResult?.Content ?? "", JsonDefaults.CamelCasing);
            ViewBag.Factories = responseModel?.Data;

            return View();
        }

        [RouteDisplayValue(RouteDisplay.LAYOUT)]
        [HttpGet("list-layout")]
        [Authorize(commonAPIConstant.Permissions.MASTER_DATA_READ, commonAPIConstant.Permissions.MASTER_DATA_WRITE, commonAPIConstant.Permissions.FACTORY_DATA_INQUIRY)]
        public async Task<IActionResult> ListLayout()
        {
            var request = new RestRequest(commonAPIConstant.Endpoint.API_Storage_Get_Layout_List);
            var token = _httpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
            request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
            var response = await _restClient.GetAsync(request);
            if (response?.IsSuccessful == false)
            {
                return BadRequest(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
            if (string.IsNullOrEmpty(response?.Content))
            {
                return BadRequest(new ResponseModel { Code = (int)response.StatusCode, Message = "Không có bản ghi nào" });
            }
            var result = JsonConvert.DeserializeObject<ResponseModel<List<LayoutDto>>>(response.Content);
            if (result == null)
            {
                return BadRequest(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
            else if (result.Code != StatusCodes.Status200OK)
            {
                return BadRequest(new ResponseModel<List<LayoutDto>>(result.Code, result.Message, result.Data));
            }

            // get factories for filter dropdown
            try
            {
                var factoryReq = new RestRequest(commonAPIConstant.Endpoint.api_Storage_Factory);
                factoryReq.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                var factoryResp = await _restClient.GetAsync(factoryReq);
                if (factoryResp?.IsSuccessful == true && !string.IsNullOrEmpty(factoryResp.Content))
                {
                    var factories = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<FactoryInfoModel>>>(factoryResp.Content)?.Data;
                    ViewBag.Factories = factories;
                }
                else
                {
                    ViewBag.Factories = null;
                }
            }
            catch
            {
                ViewBag.Factories = null;
            }

            // support filtering by factory via query string ?factoryId={id}
            var factoryIdQuery = Request.Query["factoryId"].ToString();
            ViewBag.SelectedFactory = factoryIdQuery;
            var layouts = result.Data ?? new List<LayoutDto>();
            if (!string.IsNullOrEmpty(factoryIdQuery) && Guid.TryParse(factoryIdQuery, out var parsedFactoryId))
            {
                var factoriesObj = ViewBag.Factories as IEnumerable<BIVN.FixedStorage.Services.Common.API.Dto.Factory.FactoryInfoModel>;
                var selectedFactoryName = factoriesObj?.FirstOrDefault(f => f.Id == parsedFactoryId)?.Name;
                if (!string.IsNullOrEmpty(selectedFactoryName))
                {
                    layouts = layouts.Where(l => string.Equals(l.FactoryName, selectedFactoryName, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            return View("ListLayoutIndex", layouts);
        }

        [RouteDisplayValue(RouteDisplay.HISTORY)]
        [HttpGet("in-out-history")]
        [Authorize(commonAPIConstant.Permissions.HISTORY_MANAGEMENT)]
        public async Task<IActionResult> InOutHistory()
        {
            var token = Request.TokenFromCookie();

            try
            {
                //Phong Ban:
                var reqDepartment = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Department);
                reqDepartment.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"Bearer {token}");

                //Call API DS Phong Ban:
                var resultListDepartment = await _restClient.GetAsync(reqDepartment);
                var convertedResultListDepartment = JsonSerializer.Deserialize<ResponseModel<IEnumerable<DepartmentDto>>>(resultListDepartment?.Content, JsonDefaults.CamelCasing)?.Data;
                ViewBag.Departments = convertedResultListDepartment;
            }
            catch(Exception ex)
            {
                _logger.LogError("Lỗi khi get API danh sách phòng ban");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            try
            {
                var reqFactory = new RestRequest(commonAPIConstant.Endpoint.api_Storage_Factory);
                reqFactory.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"Bearer {token}");

                var result = await _restClient.GetAsync(reqFactory);
                var convertedResult = JsonSerializer.Deserialize<ResponseModel<IEnumerable<FactoryInfoModel>>>(result?.Content, JsonDefaults.CamelCasing)?.Data;
                ViewBag.Factories = convertedResult;
            }
            catch(Exception ex)
            {
                _logger.LogError("Lỗi khi get API danh sách nhà máy");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            try
            {
                var layoutDropdownListRequest = new RestRequest(commonAPIConstant.Endpoint.API_Storage_Get_Layout_DropDownList);
                layoutDropdownListRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"Bearer {token}");
                var layoutDropdownListResponse = await _restClient.GetAsync(layoutDropdownListRequest);
                ViewBag.Layouts = JsonSerializer.Deserialize<ResponseModel<List<DropDownListItemDto>>>(layoutDropdownListResponse?.Content, JsonDefaults.CamelCasing)?.Data;
            }
            catch(Exception ex)
            {
                _logger.LogError("Lỗi khi get API danh sách khu vực");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            return View();
        }
    }
}
