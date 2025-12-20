using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace WebApp.Presentation.Controllers
{
    public class InventoryReportingController : Controller
    {
        private readonly ILogger<InventoryReportingController> _logger;
        private readonly IRestClient _restClient;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IInventoryService _inventoryService;

        public InventoryReportingController(
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<InventoryReportingController> logger,
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


        // GET: InventoryReporting
        [RouteDisplayValue(RouteDisplay.INVENTORY_REPORTING)]
        [HttpGet("inventory-reporting")]
        public async Task<IActionResult> Index()
        {
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
            if (!CheckReportRole())
            {
                return new RedirectToActionResult("PreventAccessPage", "Home", null);
            }

            return View();
        }

        //Kiểm tra quyền xem màn báo cáo
        private bool CheckReportRole()
        {
            //Kiểm tra quyền
            var inventoryInfoModel = HttpContext.InventoryInfo();
            var canAccess = User.IsGrant(commonAPIConstant.Permissions.VIEW_REPORT) ||
                                HttpContext.IsPromoter();
            return canAccess;
        }
    }
}
