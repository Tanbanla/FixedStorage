namespace Inventory.API.Controllers
{
    [Route(commonAPIConstant.Endpoint.InventoryService.Internal.root)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
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

        [InternalService]
        [HttpGet(commonAPIConstant.Endpoint.InventoryService.Internal.inventoryAccount)]
        public async Task<IActionResult> InventoryAccount(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError(nameof(userId), Constants.ResponseMessages.InvalidId);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModelDetail
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                    Data = ModelState.ErrorMessages()
                });
            }

            var convertedUserId = Guid.Parse(userId);
            var result = await _internalService.GetInventoryLoggedInfo(convertedUserId);

            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [InternalService]
        [HttpDelete(commonAPIConstant.Endpoint.InventoryService.Internal.deleteInventoryAccount)]
        public async Task<IActionResult> DeleteInventoryAccount(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                {
                    ModelState.AddModelError(nameof(userId), Constants.ResponseMessages.InvalidId);
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModelDetail
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = Constants.ResponseMessages.InValidValidationMessage,
                        Data = ModelState.ErrorMessages()
                    });
                }

                var result = await _internalService.DeleteInventoryAccount(Guid.Parse(userId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [InternalService]
        [HttpGet(commonAPIConstant.Endpoint.InventoryService.Internal.checkAuditAccountAssignLocation)]
        public async Task<IActionResult> CheckAuditAccountAssignLocation(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                {
                    ModelState.AddModelError(nameof(userId), Constants.ResponseMessages.InvalidId);
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModel<bool>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = Constants.ResponseMessages.InValidValidationMessage,
                        Data = false
                    });
                }

                var convertedUserId = Guid.Parse(userId);
                var result = await _internalService.CheckAuditAccountAssignLocation(convertedUserId);

                return result.Code == StatusCodes.Status200OK ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ResponseModel<bool>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer,
                    Data = false
                });
            }

        }

        [InternalService]
        [HttpPut(commonAPIConstant.Endpoint.InventoryService.Internal.updateInventoryAccount)]
        public async Task<IActionResult> UpdateInventoryAccount(string userId, string newUserName)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                {
                    ModelState.AddModelError(nameof(userId), "Id không hợp lệ.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModelDetail
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Dữ liệu không hợp lệ.",
                        Data = ModelState.ErrorMessages()
                    });
                }

                var result = await _internalService.UpdateInventoryAccount(Guid.Parse(userId), newUserName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

    }
}
