using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
namespace WebApp.Presentation.Controllers
{
    public class InventoryHistoryController : Controller
    {
        private readonly ILogger<InventoryHistoryController> _logger;
        private readonly IRestClient _restClient;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IInventoryService _inventoryService;

        public InventoryHistoryController(
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<InventoryHistoryController> logger,
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

        // GET: InventoryHistory
        [RouteDisplayValue(RouteDisplay.INVENTORY_HISTORY)]
        [HttpGet("inventory-history")]
        public async Task<IActionResult> Index()
        {
            var token = Request.TokenFromCookie();
            try
            {
                //Phong Ban:
                var reqDepartment = new RestRequest(commonAPIConstant.Endpoint.api_Inventory_Location_Departments);
                reqDepartment.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                //Call API DS Phong Ban:
                var resultListDepartment = await _restClient.GetAsync(reqDepartment);
                var getDepartments = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<DepartmentInventoryDto>>>(resultListDepartment?.Content ?? string.Empty);

                ViewBag.InventoryDepartments = getDepartments?.Data;
            }
            catch(Exception ex)
            {

                _logger.LogError("Lỗi khi get API danh sách phòng ban");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            try
            {
                LocationByDepartmentDto locationByDepartment = new();
                //Khu vuc:
                var reqLocations = new RestRequest(commonAPIConstant.Endpoint.api_Inventory_Location_DepartmentName);
                reqLocations.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
                reqLocations.AddBody(locationByDepartment);
                //Call API DS Phong Ban:
                var resultLocations = await _restClient.ExecutePostAsync(reqLocations);
                var getLocations = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<DepartmentInventoryDto>>>(resultLocations?.Content ?? string.Empty);
                var getLocationsDistinct = getLocations.Data;
                ViewBag.InventoryLocations = getLocationsDistinct;
            }
            catch(Exception ex)
            {
                _logger.LogError("Lỗi khi get API danh sách khu vực");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

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
                _logger.LogHttpContext(HttpContext, ex.Message);
            }
            return View();
        }

        // GET: InventoryHistory/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: InventoryHistory/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: InventoryHistory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: InventoryHistory/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: InventoryHistory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: InventoryHistory/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: InventoryHistory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
