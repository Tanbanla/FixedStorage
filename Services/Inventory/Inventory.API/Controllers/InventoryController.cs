using BIVN.FixedStorage.Services.Inventory.API.Attributes;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity;
using Microsoft.AspNetCore.Identity;

namespace Inventory.API.Controllers
{
    [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.root)]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly IInventoryService _inventoryService;
        private readonly HttpContext _httpContext;
        public InventoryController(ILogger<InventoryController> logger,
                                    IInventoryService inventoryService, IHttpContextAccessor httpContextAccessor
            )
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _httpContext = httpContextAccessor.HttpContext;
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.inventory_Check)]
        public async Task<IActionResult> CheckInventory(string inventoryId, string accountId)
        {
            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (accountId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            var result = await _inventoryService.CheckInventory(inventoryId, accountId);
            return Ok(result);
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.Inventory.import_DocType)]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> Import([FromForm] IFormFile file, [FromRoute] string docType, [FromRoute] Guid inventoryId)
        {
            try
            {
                var result = await _inventoryService.ImportInventoryDocumentAsync(file, docType, inventoryId);

                var currDate = DateTime.Now.ToString(Constants.DatetimeFormat);
                var fileType = Constants.FileResponse.ExcelType;
                var fileName = string.Format("Phieu{1}_Fileloi_{0}.xlsx", currDate, docType);

                if (result.Code == (int)HttpStatusCodes.InvalidFileExcel ||
                    result.Data.Code == (int)HttpStatusCodes.InvalidFileExcel)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.InvalidFileExcel,
                        Message = result?.Data?.Message ?? result.Message,
                    });
                }

                if (result.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(result);
                }

                return Ok(new
                {
                    Bytes = result.Data.Result,
                    FileType = fileType,
                    FileName = fileName,
                    SuccessCount = result.Data.SuccessCount,
                    FailCount = result.Data.FailCount,
                });
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
        /// <summary>
        /// Validate import inventory document type c
        /// </summary>
        /// <param name="file"></param>
        /// <param name="inventoryId"></param>
        /// <param name="isBypassWarning">Bypass check warning - default is false</param>
        /// <returns></returns>
        [HttpPost(commonAPIConstant.Endpoint.InventoryService.Inventory.doc_C_Validate)]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ValidateDocTypeC([FromForm] IFormFile file, [FromRoute] Guid inventoryId, [FromQuery] bool isBypassWarning = false)
        {
            var result = await _inventoryService.ValidateInventoryDocTypeC(file, inventoryId, isBypassWarning);
            return Ok(result);
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.Inventory.import_Doc_C)]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ImportInventoryDocTypeC([FromForm] IFormFile file, [FromRoute] Guid inventoryId, [FromQuery] bool isBypassWarning = false)
        {
            var result = await _inventoryService.ImportInventoryDocTypeC(file, inventoryId, isBypassWarning);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.scan_doc_AE)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<SwaggerDocABEListResponseModel>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<bool>))]
        [ApiExplorerSettings(GroupName = "Doc List")]
        public async Task<IActionResult> ScanDocsAE(string inventoryId, string accountId, string componentCode, string positionCode, string docCode, InventoryActionType? actionType, [FromQuery] bool isErrorInvestigation = false)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (string.IsNullOrEmpty(componentCode.Trim()))
            {
                ModelState.AddModelError(nameof(componentCode), Constants.ResponseMessages.InValidValidationMessage);
            }
            //if (string.IsNullOrEmpty(positionCode.Trim()))
            //{
            //    ModelState.AddModelError(nameof(componentCode), Constants.ResponseMessages.InValidValidationMessage);
            //}
            //if (string.IsNullOrEmpty(docCode.Trim()))
            //{
            //    ModelState.AddModelError(nameof(docCode), Constants.ResponseMessages.InValidValidationMessage);
            //}
            if (!actionType.HasValue)
            {
                ModelState.Remove(nameof(actionType));
                ModelState.TryAddModelError(nameof(actionType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);

            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                if (!isErrorInvestigation)
                {
                    var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                    if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validInventoryDateResult);
                    }
                }

            }

            var docsABEResult = await _inventoryService.ScanDocsAE(inventoryGuid, accountGuid, componentCode.Trim(), positionCode, docCode, actionType.Value, isErrorInvestigation);
            if (docsABEResult.Code == 200)
            {
                return Ok(docsABEResult);
            }
            return BadRequest(docsABEResult);
        }

        [HttpPost]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.doc_C)]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<DocCListViewModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<bool>))]
        [ApiExplorerSettings(GroupName = "Doc List")]
        public async Task<IActionResult> DocListC([FromBody] ListDocCFilterModel listDocCFilterModel)
        {
            if (listDocCFilterModel == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }
            if (!Guid.TryParse(listDocCFilterModel.InventoryId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(listDocCFilterModel.InventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(listDocCFilterModel.AccountId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(listDocCFilterModel.AccountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(listDocCFilterModel.MachineModel))
            {
                ModelState.AddModelError(nameof(listDocCFilterModel.MachineModel), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (string.IsNullOrEmpty(listDocCFilterModel.LineName))
            {
                ModelState.AddModelError(nameof(listDocCFilterModel.LineName), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (string.IsNullOrEmpty(listDocCFilterModel.MachineType))
            {
                ModelState.AddModelError(nameof(listDocCFilterModel.MachineType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!listDocCFilterModel.ActionType.HasValue
                || !Enum.IsDefined(typeof(InventoryActionType), listDocCFilterModel.ActionType))
            {
                ModelState.Remove(nameof(listDocCFilterModel.ActionType));
                ModelState.TryAddModelError(nameof(listDocCFilterModel.ActionType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var validRoleResult = await _inventoryService.CheckValidInventoryRole(listDocCFilterModel.AccountId, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
            if (validRoleResult.Code != StatusCodes.Status200OK)
            {
                return BadRequest(validRoleResult);
            }
            var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(listDocCFilterModel.InventoryId);
            if (validInventoryDateResult.Code != StatusCodes.Status200OK)
            {
                return BadRequest(validInventoryDateResult);
            }

            listDocCFilterModel.MachineType.Trim();
            listDocCFilterModel.MachineModel.Trim();
            listDocCFilterModel.LineName.Trim();

            var docListCResult = await _inventoryService.GetDocsC(listDocCFilterModel);
            if (docListCResult.Code == StatusCodes.Status200OK)
            {
                return Ok(docListCResult);
            }

            return BadRequest(docListCResult);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.detail_Document)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<DocumentDetailModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<bool>))]
        [ApiExplorerSettings(GroupName = "Doc Detail")]
        public async Task<IActionResult> DocumentDetail(string inventoryId, string accountId, string documentId, InventoryActionType? actionType, string searchTerm = "", int page = 1, [FromQuery] bool isErrorInvestigation = false)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(documentId) || !Guid.TryParse(documentId, out _))
            {
                ModelState.AddModelError(nameof(documentId), Constants.ResponseMessages.InvalidId);
            }
            if (page <= 0)
            {
                ModelState.AddModelError(nameof(page), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!actionType.HasValue)
            {
                ModelState.Remove(nameof(actionType));
                ModelState.TryAddModelError(nameof(actionType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);
            var docGuid = Guid.Parse(documentId);

            //20240910: Nếu tài khoản vai trò Xúc Tiến => Được xem hoặc chỉnh sửa phiếu trong đợt kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != (int)InventoryAccountRoleType.Promotion)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                if (!isErrorInvestigation)
                {
                    var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                    if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validInventoryDateResult);
                    }
                }
            }

            var filterModel = new DocumentDetailFilterModel
            {
                InventoryId = inventoryGuid,
                AccountId = accountGuid,
                DocumentId = docGuid,
                Page = page,
                SearchTerm = string.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.Trim(),
                ActionType = actionType.Value,
            };

            var detailResult = await _inventoryService.DetailOfDocument(filterModel);
            if (detailResult.Code == StatusCodes.Status200OK)
            {
                return Ok(detailResult);
            }
            return BadRequest(detailResult);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.detailHistoryId)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<HistoryDetailViewModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<bool>))]
        [ApiExplorerSettings(GroupName = "Doc Detail")]
        public async Task<IActionResult> HistoryDetail(string inventoryId, string accountId, string historyId, string searchTerm = "", int page = 1)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(historyId) || !Guid.TryParse(historyId, out _))
            {
                ModelState.AddModelError(nameof(historyId), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);
            var historyGuid = Guid.Parse(historyId);

            var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
            if (validRoleResult.Code != StatusCodes.Status200OK)
            {
                return BadRequest(validRoleResult);
            }
            var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
            if (validInventoryDateResult.Code != StatusCodes.Status200OK)
            {
                return BadRequest(validInventoryDateResult);
            }

            searchTerm.Trim().ToLower();
            var historyDetailResult = await _inventoryService.HistoryDetail(inventoryGuid, accountGuid, historyGuid, searchTerm, page);
            if (historyDetailResult.Code == StatusCodes.Status200OK)
            {
                return Ok(historyDetailResult);
            }
            return BadRequest(historyDetailResult);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_DocC_Models)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<string>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Doc C")]
        public async Task<IActionResult> DropDownModelsDocC(string inventoryId, string accountId)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);

            //20240910: Nếu tài khoản vai trò Xúc Tiến => Được xem hoặc chỉnh sửa phiếu trong đợt kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            var modelsResult = await _inventoryService.GetModelCodesForDocC(inventoryGuid, accountGuid);
            if (modelsResult.Code == StatusCodes.Status200OK)
            {
                return Ok(modelsResult);
            }
            return BadRequest(modelsResult);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_DocC_Machines)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<string>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Doc C")]
        public async Task<IActionResult> DropDownModelsDocC(string inventoryId, string accountId, string machineModel)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(machineModel))
            {
                ModelState.AddModelError(nameof(machineModel), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);
            machineModel.Trim();

            //20240910: Nếu tài khoản vai trò Xúc Tiến => Được xem hoặc chỉnh sửa phiếu trong đợt kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            var machineTypesResult = await _inventoryService.GetMachineTypesDocC(inventoryGuid, accountGuid, machineModel);
            if (machineTypesResult.Code == StatusCodes.Status200OK)
            {
                return Ok(machineTypesResult);
            }
            return BadRequest(machineTypesResult);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_DocC_Lines)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<string>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Doc C")]
        public async Task<IActionResult> DropDownModelsDocC(string inventoryId, string accountId, string machineModel, string machineType)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(machineModel))
            {
                ModelState.AddModelError(nameof(machineModel), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (string.IsNullOrEmpty(machineType))
            {
                ModelState.AddModelError(nameof(machineType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InvalidId
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);
            machineModel.Trim();
            machineType.Trim();


            //20240910: Nếu tài khoản vai trò Xúc Tiến => Được xem hoặc chỉnh sửa phiếu trong đợt kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            var linesResult = await _inventoryService.GetLineNamesDocC(inventoryGuid, accountGuid, machineModel, machineType);
            if (linesResult.Code == StatusCodes.Status200OK)
            {
                return Ok(linesResult);
            }
            return BadRequest(linesResult);
        }

        [HttpPost]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.submit_Inventory)]
        public async Task<IActionResult> SubmitInventory(string inventoryId, string accountId, string docId, [FromForm] SubmitInventoryDto submitInventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }

            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (accountId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (docId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }

            if (submitInventoryDto.UserCode.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });

            }

            var result = await _inventoryService.SubmitInventory(inventoryId, accountId, docId, submitInventoryDto);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.submit_Confirm)]
        public async Task<IActionResult> SubmitConfirm([FromRoute] Guid inventoryId, [FromRoute] Guid accountId, [FromRoute] Guid docId, SubmitInventoryAction actionType, [FromForm] SubmitInventoryDto submitInventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }
            //if (inventoryId.IsNullOrEmpty())
            //{
            //    return BadRequest(new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = Constants.ResponseMessages.InvalidId,
            //    });
            //}
            //if (accountId.IsNullOrEmpty())
            //{
            //    return BadRequest(new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = Constants.ResponseMessages.InvalidId,
            //    });
            //}
            //if (docId.IsNullOrEmpty())
            //{
            //    return BadRequest(new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = Constants.ResponseMessages.InvalidId,
            //    });
            //}

            if (submitInventoryDto.UserCode.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }

            var result = await _inventoryService.SubmitConfirm(inventoryId, accountId, docId, actionType, submitInventoryDto);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_Department)]
        public async Task<IActionResult> DropDownDepartment(string inventoryId, string accountId)
        {
            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (accountId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            var result = await _inventoryService.DropDownDepartment(inventoryId, accountId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_Department_By_Location)]
        public async Task<IActionResult> DropDownLocation(string inventoryId, string accountId, string departmentName = "-1")
        {
            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (accountId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (departmentName.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }
            var result = await _inventoryService.DropDownLocation(inventoryId, accountId, departmentName);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_Location)]
        public async Task<IActionResult> DropDownComponentCode(string inventoryId, string accountId, string departmentName = "-1", string locationName = "-1")
        {
            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (accountId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (departmentName.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (locationName.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }
            var result = await _inventoryService.DropDownComponentCode(inventoryId, accountId, departmentName, locationName);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.list_Audit)]
        public async Task<IActionResult> ListAudit([FromBody] ListAuditFilterDto listAuditFilterDto)
        {
            if (listAuditFilterDto is null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }
            //if (listAuditFilterDto.InventoryId.IsNullOrEmpty())
            //{
            //    return BadRequest(new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = Constants.ResponseMessages.InvalidId,
            //    });
            //}
            //if (listAuditFilterDto.AccountId.IsNullOrEmpty())
            //{
            //    return BadRequest(new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = Constants.ResponseMessages.InvalidId,
            //    });
            //}
            if (listAuditFilterDto.DepartmentName.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }
            if (listAuditFilterDto.LocationName.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }
            if (listAuditFilterDto.ComponentCode.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }

            var result = await _inventoryService.ListAudit(listAuditFilterDto);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.audit_Scan_QR)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<AuditInfoModel>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.InventoryNotFoundComponentCode, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.ComponentNotInYourAudit, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotConfirmBeforeAudit, Type = typeof(ResponseModel<>))]
        public async Task<IActionResult> AuditScanQR(string inventoryId, string accountId, string componentCode)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(componentCode))
            {
                ModelState.AddModelError(nameof(componentCode), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);
            componentCode.Trim();

            var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Audit);
            if (validRoleResult.Code != StatusCodes.Status200OK)
            {
                return BadRequest(validRoleResult);
            }
            var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
            if (validInventoryDateResult.Code != StatusCodes.Status200OK)
            {
                return BadRequest(validInventoryDateResult);
            }

            //valid for audit target of current user
            var isValidateAuditTarget = await _inventoryService.CheckValidAuditTarget(inventoryGuid, accountGuid, componentCode);
            if (isValidateAuditTarget.Code != StatusCodes.Status200OK)
            {
                return BadRequest(isValidateAuditTarget);
            }


            var scanQRResults = await _inventoryService.ScanQR(inventoryGuid, accountGuid, componentCode);
            if (scanQRResults.Code == StatusCodes.Status200OK)
            {
                return Ok(scanQRResults);
            }

            return BadRequest(scanQRResults);
        }

        [HttpPost]
        //[BIVN.FixedStorage.Services.Inventory.API.Attributes.AllowAnonymous]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.submit_Audit)]
        public async Task<IActionResult> SubmitAudit(string inventoryId, string accountId, string docId, SubmitInventoryAction actionType, [FromForm] SubmitInventoryDto submitInventoryDto)
        {
            if (!submitInventoryDto.IsAuditWebsite)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = ModelState.ErrorTextMessages()
                    });
                }
            }

            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (accountId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }
            if (docId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InvalidId,
                });
            }

            if (submitInventoryDto.UserCode.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }

            var result = await _inventoryService.SubmitAudit(inventoryId, accountId, docId, actionType, submitInventoryDto);
            return Ok(result);
        }


        [HttpPost]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [HttpPost(commonAPIConstant.Endpoint.InventoryService.Inventory.isHighlight_Check)]
        public async Task<IActionResult> IsHightLightCheck([FromBody] CheckIsHightLightDocTypeCDto checkIsHightLightDocTypeCDto)
        {
            var result = await _inventoryService.IsHightLightCheck(checkIsHightLightDocTypeCDto);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_DocB_Models)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<string>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Doc B")]
        public async Task<IActionResult> DropDownModelsDocB(string inventoryId, string accountId)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);

            //20240910: Nếu tài khoản có vai trò Xúc Tiến - được phép xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            var modelsResult = await _inventoryService.GetModelCodesForDocB(inventoryGuid, accountGuid);
            if (modelsResult.Code == StatusCodes.Status200OK)
            {
                return Ok(modelsResult);
            }
            return BadRequest(modelsResult);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_DocB_Machines)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<string>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Doc B")]
        public async Task<IActionResult> DropDownMachinesDocB(string inventoryId, string accountId, string machineModel)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(machineModel))
            {
                ModelState.AddModelError(nameof(machineModel), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);
            machineModel.Trim();

            //20240910: Nếu tài khoản có vai trò Xúc Tiến - được phép xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            var machineTypesResult = await _inventoryService.GetMachineTypesDocB(inventoryGuid, accountGuid, machineModel);
            if (machineTypesResult.Code == StatusCodes.Status200OK)
            {
                return Ok(machineTypesResult);
            }
            return BadRequest(machineTypesResult);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_DocB_ModelCodes)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<string>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Doc B")]
        public async Task<IActionResult> DropDownModelCodesDocB(string inventoryId, string accountId, string machineModel, string machineType)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(machineModel))
            {
                ModelState.AddModelError(nameof(machineModel), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (string.IsNullOrEmpty(machineType))
            {
                ModelState.AddModelError(nameof(machineType), Constants.ResponseMessages.InValidValidationMessage);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InvalidId
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);
            machineModel.Trim();
            machineType.Trim();

            //20240910: Nếu tài khoản có vai trò Xúc Tiến - được phép xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            var linesResult = await _inventoryService.GetDropDownModelCodesForDocB(inventoryGuid, accountGuid, machineModel, machineType);
            if (linesResult.Code == StatusCodes.Status200OK)
            {
                return Ok(linesResult);
            }
            return BadRequest(linesResult);
        }

        [HttpGet]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.dropdown_DocB_Lines)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<string>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<>))]
        [ApiExplorerSettings(GroupName = "Doc B")]
        public async Task<IActionResult> DropDownLinesDocB(string inventoryId, string accountId, string machineModel, string machineType, string modelCode)
        {
            if (string.IsNullOrEmpty(inventoryId) || !Guid.TryParse(inventoryId, out _))
            {
                ModelState.AddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out _))
            {
                ModelState.AddModelError(nameof(accountId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(machineModel))
            {
                ModelState.AddModelError(nameof(machineModel), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (string.IsNullOrEmpty(machineType))
            {
                ModelState.AddModelError(nameof(machineType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (string.IsNullOrEmpty(modelCode))
            {
                ModelState.AddModelError(nameof(modelCode), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(inventoryId);
            var accountGuid = Guid.Parse(accountId);

            //20240910: Nếu tài khoản có vai trò Xúc Tiến - được phép xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(Guid.Parse(inventoryId));
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            var modelsResult = await _inventoryService.GetLineNamesDocB(inventoryGuid, accountGuid, machineModel, machineType, modelCode);
            if (modelsResult.Code == StatusCodes.Status200OK)
            {
                return Ok(modelsResult);
            }
            return BadRequest(modelsResult);
        }

        [HttpPost]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.doc_B)]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<DocBListViewModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<bool>))]
        [ApiExplorerSettings(GroupName = "Doc List")]
        public async Task<IActionResult> DocListB([FromBody] ListDocBFilterModel listDocBFilterModel)
        {
            //20240814: Comment Here:
            if (listDocBFilterModel == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }
            if (!Guid.TryParse(listDocBFilterModel.InventoryId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(listDocBFilterModel.InventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(listDocBFilterModel.AccountId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(listDocBFilterModel.AccountId), Constants.ResponseMessages.InvalidId);
            }
            //if (string.IsNullOrEmpty(listDocBFilterModel.MachineModel))
            //{
            //    ModelState.AddModelError(nameof(listDocBFilterModel.MachineModel), Constants.ResponseMessages.InValidValidationMessage);
            //}
            //if (string.IsNullOrEmpty(listDocBFilterModel.ModelCode))
            //{
            //    ModelState.AddModelError(nameof(listDocBFilterModel.ModelCode), Constants.ResponseMessages.InValidValidationMessage);
            //}
            //if (string.IsNullOrEmpty(listDocBFilterModel.MachineType))
            //{
            //    ModelState.AddModelError(nameof(listDocBFilterModel.MachineType), Constants.ResponseMessages.InValidValidationMessage);
            //}
            if (!listDocBFilterModel.ActionType.HasValue
                || !Enum.IsDefined(typeof(InventoryActionType), listDocBFilterModel.ActionType))
            {
                ModelState.Remove(nameof(listDocBFilterModel.ActionType));
                ModelState.TryAddModelError(nameof(listDocBFilterModel.ActionType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            //20240910: Nếu tài khoản có vai trò Xúc Tiến - được phép xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(listDocBFilterModel.AccountId, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(listDocBFilterModel.InventoryId);
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            //listDocBFilterModel.MachineType.Trim();
            //listDocBFilterModel.MachineModel.Trim();
            //listDocBFilterModel.ModelCode.Trim();

            var docListBResult = await _inventoryService.GetDocsB(listDocBFilterModel);
            if (docListBResult.Code == StatusCodes.Status200OK)
            {
                return Ok(docListBResult);
            }

            return BadRequest(docListBResult);
        }


        [HttpPost]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.doc_AE)]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<DocAEListViewModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<bool>))]
        [ApiExplorerSettings(GroupName = "Doc List")]
        public async Task<IActionResult> DocListAE([FromBody] ListDocAEFilterModel listDocAEFilterModel)
        {
            if (listDocAEFilterModel == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }
            if (!Guid.TryParse(listDocAEFilterModel.InventoryId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(listDocAEFilterModel.InventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(listDocAEFilterModel.AccountId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(listDocAEFilterModel.AccountId), Constants.ResponseMessages.InvalidId);
            }
            if (!listDocAEFilterModel.ActionType.HasValue
                || !Enum.IsDefined(typeof(InventoryActionType), listDocAEFilterModel.ActionType))
            {
                ModelState.Remove(nameof(listDocAEFilterModel.ActionType));
                ModelState.TryAddModelError(nameof(listDocAEFilterModel.ActionType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            //20240910: Nếu tài khoản có vai trò Xúc Tiến thì được phép xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(listDocAEFilterModel.AccountId, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(listDocAEFilterModel.InventoryId);
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            var docListAEResult = await _inventoryService.GetDocsAE(listDocAEFilterModel);
            if (docListAEResult.Code == StatusCodes.Status200OK)
            {
                return Ok(docListAEResult);
            }

            return BadRequest(docListAEResult);
        }

        [HttpPost]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.scan_doc_B)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<SwaggerDocABEListResponseModel>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<bool>))]
        [ApiExplorerSettings(GroupName = "Doc List")]
        public async Task<IActionResult> ScanDocB([FromBody] ScanDocBFilterModel model, [FromQuery] bool isErrorInvestigation = false)
        {
            if (string.IsNullOrEmpty(model.InventoryId) || !Guid.TryParse(model.InventoryId, out _))
            {
                ModelState.AddModelError(nameof(model.InventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (string.IsNullOrEmpty(model.AccountId) || !Guid.TryParse(model.AccountId, out _))
            {
                ModelState.AddModelError(nameof(model.AccountId), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (string.IsNullOrEmpty(model.ComponentCode.Trim()))
            {
                ModelState.AddModelError(nameof(model.ComponentCode), Constants.ResponseMessages.InValidValidationMessage);
            }
            //if (string.IsNullOrEmpty(model.MachineModel.Trim()))
            //{
            //    ModelState.AddModelError(nameof(model.MachineModel), Constants.ResponseMessages.InValidValidationMessage);
            //}
            //if (string.IsNullOrEmpty(model.MachineType.Trim()))
            //{
            //    ModelState.AddModelError(nameof(model.MachineType), Constants.ResponseMessages.InValidValidationMessage);
            //}
            //if (string.IsNullOrEmpty(model.ModelCode.Trim()))
            //{
            //    ModelState.AddModelError(nameof(model.ModelCode), Constants.ResponseMessages.InValidValidationMessage);
            //}
            if (!model.ActionType.HasValue)
            {
                ModelState.Remove(nameof(model.ActionType));
                ModelState.TryAddModelError(nameof(model.ActionType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            var inventoryGuid = Guid.Parse(model.InventoryId);
            var accountGuid = Guid.Parse(model.AccountId);

            //20240910: Nếu tài khoản có vai trò Xúc Tiến thì được phép xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(accountGuid, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                if (!isErrorInvestigation)
                {
                    var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(inventoryGuid);
                    if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                    {
                        return BadRequest(validInventoryDateResult);
                    }
                }
            }

            var scanDocBResult = await _inventoryService.ScanDocB(model, isErrorInvestigation);
            if (scanDocBResult.Code == 200)
            {
                return Ok(scanDocBResult);
            }
            return BadRequest(scanDocBResult);
        }

        [HttpPost]
        [Route(commonAPIConstant.Endpoint.InventoryService.Inventory.list_doc_C)]
        [Authorize(commonAPIConstant.Permissions.MOBILE_ACCESS)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<DocCListViewModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        [SwaggerResponse((int)HttpStatusCodes.NotYetInventoryDate, Type = typeof(ResponseModel<bool>))]
        [ApiExplorerSettings(GroupName = "Doc List")]
        public async Task<IActionResult> ListDocC([FromBody] ListDocCFilterModel listDocCFilterModel)
        {
            //20240814: Comment Here:
            if (listDocCFilterModel == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }
            if (!Guid.TryParse(listDocCFilterModel.InventoryId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(listDocCFilterModel.InventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(listDocCFilterModel.AccountId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(listDocCFilterModel.AccountId), Constants.ResponseMessages.InvalidId);
            }
            //if (string.IsNullOrEmpty(listDocCFilterModel.MachineModel))
            //{
            //    ModelState.AddModelError(nameof(listDocCFilterModel.MachineModel), Constants.ResponseMessages.InValidValidationMessage);
            //}
            //if (string.IsNullOrEmpty(listDocCFilterModel.LineName))
            //{
            //    ModelState.AddModelError(nameof(listDocCFilterModel.LineName), Constants.ResponseMessages.InValidValidationMessage);
            //}
            //if (string.IsNullOrEmpty(listDocCFilterModel.MachineType))
            //{
            //    ModelState.AddModelError(nameof(listDocCFilterModel.MachineType), Constants.ResponseMessages.InValidValidationMessage);
            //}
            if (!listDocCFilterModel.ActionType.HasValue
                || !Enum.IsDefined(typeof(InventoryActionType), listDocCFilterModel.ActionType))
            {
                ModelState.Remove(nameof(listDocCFilterModel.ActionType));
                ModelState.TryAddModelError(nameof(listDocCFilterModel.ActionType), Constants.ResponseMessages.InValidValidationMessage);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            //20240910: Nếu tài khoản có vai trò Xúc Tiến thì được phép xem hoặc cập nhật kiểm kê:
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType != 2)
            {
                var validRoleResult = await _inventoryService.CheckValidInventoryRole(listDocCFilterModel.AccountId, (int)InventoryAccountRoleType.Inventory, (int)InventoryAccountRoleType.Audit);
                if (validRoleResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validRoleResult);
                }
                var validInventoryDateResult = await _inventoryService.CheckValidInventoryDate(listDocCFilterModel.InventoryId);
                if (validInventoryDateResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(validInventoryDateResult);
                }
            }

            //listDocCFilterModel.MachineType.Trim();
            //listDocCFilterModel.MachineModel.Trim();
            //listDocCFilterModel.LineName.Trim();

            var docListCResult = await _inventoryService.ListDocC(listDocCFilterModel);
            if (docListCResult.Code == StatusCodes.Status200OK)
            {
                return Ok(docListCResult);
            }

            return BadRequest(docListCResult);
        }


        [HttpPost(commonAPIConstant.Endpoint.InventoryService.Inventory.IMPORT_DOC_SHIP)]
        [Authorize(commonAPIConstant.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ImportDocShip([FromForm] IFormFile file, [FromRoute] Guid inventoryId)
        {
            try
            {
                var result = await _inventoryService.ImportInventoryDocumentShip(file, inventoryId);

                var currDate = DateTime.Now.ToString(Constants.DatetimeFormat);
                var fileType = Constants.FileResponse.ExcelType;
                var fileName = string.Format("Phieu{1}_Fileloi_{0}.xlsx", currDate, "Ship");

                if (result.Code == (int)HttpStatusCodes.InvalidFileExcel ||
                    result.Data.Code == (int)HttpStatusCodes.InvalidFileExcel)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.InvalidFileExcel,
                        Message = result?.Data?.Message ?? result.Message,
                    });
                }

                if (result.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(result);
                }

                return Ok(new
                {
                    Bytes = result.Data.Result,
                    FileType = fileType,
                    FileName = fileName,
                    SuccessCount = result.Data.SuccessCount,
                    FailCount = result.Data.FailCount,
                });
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
