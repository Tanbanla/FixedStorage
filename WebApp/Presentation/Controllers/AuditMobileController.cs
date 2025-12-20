using System.Reflection.Metadata;
using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;

namespace WebApp.Presentation.Controllers
{
    public class AuditMobileController : Controller
    {
        private readonly IRestClient _restClient;
        private readonly ILogger<AuditMobileController> _logger;
        public AuditMobileController(
                                        IRestClient restClient,
                                        ILogger<AuditMobileController> logger

                                    )
        {
            _restClient = restClient;
            _logger = logger;
        }

        [RouteDisplayValue(RouteDisplay.AUDIT_MOBILE)]
        public IActionResult Index()
        {
            var currUser = HttpContext.UserFromContext();

            return View();
        }

        public IActionResult AuditTargetList()
        {
            return View();
        }

        [HttpGet("/AuditMobile/Documentdetail/{documentId}")]
        public async Task<IActionResult> DocumentDetail(string documentId, string actionType = "2")
        {
            try
            {
                var inventoryId = HttpContext.UserFromContext()?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
                var accountId = HttpContext.UserFromContext()?.InventoryLoggedInfo?.AccountId;
                var request = new RestRequest($"api/inventory/{inventoryId}/account/{accountId}/document/{documentId}/action/{actionType}");
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                var response = await _restClient.GetAsync(request);

                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<DocumentDetailModel>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                

                ViewBag.DetailDocument = result?.Data;
            }
            catch (Exception ex)
            {

                _logger.LogError("Lỗi khi lấy chi tiết phiếu giám sát trên mobile website.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet("/AuditMobile/HistoryDetail/{historyId}")]
        public async Task<IActionResult> HistoryDetail(string historyId)
        {
            try
            {
                var inventoryId = HttpContext.UserFromContext()?.InventoryLoggedInfo?.InventoryModel?.InventoryId;
                var accountId = HttpContext.UserFromContext()?.InventoryLoggedInfo?.AccountId;
                var request = new RestRequest($"api/inventory/{inventoryId}/account/{accountId}/history/{historyId}");
                var token = Request.TokenFromCookie();
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                var response = await _restClient.GetAsync(request);

                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<HistoryDetailViewModel>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }


                ViewBag.HistoryDetail = result?.Data;
            }
            catch (Exception ex)
            {

                _logger.LogError("Lỗi khi lấy chi tiết lịch sử phiếu giám sát trên mobile website.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
