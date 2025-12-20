using System.Text.Json;
using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using Inventory.API.Service.Dto.ErrorInvestigation;
using Inventory.API.Service.ErrorInvestigation;
using static BIVN.FixedStorage.Services.Common.API.Constants.Endpoint;


namespace Inventory.API.Controllers
{
    [Route(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.root)]
    [ApiController]
    public class ErrorInvestigationWebController : ControllerBase
    {
        private readonly ILogger<ErrorInvestigationWebController> _logger;
        private readonly IErrorInvestigationWebService _errorInvestigationWebService;
        private readonly IInventoryWebService _inventoryService;
        private readonly IDocumentResultService _documentResultService;
        private readonly IInventoryService _inventoryDocService;
        private readonly InventoryContext _inventoryContext;

        public ErrorInvestigationWebController(ILogger<ErrorInvestigationWebController> logger
                                        , IInventoryWebService inventoryService
                                        , IDocumentResultService documentResultService
                                        , IInventoryService inventoryDocService
                                        , IErrorInvestigationWebService errorInvestigationWebService,
                                        InventoryContext inventoryContext
                                        )
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _documentResultService = documentResultService;
            _inventoryDocService = inventoryDocService;
            _errorInvestigationWebService = errorInvestigationWebService;
            _inventoryContext = inventoryContext;
        }

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListErrorInvestigation)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListErrorInvestigaiton()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var listErrorInvestigationWeb = new ListErrorInvestigationWebModel();
            listErrorInvestigationWeb.Plant = Request.Form["Plant"].FirstOrDefault();
            listErrorInvestigationWeb.WHLoc = Request.Form["WHLoc"].FirstOrDefault();
            listErrorInvestigationWeb.AssigneeAccount = Request.Form["AssigneeAccount"].FirstOrDefault();
            listErrorInvestigationWeb.ComponentCode = Request.Form["ComponentCode"].FirstOrDefault();
            listErrorInvestigationWeb.ErrorQuantityFrom = double.TryParse(Request.Form["ErrorQuantityFrom"].FirstOrDefault(), out var errorQuantityFrom) ? errorQuantityFrom : (double?)null;
            listErrorInvestigationWeb.ErrorQuantityTo = double.TryParse(Request.Form["ErrorQuantityTo"].FirstOrDefault(), out var errorQuantityTo) ? errorQuantityTo : (double?)null;
            listErrorInvestigationWeb.ErrorMoneyFrom = double.TryParse(Request.Form["ErrorMoneyFrom"].FirstOrDefault(), out var errorMoneyFrom) ? errorMoneyFrom : (double?)null;
            listErrorInvestigationWeb.ErrorMoneyTo = double.TryParse(Request.Form["ErrorMoneyTo"].FirstOrDefault(), out var errorMoneyTo) ? errorMoneyTo : (double?)null;
            listErrorInvestigationWeb.ErrorCategories = Request.Form["ErrorCategories[]"].Select(e => int.Parse(e)).ToList();
            listErrorInvestigationWeb.Statuses = Request.Form["Statuses[]"].Select(e => Enum.Parse<ErrorInvestigationStatusType>(e)).ToList();
            listErrorInvestigationWeb.InventoryIds = Request.Form["InventoryIds[]"].Select(Guid.Parse).ToList();
            listErrorInvestigationWeb.ComponentName = Request.Form["ComponentName"].FirstOrDefault();
            listErrorInvestigationWeb.Skip = skip;
            listErrorInvestigationWeb.Take = pageSize;
            listErrorInvestigationWeb.SortColumn = sortColumn;
            listErrorInvestigationWeb.SortColumnDirection = sortColumnDirection;

            if (listErrorInvestigationWeb.ErrorQuantityFrom.HasValue && !listErrorInvestigationWeb.ErrorQuantityTo.HasValue ||
                !listErrorInvestigationWeb.ErrorQuantityFrom.HasValue && listErrorInvestigationWeb.ErrorQuantityTo.HasValue)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }
            if (listErrorInvestigationWeb.ErrorMoneyFrom.HasValue && !listErrorInvestigationWeb.ErrorMoneyTo.HasValue ||
                !listErrorInvestigationWeb.ErrorMoneyFrom.HasValue && listErrorInvestigationWeb.ErrorMoneyTo.HasValue)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }


            if (listErrorInvestigationWeb.ErrorQuantityFrom.HasValue && listErrorInvestigationWeb.ErrorQuantityTo.HasValue && listErrorInvestigationWeb.ErrorQuantityFrom.Value > listErrorInvestigationWeb.ErrorQuantityTo.Value)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            if (listErrorInvestigationWeb.ErrorMoneyFrom.HasValue && listErrorInvestigationWeb.ErrorMoneyTo.HasValue && listErrorInvestigationWeb.ErrorMoneyFrom.Value > listErrorInvestigationWeb.ErrorMoneyTo.Value)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }


            var result = await _errorInvestigationWebService.ListErrorInvestigaiton(listErrorInvestigationWeb);
            if (result?.Data != null)
            {
                var recordsTotal = result.TotalRecords;
                var items = result.Data;

                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = items };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0, isExistDocTypeC = false };
            return Ok(jsonData2);
        }

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListErrorInvestigationExportFile)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListErrorInvestigaitonExport([FromBody] ListErrorInvestigationWebModel listErrorInvestigationWeb)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }


            if (listErrorInvestigationWeb.ErrorQuantityFrom.HasValue && !listErrorInvestigationWeb.ErrorQuantityTo.HasValue ||
                !listErrorInvestigationWeb.ErrorQuantityFrom.HasValue && listErrorInvestigationWeb.ErrorQuantityTo.HasValue)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }
            if (listErrorInvestigationWeb.ErrorMoneyFrom.HasValue && !listErrorInvestigationWeb.ErrorMoneyTo.HasValue ||
                !listErrorInvestigationWeb.ErrorMoneyFrom.HasValue && listErrorInvestigationWeb.ErrorMoneyTo.HasValue)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }


            if (listErrorInvestigationWeb.ErrorQuantityFrom.HasValue && listErrorInvestigationWeb.ErrorQuantityTo.HasValue && listErrorInvestigationWeb.ErrorQuantityFrom.Value > listErrorInvestigationWeb.ErrorQuantityTo.Value)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            if (listErrorInvestigationWeb.ErrorMoneyFrom.HasValue && listErrorInvestigationWeb.ErrorMoneyTo.HasValue && listErrorInvestigationWeb.ErrorMoneyFrom.Value > listErrorInvestigationWeb.ErrorMoneyTo.Value)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            listErrorInvestigationWeb.IsExportExcel = true;
            var result = await _errorInvestigationWebService.ListErrorInvestigaiton(listErrorInvestigationWeb);
            
            return Ok(result);
        }
        [HttpPost]
        [Route(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ExportDataAdjustment)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportDataAdjustment(Guid inventoryId)
        {
            var result = await _errorInvestigationWebService.ExportDataAjustment(inventoryId);

            return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExportDataAdjustment.xlsx");

        }

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListErrorInvestigationDocuments)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListErrorInvestigationDocuments(Guid inventoryId, string componentCode)
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int pageNum = start != null ? Convert.ToInt32(start) : 0;
            var result = await _errorInvestigationWebService.ListErrorInvestigationDocuments(inventoryId, componentCode, pageNum, pageSize);
            if (result?.Data != null)
            {
                var recordsTotal = result.TotalRecords;
                var items = result.Data;

                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = items };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0, isExistDocTypeC = false };
            return Ok(jsonData2);
        }
        [HttpGet(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListErrorInvestigationDocumentsCheck)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListErrorInvestigationDocumentsCheck([FromQuery] string? userCode, Guid inventoryId, string componentCode)
        {
            var result = await _errorInvestigationWebService.ListErrorInvestigationDocumentsCheck(userCode, inventoryId, componentCode);
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListErrorInvestigationInventoryDocsHistory)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListErrorInvestigationInventoryDocsHistory([FromRoute] string componentCode, [FromBody] ErrorInvestigationInventoryDocsHistoryModel inventories)
        {
            var result = await _errorInvestigationWebService.ListErrorInvestigationInventoryDocsHistory(componentCode, inventories);
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListErrorInvestigationHistory)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListErrorInvestigaitonHistory()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var listErrorInvestigationHistoryWeb = new ListErrorInvestigationHistoryWebModel();
            listErrorInvestigationHistoryWeb.Plant = Request.Form["Plant"].FirstOrDefault();
            listErrorInvestigationHistoryWeb.WHLoc = Request.Form["WHLoc"].FirstOrDefault();
            listErrorInvestigationHistoryWeb.ComponentCode = Request.Form["ComponentCode"].FirstOrDefault();
            listErrorInvestigationHistoryWeb.AssigneeAccount = Request.Form["AssigneeAccount"].FirstOrDefault();
            listErrorInvestigationHistoryWeb.ErrorQuantityFrom = double.TryParse(Request.Form["ErrorQuantityFrom"].FirstOrDefault(), out var errorQuantityFrom) ? errorQuantityFrom : (double?)null;
            listErrorInvestigationHistoryWeb.ErrorQuantityTo = double.TryParse(Request.Form["ErrorQuantityTo"].FirstOrDefault(), out var errorQuantityTo) ? errorQuantityTo : (double?)null;
            listErrorInvestigationHistoryWeb.ErrorMoneyFrom = double.TryParse(Request.Form["ErrorMoneyFrom"].FirstOrDefault(), out var errorMoneyFrom) ? errorMoneyFrom : (double?)null;
            listErrorInvestigationHistoryWeb.ErrorMoneyTo = double.TryParse(Request.Form["ErrorMoneyTo"].FirstOrDefault(), out var errorMoneyTo) ? errorMoneyTo : (double?)null;
            listErrorInvestigationHistoryWeb.ErrorCategories = Request.Form["ErrorCategories[]"].Select(e => int.Parse(e)).ToList();
            listErrorInvestigationHistoryWeb.InventoryIds = Request.Form["InventoryIds[]"].Select(Guid.Parse).ToList();
            listErrorInvestigationHistoryWeb.Skip = skip;
            listErrorInvestigationHistoryWeb.Take = pageSize;

            if (listErrorInvestigationHistoryWeb.ErrorQuantityFrom.HasValue && !listErrorInvestigationHistoryWeb.ErrorQuantityTo.HasValue ||
                !listErrorInvestigationHistoryWeb.ErrorQuantityFrom.HasValue && listErrorInvestigationHistoryWeb.ErrorQuantityTo.HasValue)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }
            if (listErrorInvestigationHistoryWeb.ErrorMoneyFrom.HasValue && !listErrorInvestigationHistoryWeb.ErrorMoneyTo.HasValue ||
                !listErrorInvestigationHistoryWeb.ErrorMoneyFrom.HasValue && listErrorInvestigationHistoryWeb.ErrorMoneyTo.HasValue)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }


            if (listErrorInvestigationHistoryWeb.ErrorQuantityFrom.HasValue && listErrorInvestigationHistoryWeb.ErrorQuantityTo.HasValue && listErrorInvestigationHistoryWeb.ErrorQuantityFrom.Value > listErrorInvestigationHistoryWeb.ErrorQuantityTo.Value)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            if (listErrorInvestigationHistoryWeb.ErrorMoneyFrom.HasValue && listErrorInvestigationHistoryWeb.ErrorMoneyTo.HasValue && listErrorInvestigationHistoryWeb.ErrorMoneyFrom.Value > listErrorInvestigationHistoryWeb.ErrorMoneyTo.Value)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }


            var result = await _errorInvestigationWebService.ListErrorInvestigaitonHistory(listErrorInvestigationHistoryWeb);
            if (result?.Data != null)
            {
                var recordsTotal = result.TotalRecords;
                var items = result.Data;

                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = items };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0, isExistDocTypeC = false };
            return Ok(jsonData2);
        }
        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListErrorInvestigationHistoryExportFile)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListErrorInvestigationHistoryExport([FromBody] ListErrorInvestigationHistoryWebModel listErrorInvestigationHistoryWeb)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }


            if (listErrorInvestigationHistoryWeb.ErrorQuantityFrom.HasValue && !listErrorInvestigationHistoryWeb.ErrorQuantityTo.HasValue ||
                !listErrorInvestigationHistoryWeb.ErrorQuantityFrom.HasValue && listErrorInvestigationHistoryWeb.ErrorQuantityTo.HasValue)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }
            if (listErrorInvestigationHistoryWeb.ErrorMoneyFrom.HasValue && !listErrorInvestigationHistoryWeb.ErrorMoneyTo.HasValue ||
                !listErrorInvestigationHistoryWeb.ErrorMoneyFrom.HasValue && listErrorInvestigationHistoryWeb.ErrorMoneyTo.HasValue)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }


            if (listErrorInvestigationHistoryWeb.ErrorQuantityFrom.HasValue && listErrorInvestigationHistoryWeb.ErrorQuantityTo.HasValue && listErrorInvestigationHistoryWeb.ErrorQuantityFrom.Value > listErrorInvestigationHistoryWeb.ErrorQuantityTo.Value)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            if (listErrorInvestigationHistoryWeb.ErrorMoneyFrom.HasValue && listErrorInvestigationHistoryWeb.ErrorMoneyTo.HasValue && listErrorInvestigationHistoryWeb.ErrorMoneyFrom.Value > listErrorInvestigationHistoryWeb.ErrorMoneyTo.Value)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            listErrorInvestigationHistoryWeb.IsExportExcel = true;
            var result = await _errorInvestigationWebService.ListErrorInvestigaitonHistory(listErrorInvestigationHistoryWeb);

            return Ok(result);
        }

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListInvestigationDetail)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListInvestigaitonDetail()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var listErrorInvestigationHistoryWeb = new ListErrorInvestigationHistoryWebModel();
            listErrorInvestigationHistoryWeb.ErrorCategories = Request.Form["ErrorCategories[]"].Select(e => int.Parse(e)).ToList();
            listErrorInvestigationHistoryWeb.ErrorTypes = Request.Form["ErrorTypes[]"].Select(e => Enum.Parse<ErrorType>(e)).ToList();
            listErrorInvestigationHistoryWeb.Plant = Request.Form["Plant"].FirstOrDefault();
            listErrorInvestigationHistoryWeb.WHLoc = Request.Form["WHLoc"].FirstOrDefault();
            listErrorInvestigationHistoryWeb.ComponentCode = Request.Form["ComponentCode"].FirstOrDefault();
            listErrorInvestigationHistoryWeb.InventoryIds = Request.Form["InventoryIds[]"].Select(Guid.Parse).ToList();
            listErrorInvestigationHistoryWeb.Skip = skip;
            listErrorInvestigationHistoryWeb.Take = pageSize;

            var result = await _errorInvestigationWebService.ListErrorInvestigaitonHistory(listErrorInvestigationHistoryWeb);
            if (result?.Data != null)
            {
                var recordsTotal = result.TotalRecords;
                var items = result.Data;

                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = items };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0, isExistDocTypeC = false };
            return Ok(jsonData2);
        }

        [HttpPut(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.UpdateErrorTypesForInvestigationHistory)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> UpdateErrorTypesForInvestigationHistory([FromBody] List<Guid> errorHistoryIds, [FromQuery] AdjustmentType type)
        {
            try
            {
                if (!errorHistoryIds.Any())
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = commonAPIConstant.ResponseMessages.InValidValidationMessage
                    });
                }

                var result = await _errorInvestigationWebService.UpdateErrorTypesForInvestigationHistory(errorHistoryIds, type);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);

            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện điều chỉnh loại sai số.", ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi hệ thống khi thực hiện điều chỉnh loại sai số."
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.InvestigationPercent)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> InvestigationPercent(Guid inventoryId)
        {
            var result = await _errorInvestigationWebService.InvestigationPercent(inventoryId);
            
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

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ListInvestigationDetailExport)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListInvestigaitonDetailExport([FromBody] ListErrorInvestigationHistoryWebModel listErrorInvestigationHistoryWebModel)
        {

            listErrorInvestigationHistoryWebModel.IsExportExcel = true;
            var result = await _errorInvestigationWebService.ListErrorInvestigaitonHistory(listErrorInvestigationHistoryWebModel);

            return Ok(result);
        }

        [HttpGet(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ErrorPercent)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ErrorPercent(Guid inventoryId)
        {
            var result = await _errorInvestigationWebService.ErrorPercent(inventoryId);

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

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ImportErrorInvestigationUpdate)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ImportErrorInvestigationUpdate([FromForm] IFormFile file)
        {
            try
            {
                var result = await _errorInvestigationWebService.ImportErrorInvestigationUpdate(file);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi import file cập nhật số lượng điều chỉnh.");
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ImportErrorInvestigationUpdatePivot)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ImportErrorInvestigationUpdatePivot([FromForm] IFormFile file, Guid inventoryId)
        {
            try
            {
                var result = await _errorInvestigationWebService.ImportErrorInvestigationUpdatePivot(file, inventoryId);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi import file cập nhật số lượng điều chỉnh Pivot.");
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ErrorCategoryManagement)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ErrorCategoryManagement()
        {
            var result = await _errorInvestigationWebService.ErrorCategoryManagement();

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

        [HttpGet(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.ErrorCategoryManagementById)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ErrorCategoryManagementById([FromRoute] Guid errorCategoryId)
        {
            var result = await _errorInvestigationWebService.ErrorCategoryManagementById(errorCategoryId);

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

        [HttpPost(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.AddNewErrorCategoryManagement)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> AddNewErrorCategoryManagement([FromBody] ErrorCategoryModel errorCategoryModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }
            var result = await _errorInvestigationWebService.AddNewErrorCategoryManagement(errorCategoryModel);

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

        [HttpPut(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.UpdateErrorCategoryManagement)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> UpdateErrorCategoryManagement([FromRoute] Guid errorCategoryId, [FromBody] ErrorCategoryModel errorCategoryModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }
            var result = await _errorInvestigationWebService.UpdateErrorCategoryManagement(errorCategoryId, errorCategoryModel);

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

        [HttpDelete(commonAPIConstant.Endpoint.ErrorInvestigationService.ErrorInvestigationWebRoute.RemoveErrorCategoryManagement)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> RemoveErrorCategoryManagement(Guid errorCategoryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }
            var result = await _errorInvestigationWebService.RemoveErrorCategoryManagement(errorCategoryId);

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
