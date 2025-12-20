using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using Microsoft.AspNetCore.Mvc;
using WebApp.Application.Services.ErrorInvestigationWeb;

namespace WebApp.Presentation.Controllers
{
    public class ErrorInvestigationController : Controller
    {
        private readonly ILogger<ErrorInvestigationController> _logger;
        private readonly IRestClient _restClient;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IInventoryService _inventoryService;
        private readonly IErrorInvestigationWebService _errorInvestigationWebService;

        public ErrorInvestigationController(
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<ErrorInvestigationController> logger,
                              IRestClient restClient,
                              IConfiguration configuration,
                              IInventoryService inventoryService,
                              IErrorInvestigationWebService errorInvestigationWebService
                                )
        {
            _logger = logger;
            _restClient = restClient;
            _configuration = configuration;
            //_httpContext = httpContextAccessor.HttpContext;
            _httpContextAccessor = httpContextAccessor;
            _inventoryService = inventoryService;
            _errorInvestigationWebService = errorInvestigationWebService;
        }

        // GET: ErrorInvestigationController
        [RouteDisplayValue(RouteDisplay.ERROR_INVESTIGATION)]
        [HttpGet("error-investigation")]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> Index()
        {
            var currUser = HttpContext.UserFromContext();
            var canAccess = HttpContext.IsInventory() || HttpContext.IsPromoter();
            if (!canAccess)
            {
                return RedirectPreventAccessResult();
            }
            var token = Request.TokenFromCookie();
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.api_Inventory_Web_Dropdown_InventoryName);
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
                var response = await _restClient.GetAsync(request);
                var convertedResponse = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListInventoryModel>>>(response?.Content ?? string.Empty);
                ViewBag.InventoryNames = convertedResponse?.Data ?? new List<ListInventoryModel>();

                var errorCategoryRequest = new RestRequest("api/error-investigation/web/management/error-category");
                errorCategoryRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
                var errorCategoryResponse = await _restClient.GetAsync(errorCategoryRequest);
                var convertErrorCategoryResponse = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ErrorCategoryManagementDto>>>(errorCategoryResponse?.Content ?? string.Empty);
                ViewBag.ErrorCategories = convertErrorCategoryResponse?.Data ?? new List<ErrorCategoryManagementDto>();

            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi get API api/inventory/web/dropdown/inventory-name");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            return View();
        }

        [HttpPost("error-investigation/inventory/export")]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportListErrorInvestigation(ListErrorInvestigationWebModel model)
        {
            try
            {
                var request = new RestRequest($"api/error-investigation/web/inventory/export-file");
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }


                var result = JsonConvert.DeserializeObject<ResponseModel<List<ListErrorInvestigationWebDto>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }

                var errorCategoryRequest = new RestRequest("api/error-investigation/web/management/error-category");
                errorCategoryRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
                var errorCategoryResponse = await _restClient.GetAsync(errorCategoryRequest);
                var convertErrorCategoryResponse = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ErrorCategoryManagementDto>>>(errorCategoryResponse?.Content ?? string.Empty);


                var exportResult = await _errorInvestigationWebService.ExportListErrorInvestigation(result.Data, convertErrorCategoryResponse.Data);

                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"Danhsachsaiso_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");

            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }


    }
}
