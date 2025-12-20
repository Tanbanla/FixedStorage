namespace BIVN.FixedStorage.WebApp.Components
{
    [ViewComponent]
    public class CreateDepartmentViewComponent : ViewComponent
    {
        private readonly IRestClient _restClient;
        private readonly ILogger<CreateDepartmentViewComponent> _logger;

        public CreateDepartmentViewComponent(IRestClient restClient,
                                            ILogger<CreateDepartmentViewComponent> logger
                                        )
        {
            _restClient = restClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string token = Request.TokenFromCookie();
            var restReq = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Department_Users);
            restReq.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
            try
            {
                var result = await _restClient.ExecuteGetAsync(restReq);
                ViewBag.SelectUsers = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<SelectUserDepartmentViewModel>>>(result?.Content)?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi kết nối tới Identity service");
                _logger.LogHttpContext(HttpContext, ex.Message);
            }

            return View();
        }
    }
}
