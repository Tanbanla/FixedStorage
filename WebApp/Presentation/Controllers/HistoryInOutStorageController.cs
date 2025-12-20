namespace WebApp.Presentation.Controllers
{
    [Authorize]
    public class HistoryInOutStorageController : Controller
    {
        private readonly ILogger<ComponentsController> _logger;
        private readonly IRestClient _restClient;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHistoryInOutStorageService _historyInOutStorageService;

        public HistoryInOutStorageController(
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<ComponentsController> logger,
                              IRestClient restClient,
                              IConfiguration configuration,
                              IHistoryInOutStorageService historyInOutStorageService
                                )
        {
            _logger = logger;
            _restClient = restClient;
            _configuration = configuration;
            //_httpContext = httpContextAccessor.HttpContext;
            _httpContextAccessor = httpContextAccessor;
            _historyInOutStorageService = historyInOutStorageService;
        }

        [HttpPost("export/history")]
        [Authorize(commonAPIConstant.Permissions.MASTER_DATA_READ)]
        public async Task<IActionResult> ExportHistoryAsync([FromForm] HistoryInOutExportDto model)
        {
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Storage_Get_History_InOut_Export);
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<HistoryInOutExportResultDto>>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }

                var exportResult = await _historyInOutStorageService.ExportExcelHistoryInOutStorageAsync(result.Data, "TemplateExportHistoryInOutStorage.xlsx");

                

                return File((byte[])exportResult.Data, commonAPIConstant.FileResponse.ExcelType, $"LichSuXuatNhapKho_{DateTime.Now.Date.Day}_{DateTime.Now.Date.Month}_{DateTime.Now.Date.Year}.xlsx");


            }
            catch (Exception exception)
            {
                _logger.LogHttpContext(HttpContext, exception.Message);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
