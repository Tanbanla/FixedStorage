using BIVN.FixedStorage.Services.Common.API.Dto.Component;

namespace WebApp.Presentation.Controllers
{
    [Authorize]
    public class ComponentsController : Controller
    {
        private readonly ILogger<ComponentsController> _logger;
        private readonly IRestClient _restClient;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IComponentService _componentService;

        public ComponentsController(
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<ComponentsController> logger,
                              IRestClient restClient,
                              IConfiguration configuration,
                              IComponentService componentService
                                )
        {
            _logger = logger;
            _restClient = restClient;
            _configuration = configuration;
            //_httpContext = httpContextAccessor.HttpContext;
            _httpContextAccessor = httpContextAccessor;
            _componentService = componentService;
        }

        [RouteDisplayValue(RouteDisplay.COMPONENT)]
        [HttpGet(commonAPIConstant.Endpoint.WebApp_Get_Component_List)]
        [Authorize(commonAPIConstant.Permissions.MASTER_DATA_READ, commonAPIConstant.Permissions.MASTER_DATA_WRITE)]
        public async Task<IActionResult> Index()
        {
            var token = Request.TokenFromCookie();

            // layout dropdown-list
            var layoutDropdownListRequest = new RestRequest(commonAPIConstant.Endpoint.API_Storage_Get_Layout_DropDownList);
            layoutDropdownListRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
            var layoutDropdownListResponse = await _restClient.GetAsync(layoutDropdownListRequest);
            ViewBag.LayoutList = JsonConvert.DeserializeObject<ResponseModel<List<DropDownListItemDto>>>(layoutDropdownListResponse?.Content)?.Data;
            ViewBag.SelectedLayout = Request.Query["layout"].ToString();

            // inventory status dropdown-list
            var inventoryStatusDropdownListRequest = new RestRequest(commonAPIConstant.Endpoint.API_Storage_Get_InventoryStatus_DropDownList);
            inventoryStatusDropdownListRequest.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
            var inventoryStatusDropdownListResponse = await _restClient.GetAsync(inventoryStatusDropdownListRequest);
            ViewBag.SelectedInventoryStatus = Request.Query["inventoryStatus"].ToString();
            ViewBag.InventoryStatusList = JsonConvert.DeserializeObject<ResponseModel<List<DropDownListItemDto>>>(inventoryStatusDropdownListResponse?.Content)?.Data;
            return View();
        }

        [HttpPost("export/component/list")]
        [Authorize(commonAPIConstant.Permissions.MASTER_DATA_READ, commonAPIConstant.Permissions.MASTER_DATA_WRITE)]
        public async Task<IActionResult> ExportComponentListAsync(ComponentsFilterToExportDto model)
        {
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Storage_Get_Component_List_Export);
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<ComponentFilterItemResultDto>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }

                var exportResult = await _componentService.ExportFilteredComponentListAsync(result.Data, "TemplateExportComponentList.xlsx");

                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"DanhSachLinhKien_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.WebApp_Import_Component_List)]
        [Authorize(commonAPIConstant.Permissions.MASTER_DATA_WRITE, commonAPIConstant.Permissions.MASTER_DATA_WRITE)]
        public async Task<IActionResult> ImportComponentListAsync([FromForm] IFormFile file)
        {
            try
            {
                var result = new ResponseModel<ComponentImportInfoResultDto>(StatusCodes.Status200OK, new ComponentImportInfoResultDto());
                if (file == null)
                {
                    result.Code = StatusCodes.Status500InternalServerError;
                    result.Message = "File import không tồn tại";
                    return BadRequest(result);
                }
                else if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".csv"))
                {
                    result.Code = StatusCodes.Status400BadRequest;
                    result.Message = commonAPIConstant.ResponseMessages.InvalidFileFormat;
                    return BadRequest(result);
                }

                var request = new RestRequest(commonAPIConstant.Endpoint.API_Storage_Import_Component_List);
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    request.AddFile("file", fileBytes, file.FileName);
                }
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false || string.IsNullOrEmpty(response.Content))
                {
                    result.Code = StatusCodes.Status500InternalServerError;
                    result.Message = "Xảy ra lỗi trong quá trình Import linh kiện";
                    return BadRequest(result);
                }

                var resultImport = JsonConvert.DeserializeObject<ResponseModel<ComponentItemImportResultDto>>(response?.Content);

                if (resultImport.Code == (int)HttpStatusCodes.InvalidFileExcel)
                {
                    result.Code = (int)HttpStatusCodes.InvalidFileExcel;
                    result.Message = commonAPIConstant.ResponseMessages.InvalidFileFormat;
                    return BadRequest(result);
                }

                if (resultImport == null)
                {
                    result.Code = StatusCodes.Status500InternalServerError;
                    result.Message = "Xảy ra lỗi trong quá trình Import linh kiện.";
                    return BadRequest(result);
                }
                result.Data.SuccessCount = resultImport.Data.SuccessCount;
                result.Data.FailCount = resultImport.Data.FailCount;

                // Excel file import error result exists
                if (resultImport.Data.FailedImportComponents?.Any() == true)
                {
                    result.Data.FileName = $"DanhSachLinhKienImport_{DateTime.Now.ToString(commonAPIConstant.DatetimeFormat)}.xlsx";
                    var importSuccess = await _componentService.ImportComponentListFromExcel(resultImport.Data.FailedImportComponents, result.Data.FileName);
                    if (importSuccess)
                    {
                        result.Data.FileUrl = $"{Request.Scheme}://{Request.Host}/assets/{result.Data.FileName}";
                        return Ok(result);
                    }
                    else
                    {
                        result.Message = "Không tồn tại file mẫu Template nhập Import linh kiện.";
                        result.Code = StatusCodes.Status500InternalServerError;
                        return BadRequest(result);
                    }
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
