namespace Identity.API.Controllers
{
    [Route(Constants.Endpoint.Internal.absolute)]
    [ApiController]
    public class InternalController : ControllerBase
    {
        private readonly IInternalService _internalService;

        public InternalController(IInternalService internalService
                                    )
        {
            _internalService = internalService;
        }

        [InternalService]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var result = _internalService.GetUsers();
            if(result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        [InternalService]
        [HttpGet("users/roles/{userId}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            if(string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.TryAddModelError(nameof(userId), "Id không hợp lệ");
                return BadRequest(new ResponseModel
                {
                    Message = "Dữ liệu không hợp lệ",
                    Data = ModelState.ErrorMessages()
                });
            }

            var result = _internalService.GetUserRoles(userId);
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return NotFound(result);
        }


        [InternalService]
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var result = _internalService.GetDepartments();
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        [InternalService]
        [HttpGet("list/user")]
        public async Task<IActionResult> ListUser()
        {
            var result = _internalService.ListUser();
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        [InternalService]
        [HttpGet("role/by-department")]
        //Use AllowAnonymous for debugging
        //[@Authorize(Constants.Permissions.ROLE_MANAGEMENT, Constants.Permissions.USER_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>))]
        public async Task<IActionResult> GetAllRoleWithUserNames()
        {
            var result = _internalService.GetAllRoleWithUserNames();
            if (result?.Data == null)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

    }
}
