using System.Text.Json;
using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Inventory.API.Service.Dto.ErrorInvestigation;
using Inventory.API.Service.Dto.ErrorInvestigation;
using Inventory.API.Service.ErrorInvestigation;

namespace Inventory.API.Controllers
{
    [Route(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationRoute.root)]
    [ApiController]
    public class ErrorInvestigationController : ControllerBase
    {
        private readonly ILogger<ErrorInvestigationController> _logger;
        private readonly HttpContext _httpContext;
        private readonly IErrorInvestigationService _errorInvestigationService;

        public ErrorInvestigationController(ILogger<ErrorInvestigationController> logger, IHttpContextAccessor httpContextAccessor, IErrorInvestigationService errorInvestigationService)
        {
            _logger = logger;
            _httpContext = httpContextAccessor.HttpContext;
            _errorInvestigationService = errorInvestigationService;
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationRoute.ListErrorInvestigation)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<ErrorInvestigationListDto>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.CannotErrorInvestigation, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Error Investigation")]
        public async Task<IActionResult> ListErrorInvestigation(Guid inventoryId, [FromQuery] ErrorInvestigationStatusType? status, [FromQuery] string componentCode, [FromQuery] int pageSize = 20, [FromQuery] int pageNum = 1)
        {
            //25122024: Nếu tài khoản có vai trò Điều tra sai số thì được phép xem hoặc chỉnh sửa điều tra:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            var roleCurrent = checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType;
            if (!(roleCurrent == (int)InventoryAccountRoleType.Promotion || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInCharge
                    || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInManagerment))
            {
                if (!(checkAccountTypeAndRole.AccountType == nameof(AccountType.TaiKhoanRieng) && checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.RoleName))
                {
                    var validRoleResult = await _errorInvestigationService.CheckValidInventoryRole(checkAccountTypeAndRole.InventoryLoggedInfo.AccountId, (int)InventoryAccountRoleType.Inventory);
                    if (validRoleResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validRoleResult);
                    }
                }
                //var validInventoryDateResult = await _errorInvestigationService.CheckValidInventoryDate(inventoryId);
                //if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                //{
                //    return BadRequest(validInventoryDateResult);
                //}
            }

            var result = await _errorInvestigationService.ErrorInvestigationList(inventoryId, status, componentCode, pageSize, pageNum);
            if (result.Code == StatusCodes.Status200OK)
            {
                var jsonSetting = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new NullableDoubleConverter() },
                };

                return new JsonResult(result, jsonSetting);
            }

            return BadRequest(result);
        }

        [HttpPut(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationRoute.UpdateErrorInvestigationStatus)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        [ApiExplorerSettings(GroupName = "Error Investigation")]
        public async Task<IActionResult> UpdateStatusErrorInvestigation(Guid inventoryId, string componentCode)
        {
            //25122024: Nếu tài khoản có vai trò Điều tra sai số thì được phép xem hoặc chỉnh sửa điều tra:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            var roleCurrent = checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType;
            if (!(roleCurrent == (int)InventoryAccountRoleType.Promotion || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInCharge
                    || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInManagerment))
            {
                if (!(checkAccountTypeAndRole.AccountType == nameof(AccountType.TaiKhoanRieng) && checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.RoleName))
                {
                    var validRoleResult = await _errorInvestigationService.CheckValidInventoryRole(checkAccountTypeAndRole.InventoryLoggedInfo.AccountId, (int)InventoryAccountRoleType.Inventory);
                    if (validRoleResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validRoleResult);
                    }
                }
                //var validInventoryDateResult = await _errorInvestigationService.CheckValidInventoryDate(inventoryId);
                //if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                //{
                //    return BadRequest(validInventoryDateResult);
                //}
            }

            var result = await _errorInvestigationService.UpdateStatusErrorInvestigation(inventoryId, componentCode);
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationRoute.ConfirmErrorInvestigation)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        [ApiExplorerSettings(GroupName = "Error Investigation")]
        public async Task<IActionResult> ConfirmErrorInvestigation(Guid inventoryId, string componentCode, ErrorInvestigationConfirmType type, [FromForm] ErrorInvestigationConfirmModel model)
        {
            //25122024: Nếu tài khoản có vai trò Điều tra sai số thì được phép xem hoặc chỉnh sửa điều tra:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            var roleCurrent = checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType;
            if (!(roleCurrent == (int)InventoryAccountRoleType.Promotion || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInCharge 
                    || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInManagerment))
            {
                if (!(checkAccountTypeAndRole.AccountType == nameof(AccountType.TaiKhoanRieng) && (checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.RoleName ||
                      checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.AdministratorRoleName || checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.MCRoleName ||
                      checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.InventoryRoleName || checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.InvGRoleName)))
                {
                    var validRoleResult = await _errorInvestigationService.CheckValidInventoryRole(checkAccountTypeAndRole.InventoryLoggedInfo.AccountId, (int)InventoryAccountRoleType.Inventory);
                    if (validRoleResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validRoleResult);
                    }
                }
                //var validInventoryDateResult = await _errorInvestigationService.CheckValidInventoryDate(inventoryId);
                //if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                //{
                //    return BadRequest(validInventoryDateResult);
                //}
            }

            //Quantity, ErrorCategory bắt buộc phải có dữ liệu:
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages(),
                });
            }

            var result = await _errorInvestigationService.ConfirmErrorInvestigation(inventoryId, componentCode, type, model);
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationRoute.ErrorInvestigationDocumentList)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<ErrorInvestigationDocumentListDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.CannotErrorInvestigation, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Error Investigation")]
        public async Task<IActionResult> ErrorInvestigationDocumentList([FromQuery] string? userCode, Guid inventoryId, string componentCode)
        {
            //25122024: Nếu tài khoản có vai trò Điều tra sai số thì được phép xem hoặc chỉnh sửa điều tra:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            var roleCurrent = checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType;
            if (!(roleCurrent == (int)InventoryAccountRoleType.Promotion || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInCharge
                    || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInManagerment))
            {
                if (!(checkAccountTypeAndRole.AccountType == nameof(AccountType.TaiKhoanRieng) && checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.RoleName))
                {
                    var validRoleResult = await _errorInvestigationService.CheckValidInventoryRole(checkAccountTypeAndRole.InventoryLoggedInfo.AccountId, (int)InventoryAccountRoleType.Inventory);
                    if (validRoleResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validRoleResult);
                    }
                }
                //var validInventoryDateResult = await _errorInvestigationService.CheckValidInventoryDate(inventoryId);
                //if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                //{
                //    return BadRequest(validInventoryDateResult);
                //}
            }

            var result = await _errorInvestigationService.ErrorInvestigationDocumentList(userCode, inventoryId, componentCode);
            if (result.Code == StatusCodes.Status200OK)
            {
                var jsonSetting = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new NullableDoubleConverter() },
                };

                return new  JsonResult (result,jsonSetting);
            }

            return BadRequest(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationRoute.ErrorInvestigationConfirmedViewDetail)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<ErrorInvestigationConfirmedViewDetailDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.CannotErrorInvestigation, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Error Investigation")]
        public async Task<IActionResult> ErrorInvestigationConfirmedViewDetail(Guid inventoryId, string componentCode)
        {
            //25122024: Nếu tài khoản có vai trò Điều tra sai số thì được phép xem hoặc chỉnh sửa điều tra:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            var roleCurrent = checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType;
            if (!(roleCurrent == (int)InventoryAccountRoleType.Promotion || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInCharge
                    || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInManagerment))
            {
                if(!(checkAccountTypeAndRole.AccountType == nameof(AccountType.TaiKhoanRieng) && (checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.RoleName ||
                     checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.InvGRoleName || checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.AdministratorRoleName ||
                     checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.MCRoleName || checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.InventoryRoleName)))
                {
                    var validRoleResult = await _errorInvestigationService.CheckValidInventoryRole(checkAccountTypeAndRole.InventoryLoggedInfo.AccountId, (int)InventoryAccountRoleType.Inventory);
                    if (validRoleResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validRoleResult);
                    }
                }
                //var validInventoryDateResult = await _errorInvestigationService.CheckValidInventoryDate(inventoryId);
                //if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                //{
                //    return BadRequest(validInventoryDateResult);
                //}
            }

            var result = await _errorInvestigationService.ErrorInvestigationConfirmedViewDetail(inventoryId, componentCode);
            if (result.Code == StatusCodes.Status200OK)
            {
                var jsonSetting = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new NullableDoubleConverter() },
                };

                return new JsonResult(result, jsonSetting);
            }

            return BadRequest(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationRoute.ErrorInvestigationHistories)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<ErrorInvestigationHistoriesDto>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.CannotErrorInvestigation, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Error Investigation")]
        public async Task<IActionResult> ErrorInvestigationHistories(Guid inventoryId, string componentCode)
        {
            //25122024: Nếu tài khoản có vai trò Điều tra sai số thì được phép xem hoặc chỉnh sửa điều tra:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            var roleCurrent = checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType;
            if (!(roleCurrent == (int)InventoryAccountRoleType.Promotion || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInCharge
                    || roleCurrent == (int)InventoryAccountRoleType.PromotionPersonInManagerment))
            {
                if (!(checkAccountTypeAndRole.AccountType == nameof(AccountType.TaiKhoanRieng) && checkAccountTypeAndRole.RoleName == Constants.DefaultAccount.RoleName))
                {
                    var validRoleResult = await _errorInvestigationService.CheckValidInventoryRole(checkAccountTypeAndRole.InventoryLoggedInfo.AccountId, (int)InventoryAccountRoleType.Inventory);
                    if (validRoleResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validRoleResult);
                    }
                }
                //var validInventoryDateResult = await _errorInvestigationService.CheckValidInventoryDate(inventoryId);
                //if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                //{
                //    return BadRequest(validInventoryDateResult);
                //}
            }

            var result = await _errorInvestigationService.ErrorInvestigationHistories(inventoryId, componentCode);
            if (result.Code == StatusCodes.Status200OK)
            {
                var jsonSetting = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new NullableDoubleConverter() },
                };

                return new JsonResult(result, jsonSetting);
            }

            return BadRequest(result);
        }


    }
}
