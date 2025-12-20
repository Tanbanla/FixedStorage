namespace BIVN.FixedStorage.Services.Storage.API.Controllers
{
    [ApiController]
    [Route(commonAPIConstant.Endpoint.api_Storage_Factory)]
    public class FactoryController : ControllerBase
    {
        private readonly ILogger<FactoryController> _logger;
        private readonly IFactoryService _factoryService;

        public FactoryController(ILogger<FactoryController> logger,
                                IFactoryService factoryService
                                )
        {
            _logger = logger;
            _factoryService = factoryService;
        }

        [HttpGet]
        //Use AllowAnonymous for debugging
        [Authorize(Constants.Permissions.FACTORY_DATA_INQUIRY, Constants.Permissions.ROLE_MANAGEMENT, 
                Constants.Permissions.INPUT, Constants.Permissions.HISTORY_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<FactoryInfoModel>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> FactoryList()
        {
            var result = await _factoryService.FactoryListAsync();
            if(result?.Data?.Any() == false)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
