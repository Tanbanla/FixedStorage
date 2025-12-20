using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using BIVN.FixedStorage.Services.Common.API.Dto.QRCode;

namespace WebApp.Presentation.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly IRestClient _restClient;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IInventoryService _inventoryService;

        public InventoryController(
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<InventoryController> logger,
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


        [RouteDisplayValue(RouteDisplay.INVENTORY)]
        [HttpGet("inventory")]
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

                ViewBag.DepartmentInventory = getDepartments?.Data;
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
                var getLocationsDistinct = getLocations?.Data;
                ViewBag.LocationInventory = getLocationsDistinct;
            }
            catch(Exception ex)
            {

                _logger.LogError("Lỗi khi get API danh sách khu vực");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            return View();
        }

        [HttpPost("export/inventory")]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportListInventory(ListInventoryToExportDto model)
        {
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.api_Inventory_Web_Export);
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<ListInventoryModel>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var exportResult = await _inventoryService.ExportListInventory(result.Data);

                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"DanhSachDotKiemKe_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("inventory/{inventoryId}/audit-target/export")]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportListAuditTarget(ListAuditTargetDto model, string inventoryId)
        {
            try
            {
                var request = new RestRequest($"api/inventory/{inventoryId}/audit-target/export");
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<ListAuditTargetViewModel>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var exportResult = await _inventoryService.ExportListAuditTarget(result.Data);

                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"DanhSachGiamSat_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("inventory/{inventoryId}/document/export")]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportListInventoryDocument(ListInventoryDocumentDto model, string inventoryId)
        {
            try
            {
                var request = new RestRequest($"api/inventory/web/{inventoryId}/document/export");
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<ListInventoryDocumentModel>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }

                var exportResult = await _inventoryService.ExportListInventoryDocument(result.Data);

                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"PhieuKiemKe_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");

            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("inventory/summary/export-txt")]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportTxtSummaryInventoryDocument(DocumentResultListFilterModel model)
        {
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.api_Iventory_Web_DocResult_Export);
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<DocumentResultViewModel>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }

                var exportResult = await _inventoryService.ExportTxtSummaryInventoryDocument(result.Data);

                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.Txt, $"Tonghopketqua_{DateTime.Now.ToString(commonAPIConstant.DatetimeFormat)}.xlsx");

            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("inventory/history/export")]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportListInventoryDocumentHistory(ListDocumentHistoryDto model)
        {
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.api_Iventory_Web_History_Export);
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<ListDocumentHistoryModel>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }

                var exportResult = await _inventoryService.ExportListInventoryDocumentHistory(result.Data);

                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"DanhSachLichKiemKe_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("inventory/generate/qrcode")]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> GenerateQRCode(ListDocTypeCToExportQRCodeDto model)
        {
            try
            {
                var request = new RestRequest($"api/inventory/web/doc-type-c/export/qrcode");
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<ListDocTypeCToExportQRCodeModel>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                if (result.Code == StatusCodes.Status404NotFound)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status404NotFound, Message = "Không tìm thấy dữ liệu." });
                }
                var exportResult = await _inventoryService.ExportQRCode(result.Data);
                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"DanhSachPhieuCXuatQRCode_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("inventory/error/export")]
        //[Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcelInventoryError(ListDocToExportInventoryErrorDto model)
        {
            try
            {
                var request = new RestRequest($"api/inventory/web/export/inventory/error");
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<ListDocToExportInventoryErrorModel>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }

                var exportResult = await _inventoryService.ExportInventoryError(result.Data);
                if(exportResult.Code == StatusCodes.Status200OK)
                {
                    return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"DanhSachXuatSaiSo_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");
                }

                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }


        [RouteDisplayValue(RouteDisplay.INVENTORY, RouteDisplay.DETAIL, RouteDisplay.INVENTORY_GENERAL_INFO)]
        [HttpGet("inventory/{id}")]
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: InventoryController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: InventoryController/Create
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

        // GET: InventoryController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: InventoryController/Edit/5
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

        // GET: InventoryController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: InventoryController/Delete/5
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
