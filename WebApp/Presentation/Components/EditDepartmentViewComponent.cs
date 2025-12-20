namespace BIVN.FixedStorage.WebApp.Components
{
    [ViewComponent]
    public class EditDepartmentViewComponent : ViewComponent
    {
        private readonly IRestClient _restClient;
        private readonly ILogger<CreateDepartmentViewComponent> _logger;

        public EditDepartmentViewComponent(IRestClient restClient,
                                            ILogger<CreateDepartmentViewComponent> logger
                                        )
        {
            _restClient = restClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var token = Request.Cookies[commonAPIConstant.HttpContextModel.TokenKey];
            var restReq = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Department_Users);
            restReq.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);
            try
            {
                var result = await _restClient.GetAsync(restReq);
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
