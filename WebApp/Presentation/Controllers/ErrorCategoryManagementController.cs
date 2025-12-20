using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Presentation.Controllers
{
    public class ErrorCategoryManagementController : Controller
    {
        private readonly ILogger<ErrorCategoryManagementController> _logger;
        private readonly IRestClient _restClient;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IInventoryService _inventoryService;

        public ErrorCategoryManagementController(
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<ErrorCategoryManagementController> logger,
                              IRestClient restClient,
                              IConfiguration configuration,
                              IInventoryService inventoryService
                                )
        {
            _logger = logger;
            _restClient = restClient;
            _configuration = configuration;
            //_httpContext = httpContextAccessor.HttpContext;
            _httpContextAccessor = httpContextAccessor;
            _inventoryService = inventoryService;
        }

        // GET: ErrorInvestigationController
        [RouteDisplayValue(RouteDisplay.ERROR_CATEGORY_MANAGEMENT)]
        [HttpGet("error-category-management")]
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
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi get API api/inventory/web/dropdown/inventory-name");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            return View();
        }
    }
}
