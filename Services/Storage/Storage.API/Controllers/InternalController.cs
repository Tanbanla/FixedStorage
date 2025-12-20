namespace Storage.API.Controllers
{
    [Route("api/internal")]
    [ApiController]
    public class InternalController : ControllerBase
    {
        private readonly ILogger<InternalController> _logger;
        private readonly IInternalService _internalService;

        public InternalController(ILogger<InternalController> logger,
                                   IInternalService internalService
                                )
        {
            _logger = logger;
            _internalService = internalService;
        }

        [HttpGet("factories")]
        [InternalService]
        public async Task<IActionResult> Factories()
        {
            var result = await _internalService.GetFactories();
            if(result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpGet("layouts")]
        [InternalService]
        public async Task<IActionResult> Layouts()
        {
            var result = await _internalService.GetLayouts();
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }
            return NotFound(result);
        }
    }
}
