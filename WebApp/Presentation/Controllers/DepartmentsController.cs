using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace WebApp.Presentation.Controllers
{
    [Authorize]
    public class DepartmentsController : Controller
    {
        private const string _CONTROLLERNAME = "Departments";
        private readonly IRestClient _restClient;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(IRestClient restClient,
                                     ILogger<DepartmentsController> logger
                                    )
        {
            _restClient = restClient;
            _logger = logger;
        }
        [RouteDisplayValue(RouteDisplay.DEPARTMENT)]
        [Route("departments")]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        [Authorize(commonAPIConstant.Permissions.DEPARTMENT_MANAGEMENT)]
        public async Task<IActionResult> Index()
        {
            var token = Request.TokenFromCookie();
            var req = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Department);
            req.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
            try
            {

                var result = await _restClient.GetAsync(req);
                _logger.LogInformation($"request url: {_restClient.BuildUri(req).AbsoluteUri}");
                _logger.LogInformation($"response content:{result?.Content}");
                var Departments = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<DepartmentDto>>>(result?.Content)?.Data;
                ViewBag.Departments = Departments;

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
                _logger.LogError("Lỗi khi get API danh sách phòng ban.");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            return View();
        }

        [HttpDelete("delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            //var existDepartment = /*API check exist department*/

            return Ok(new ResponseModel { Code = StatusCodes.Status200OK });
        }

        [HttpGet("edit/prepare")]
        public async Task<IActionResult> PrepareEdit()
        {
            return ViewComponent("EditDepartment");
        }

        [HttpPost("edit/confirm")]
        public async Task<IActionResult> ConfirmEdit(CreateNewDepartmentDto createNewDepartmentDto)
        {
            return ViewComponent("EditDepartment", createNewDepartmentDto);
        }
    }
}
