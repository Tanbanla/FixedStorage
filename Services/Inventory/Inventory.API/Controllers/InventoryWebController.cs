using BIVN.FixedStorage.Services.Common.API.Dto.QRCode;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Inventory.API.Controllers
{
    [Route(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.root)]
    [ApiController]
    public class InventoryWebController : Controller
    {
        private readonly ILogger<InventoryWebController> _logger;
        private readonly IInventoryWebService _inventoryService;
        private readonly IDocumentResultService _documentResultService;
        private readonly IInventoryService _inventoryDocService;

        public InventoryWebController(ILogger<InventoryWebController> logger
                                        , IInventoryWebService inventoryService
                                        , IDocumentResultService documentResultService
                                        , IInventoryService inventoryDocService
                                        )
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _documentResultService = documentResultService;
            _inventoryDocService = inventoryDocService;
        }

        [HttpPost]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListInventory()
        {
            const string dateFormat = Constants.DayMonthYearFormat;

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var fromDate = Request.Form["InventoryDateStart"].FirstOrDefault();
            var toDate = Request.Form["InventoryDateEnd"].FirstOrDefault();

            var listInventory = new ListInventoryDto();
            listInventory.CreatedBy = Request.Form["CreatedBy"].FirstOrDefault();
            listInventory.Statuses = Request.Form["Statuses[]"].ToList();

            if (!string.IsNullOrEmpty(fromDate))
            {
                if (DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateFrom))
                {
                    listInventory.InventoryDateStart = parsedDateFrom;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                    listInventory.InventoryDateStart = null;
                }
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                if (DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTo))
                {
                    listInventory.InventoryDateEnd = parsedDateTo;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                    listInventory.InventoryDateEnd = null;
                }
            }

            var result = await _inventoryService.ListInventory(listInventory);

            if (result?.Data != null)
            {
                var allRecords = result?.Data;
                int recordsTotal = 0;
                recordsTotal = result.Data.Count();

                var data = result.Data.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0 };
            return Ok(jsonData2);
        }

        [HttpPost]
        [Route(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.createInventory)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryDto createInventoryDto)
        {
            if (createInventoryDto.InventoryDate.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = commonAPIConstant.ResponseMessages.InvalidInventoryDate
                });
            }

            if (createInventoryDto.AuditFailPercentage == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = commonAPIConstant.ResponseMessages.InventoryDegree
                });
            }

            var result = await _inventoryService.CreateInventory(createInventoryDto);
            return Ok(result);
        }

        [HttpPost]
        [Route(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listInventoryToExport)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListInventoryToExport([FromBody] ListInventoryDto listInventory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages(),
                });
            }
            var result = await _inventoryService.ListInventoryToExport(listInventory);
            return Ok(result);
        }

        [HttpPut(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.updateStatusInventory)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> UpdateStatusInventory(string inventoryId, InventoryStatus status, string userId)
        {
            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = commonAPIConstant.ResponseMessages.InvalidId,
                });
            }
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = commonAPIConstant.ResponseMessages.InvalidId,
                });
            }
            var result = await _inventoryService.UpdateStatusInventory(inventoryId, status, userId);

            return Ok(result);
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.inventoryDetail)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> InventoryDetail(string inventoryId)
        {
            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = commonAPIConstant.ResponseMessages.InvalidId,
                });
            }
            var result = await _inventoryService.GetInventoryDetail(inventoryId);

            return Ok(result);
        }

        [HttpPut(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.updateInventoryDetail)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> UpdateInventoryDetail([FromBody] UpdateInventoryDetail updateInventoryDetail, string inventoryId)
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
                    Message = commonAPIConstant.ResponseMessages.InvalidId,
                });
            }

            var result = await _inventoryService.UpdateInventoryDetail(updateInventoryDetail, inventoryId);

            return Ok(result);
        }


        [HttpPut(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.receivedDocStatus)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ReceivedDocStatus([FromBody] List<Guid> docIds)
        {
            try
            {
                if (!docIds.Any())
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = commonAPIConstant.ResponseMessages.InValidValidationMessage
                    });
                }

                var result = await _inventoryService.UpdateToReceivedDoc(docIds);
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện cập nhật trạng thái tiếp nhận phiếu.", ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi hệ thống khi thực hiện cập nhật trạng thái phiếu."
                });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listInventoryDocumentToExport)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListInventoryDocumentToExport([FromBody] ListInventoryDocumentDto listInventoryDocument, string inventoryId)
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
                    Message = commonAPIConstant.ResponseMessages.InvalidId,
                });
            }

            if (!string.IsNullOrEmpty(listInventoryDocument.DocNumberFrom) && string.IsNullOrEmpty(listInventoryDocument.DocNumberTo) ||
                string.IsNullOrEmpty(listInventoryDocument.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocument.DocNumberTo))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }

            if (!string.IsNullOrEmpty(listInventoryDocument.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocument.DocNumberTo) && int.Parse(listInventoryDocument.DocNumberFrom) > int.Parse(listInventoryDocument.DocNumberTo))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }
            listInventoryDocument.IsExport = true;
            var result = await _inventoryService.GetInventoryDocument(listInventoryDocument, inventoryId);

            return Ok(result);
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listInventoryDocument)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListInventoryDocument(string inventoryId)
        {
            if (inventoryId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = commonAPIConstant.ResponseMessages.InvalidId,
                });
            }

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var listInventoryDocument = new ListInventoryDocumentDto();
            listInventoryDocument.Plant = Request.Form["Plant"].FirstOrDefault();
            listInventoryDocument.WHLoc = Request.Form["WHLoc"].FirstOrDefault();
            listInventoryDocument.DocNumberFrom = Request.Form["DocNumberFrom"].FirstOrDefault();
            listInventoryDocument.DocNumberTo = Request.Form["DocNumberTo"].FirstOrDefault();
            listInventoryDocument.ComponentCode = Request.Form["ComponentCode"].FirstOrDefault();
            listInventoryDocument.ModelCode = Request.Form["ModelCode"].FirstOrDefault();
            listInventoryDocument.AssigneeAccount = Request.Form["AssigneeAccount"].FirstOrDefault();
            //listInventoryDocument.IsCheckAllDepartment = Request.Form["IsCheckAllDepartment"].FirstOrDefault();
            //listInventoryDocument.IsCheckAllLocation = Request.Form["IsCheckAllLocation"].FirstOrDefault();
            //listInventoryDocument.IsCheckAllDocType = Request.Form["IsCheckAllDocType"].FirstOrDefault();

            listInventoryDocument.Departments = Request.Form["Departments[]"].ToList();
            listInventoryDocument.Locations = Request.Form["Locations[]"].ToList();
            listInventoryDocument.DocTypes = Request.Form["DocTypes[]"].ToList();

            listInventoryDocument.Skip = skip;
            listInventoryDocument.Take = pageSize;

            if (!string.IsNullOrEmpty(listInventoryDocument.DocNumberFrom) && string.IsNullOrEmpty(listInventoryDocument.DocNumberTo) ||
                string.IsNullOrEmpty(listInventoryDocument.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocument.DocNumberTo))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }

            if (!string.IsNullOrEmpty(listInventoryDocument.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocument.DocNumberTo) && int.Parse(listInventoryDocument.DocNumberFrom) > int.Parse(listInventoryDocument.DocNumberTo))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            var result = await _inventoryService.GetInventoryDocument(listInventoryDocument, inventoryId);
            if (result?.Data != null)
            {
                var recordsTotal = result.TotalRecords;
                var items = result.Data;

                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = items, isExistDocTypeC = result?.IsExistDocTypeC };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0, isExistDocTypeC = false };
            return Ok(jsonData2);
        }


        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.downloadUpdateStatusFileTemplate)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DownloadUpdateStatusFileTemplate()
        {
            try
            {
                var result = await _inventoryService.DownloadUploadDocStatusFileTemplate();
                if (result.Code == StatusCodes.Status200OK)
                {
                    var currDate = DateTime.Now.ToString(Constants.DatetimeFormat);
                    var fileType = Constants.FileResponse.ExcelType;
                    var fileName = $"BieuMauUpdateStatus_{currDate}.xlsx";

                    return Ok(new
                    {
                        Bytes = result.Data,
                        FileType = fileType,
                        FileName = fileName,
                    });
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện tải biểu mẫu upload trạng thái phiếu.", ex.Message);
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi hệ thống khi thực hiện tải biểu mẫu upload trạng thái phiếu."
                });
            }
        }


        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.uploadChangeDocStatus)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> UploadChangeDocStatus([FromForm] IFormFile file)
        {
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);
                string[] allowExtension = new[] { ".xlsx" };

                if (allowExtension.Contains(fileExtension) == false || file.Length < 0)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = commonAPIConstant.ResponseMessages.InvalidFileFormat
                    });
                }

                var result = await _inventoryService.UploadChangeDocStatus(file);
                if (result.Code == StatusCodes.Status200OK)
                {
                    var currDate = DateTime.Now.ToString(Constants.DatetimeFormat);
                    var fileType = Constants.FileResponse.ExcelType;

                    string errTitle = result.FailCount > 0 ? "Fileloi" : "";
                    var fileName = string.Format("UploadTrangThaiPhieu_{1}_{0}.xlsx", currDate, errTitle);

                    return Ok(new ImportResponseModel
                    {
                        Bytes = result.Data,
                        FileType = fileType,
                        FileName = fileName,
                        SuccessCount = result.SuccessCount,
                        FailCount = result.FailCount
                    });
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện tải biểu mẫu upload trạng thái phiếu.", ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi hệ thống khi thực hiện tải biểu mẫu upload trạng thái phiếu."
                });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listInventoryDocumentFull)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListInventoryDocumentFull()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var cursor = Request.Form["Cursor"].FirstOrDefault();


            var listInventoryDocumentFull = new ListInventoryDocumentFullDto();
            listInventoryDocumentFull.Plant = Request.Form["Plant"].FirstOrDefault();
            listInventoryDocumentFull.WHLoc = Request.Form["WHLoc"].FirstOrDefault();
            listInventoryDocumentFull.DocNumberFrom = Request.Form["DocNumberFrom"].FirstOrDefault();
            listInventoryDocumentFull.DocNumberTo = Request.Form["DocNumberTo"].FirstOrDefault();
            listInventoryDocumentFull.ModelCode = Request.Form["ModelCode"].FirstOrDefault();
            listInventoryDocumentFull.AssigneeAccount = Request.Form["AssigneeAccount"].FirstOrDefault();
            listInventoryDocumentFull.ComponentCode = Request.Form["ComponentCode"].FirstOrDefault();

            listInventoryDocumentFull.Departments = Request.Form["Departments[]"].ToList();
            listInventoryDocumentFull.Locations = Request.Form["Locations[]"].ToList();
            listInventoryDocumentFull.DocTypes = Request.Form["DocTypes[]"].ToList();
            listInventoryDocumentFull.InventoryNames = Request.Form["InventoryNames[]"].ToList();
            listInventoryDocumentFull.Statuses = Request.Form["Statuses[]"].ToList();

            listInventoryDocumentFull.Skip = skip;
            listInventoryDocumentFull.Take = pageSize;

            listInventoryDocumentFull.Cursor = string.IsNullOrEmpty(cursor) ? default : Convert.ToDateTime(cursor);

            if (!string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberFrom) && string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberTo) ||
                string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberTo))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }

            if (!string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberTo) && int.Parse(listInventoryDocumentFull.DocNumberFrom) > int.Parse(listInventoryDocumentFull.DocNumberTo))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            var result = await _inventoryService.GetInventoryDocumentFull(listInventoryDocumentFull);

            if (result?.Data != null)
            {
                int recordsTotal = 0;
                recordsTotal = result.Data.TotalRecords;

                var data = result.Data.Data;
                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data,
                    docsNotReceiveCount = result.Data.DocsNotReceiveCount,
                    cursor = result.Data.LastCursor
                };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0, docsNotReceiveCount = 0 };
            return Ok(jsonData2);

        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listInventoryDocumentFullToExport)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListInventoryDocumentFullToExport([FromBody] ListInventoryDocumentFullDto listInventoryDocumentFull)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }


            if (!string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberFrom) && string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberTo) ||
                string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberTo))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số phiếu.",
                });
            }

            if (!string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberTo) && int.Parse(listInventoryDocumentFull.DocNumberFrom) > int.Parse(listInventoryDocumentFull.DocNumberTo))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Số phiếu vừa nhập không hợp lệ.",
                });
            }

            listInventoryDocumentFull.IsGetAllForExport = true;
            var result = await _inventoryService.GetInventoryDocumentFull(listInventoryDocumentFull);

            return Ok(result);
        }
        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.inventoryDocDetail)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> InventoryDocDetail(string docId, string searchTerm = "")
        {
            try
            {
                if (string.IsNullOrEmpty(docId) || !Guid.TryParse(docId, out _))
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = Constants.ResponseMessages.InvalidId
                    });
                }

                var result = await _inventoryService.InventoryDocDetail(docId, searchTerm);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, errorMessage: ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }


        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.inventoryNames)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS, Constants.Permissions.VIEW_ALL_INVENTORY,
            Constants.Permissions.VIEW_CURRENT_INVENTORY, Constants.Permissions.EDIT_INVENTORY)]
        public async Task<IActionResult> InventoryNames()
        {
            try
            {
                var result = await _inventoryService.DropdownInventories();
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, errorMessage: ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpDelete]
        [Route(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.deleteInventorys)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DeleteInventorys([FromRoute] string inventoryId, [FromForm] List<string> docIds)
        {
            if (string.IsNullOrEmpty(inventoryId))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }
            if (!docIds.Any())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                });
            }
            var result = await _inventoryService.DeleteInventorys(inventoryId, docIds);
            return Ok(result);
        }

        [HttpGet]
        [Route(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.getDetailInventory)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> GetDetailInventory(string inventoryId, string docId)
        {
            if (string.IsNullOrEmpty(docId))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.Inventory.RequiredInputValue,
                });
            }
            if (string.IsNullOrEmpty(inventoryId))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.Inventory.RequiredInputValue,
                });
            }

            var result = await _inventoryService.GetDetailInventory(inventoryId, docId);

            return Ok(result);
        }
        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.getDocumentTypeC)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> GetDocumentTypeC()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var inventoryId = Request.Form["InventoryId"].FirstOrDefault();
            var docId = Request.Form["DocId"].FirstOrDefault();
            var componentCode = Request.Form["ComponentCode"].FirstOrDefault();



            var result = await _inventoryService.GetDocumentTypeC(inventoryId, docId, componentCode);

            if (result?.Data != null)
            {
                var allRecords = result?.Data;
                int recordsTotal = 0;
                recordsTotal = result.Data.Count();

                var data = result.Data.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0 };
            return Ok(jsonData2);
        }


        /// <summary>
        /// Danh sách phiếu màn hình tổng hợp kết quả
        /// </summary>
        /// <returns></returns>

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.documentResults)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DocumentResults()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var filterModel = new DocumentResultListFilterModel();
            filterModel.Plant = Request.Form["Plant"].FirstOrDefault();
            filterModel.WHLoc = Request.Form["WHLoc"].FirstOrDefault();
            filterModel.DocNumberFrom = Request.Form["DocNumberFrom"].FirstOrDefault();
            filterModel.DocNumberTo = Request.Form["DocNumberTo"].FirstOrDefault();
            filterModel.ModelCode = Request.Form["ModelCode"].FirstOrDefault();
            filterModel.ComponentCode = Request.Form["ComponentCode"].FirstOrDefault();
            var inventoryId = Request.Form["InventoryId"].FirstOrDefault();
            filterModel.InventoryId = Guid.Parse(inventoryId);

            filterModel.IsCheckAllDocType = Request.Form["IsCheckAllDocType"].FirstOrDefault();
            filterModel.DocTypes = Request.Form["DocTypes[]"].ToList();
            filterModel.Skip = skip;
            filterModel.Take = pageSize;

            filterModel.OrderColumn = sortColumn;
            filterModel.OrderColumnDirection = sortColumnDirection;

            var result = await _inventoryService.DocumentResults(filterModel);
            if (result?.Data?.Data != null)
            {
                var items = result.Data.Data;
                var totalCount = result.Data.TotalRecords;

                var response = new { draw = draw, recordsFiltered = totalCount, recordsTotal = totalCount, data = items };
                return Ok(response);
            }

            var emptyList = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = Enumerable.Empty<DocumentResultViewModel>() };
            return Ok(emptyList);

        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.exportDocumentResultExcel)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportDocumentResultExcel([FromBody] DocumentResultListFilterModel filterModel)
        {
            try
            {
                if (filterModel == null)
                {
                    return BadRequest(new ImportResponseModel
                    {
                        Message = commonAPIConstant.ResponseMessages.InValidValidationMessage
                    });
                }

                if (!Guid.TryParse(filterModel.InventoryId.ToString(), out var inventoryGuid))
                {
                    return BadRequest(new ImportResponseModel
                    {
                        Message = commonAPIConstant.ResponseMessages.InvalidId
                    });
                }

                filterModel.IsAllForExport = true;
                var result = await _inventoryService.ExportDocumentResultExcel(filterModel);
                if (result.Code == StatusCodes.Status200OK)
                {
                    var bytes = result.Data;
                    var fileType = Constants.FileResponse.ExcelType;
                    var currDate = DateTime.Now;
                    var fileName = string.Format("Tonghopketqua_{0}", currDate.ToString(Constants.DatetimeFormat));

                    return Ok(new ImportResponseModel
                    {
                        Bytes = result.Data,
                        FileName = fileName,
                        FileType = fileType,
                    });
                }

                return BadRequest(result);

            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi xuất danh sách tổng hợp kết quả.", ex.Message);
                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        /// <summary>
        /// Xuất File txt màn hình tổng hợp kết quả 
        /// </summary>
        /// <returns></returns>

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.documentResultsToExport)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DocumentResultsToExport([FromBody] DocumentResultListFilterModel filterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages()
                });
            }

            var result = await _inventoryService.DocumentResultsToExport(filterModel);
            return Ok(result);

        }

        /// <summary>
        /// Import File SAP:
        /// </summary>
        /// <returns></returns>

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.documentResultsImportSAP)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DocumentResultsImportSAP([FromForm] IFormFile file, [FromForm] string userId, [FromRoute] string inventoryId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = commonAPIConstant.ResponseMessages.InvalidId,
                    });
                }
                if (string.IsNullOrEmpty(inventoryId))
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = commonAPIConstant.ResponseMessages.InvalidId,
                    });
                }

                var result = await _documentResultService.ImportFileSAP(file, inventoryId, userId);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi import file SAP.");
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listDocumentHistories)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListDocumentHistories()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var listDocumentHistory = new ListDocumentHistoryDto();
            listDocumentHistory.ComponentCode = Request.Form["ComponentCode"].FirstOrDefault();
            listDocumentHistory.DocCode = Request.Form["DocCode"].FirstOrDefault();
            listDocumentHistory.ModelCode = Request.Form["ModelCode"].FirstOrDefault();
            listDocumentHistory.AssigneeAccount = Request.Form["AssigneeAccount"].FirstOrDefault();

            listDocumentHistory.IsCheckAllDepartment = Request.Form["IsCheckAllDepartment"].FirstOrDefault();
            listDocumentHistory.IsCheckAllLocation = Request.Form["IsCheckAllLocation"].FirstOrDefault();
            listDocumentHistory.IsCheckAllDocType = Request.Form["IsCheckAllDocType"].FirstOrDefault();
            listDocumentHistory.IsCheckAllInventoryName = Request.Form["IsCheckAllInventoryName"].FirstOrDefault();

            listDocumentHistory.Departments = Request.Form["Departments[]"].ToList();
            listDocumentHistory.Locations = Request.Form["Locations[]"].ToList();
            listDocumentHistory.DocTypes = Request.Form["DocTypes[]"].ToList();
            listDocumentHistory.InventoryNames = Request.Form["InventoryNames[]"].ToList();

            listDocumentHistory.Skip = skip;
            listDocumentHistory.Take = pageSize;

            var result = await _documentResultService.ListDocumentHistories(listDocumentHistory);

            if (result?.Data != null)
            {
                int recordsTotal = 0;
                recordsTotal = result.TotalRecords;

                var data = result.Data;
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0 };
            return Ok(jsonData2);

        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listDocumentHistoriesToExport)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListDocumentHistoriesToExport([FromBody] ListDocumentHistoryDto listDocumentHistory)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = ModelState.ErrorTextMessages()
                    });
                }
                listDocumentHistory.IsExport = true;
                var result = await _documentResultService.ListDocumentHistories(listDocumentHistory);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi danh sách lịch sử kiểm kê.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }
        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.importResultFromBwins)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ImportResultFromBwins([FromRoute] string inventoryId, [FromForm] IFormFile file)
        {
            try
            {
                if (!Guid.TryParse(inventoryId, out var convertedInventoryGuid))
                {
                    return BadRequest(new ImportResponseModel
                    {
                        Message = commonAPIConstant.ResponseMessages.InValidValidationMessage
                    });
                }

                var currentUserId = HttpContext.CurrentUserId();

                var result = await _documentResultService.UploadTotalFromBwins(convertedInventoryGuid, Guid.Parse(currentUserId), file);
                if (result.Code == StatusCodes.Status200OK)
                {
                    var bytes = result.Bytes;
                    var fileType = Constants.FileResponse.ExcelType;
                    var fileName = string.Format("UploadBwins_Fileloi_{0}", DateTime.Now.ToString(Constants.DatetimeFormat));
                    return Ok(new ImportResponseModel
                    {
                        Bytes = result.Data,
                        SuccessCount = result.SuccessCount,
                        FailCount = result.FailCount,
                        FileName = fileName,
                        FileType = fileType,
                    });
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi nhập kết quả từ Bwins");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }
        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.exportTreeGroups)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportTreeGroups(Guid inventoryId, [FromForm] string machineModel, [FromForm] string machineType = null)
        {
            var result = await _inventoryService.ExportTreeGroups(inventoryId, machineModel, machineType);
            if (result.Code == StatusCodes.Status200OK)
            {
                byte[] data = (byte[])result.Data;
                return File(data, Constants.FileResponse.ExcelType, "CumCay.xlsx");
            }
            return BadRequest(result);
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.getTreeGroupFilters)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> GetTreeGroupFilters()
        {
            var result = await _inventoryService.GetTreeGroupFilters();

            return Ok(result);
        }


        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.docDetailComponentsC)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DocDetailComponentsC()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var searchValue = Request.Form["search"].FirstOrDefault();
            var documentId = Request.Form["documentId"].FirstOrDefault();
            Guid.TryParse(documentId, out Guid documentIdGuid);

            var result = await _inventoryService.DocCComponents(documentIdGuid, skip, pageSize, searchValue);
            if (result?.Data?.Data != null)
            {
                var response = new { draw = draw, recordsFiltered = result.Data.TotalRecords, recordsTotal = result.Data.TotalRecords, data = result.Data.Data };
                return Ok(response);
            }

            var emptyResponse = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0 };
            return Ok(emptyResponse);

        }

        [HttpPut(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.receiveAllDocs)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        [Authorize(Constants.Permissions.EDIT_INVENTORY)]
        public async Task<IActionResult> ReceiveAllDocs([FromBody] List<Guid> excludeIds)
        {
            try
            {
                var result = await _inventoryService.UpdateAllReceiveDoc(excludeIds);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện cập nhật trạng thái tiếp nhận phiếu.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.checkAssignedDocs)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        [Authorize(Constants.Permissions.EDIT_INVENTORY)]
        public async Task<IActionResult> CheckAssignedDocs()
        {
            try
            {
                var result = await _inventoryService.CheckAnyDocAssigned();
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện kiểm tra phiếu được phân phát.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.checkDownloadDocTemplate)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> CheckDownloadDocTemplate()
        {
            try
            {
                var result = await _inventoryService.CheckDownloadDocTemplate();
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện tải biểu mẫu.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.checkExistDocTypeA)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        [Authorize(Constants.Permissions.EDIT_INVENTORY)]
        public async Task<IActionResult> CheckExistDocTypeA(string inventoryId)
        {
            try
            {
                var result = await _inventoryService.CheckExistDocTypeA(inventoryId);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện kiểm tra tồn tại phiếu A.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }


        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.aggregateDocResults)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> AggregateDocResults(Guid inventoryId)
        {
            try
            {
                var result = await _inventoryService.AggregateDocResults(inventoryId, HttpContext.CurrentUserId());
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện tổng hợp kết quả.");
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpDelete(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.deleteInventoryDocs)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DeleteInventoryDocs(Guid inventoryId, [FromBody] ListInventoryDocumentDeleteDto listInventoryDocumentDeleteDto, bool deleteAll = false)
        {
            try
            {
                var result = await _inventoryDocService.DeleteInventoryDocs(inventoryId, listInventoryDocumentDeleteDto, deleteAll);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi hệ thống khi thực hiện xóa các phiếu kiểm kê");
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPost("{inventoryName}/import/update-quantity")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ImportUpdateQuantity([FromForm] IFormFile file, string inventoryName)
        {
            try
            {
                var result = await _inventoryService.ImportUpdateQuantity(file, inventoryName);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi import file cập nhật số lượng.", ex.Message);
                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Có lỗi khi import file cập nhật số lượng."
                });
            }
        }
        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listDocTypeCToExportQRCode)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListDocTypeCToExportQRCode([FromBody] ListDocTypeCToExportQRCodeDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = ModelState.ErrorTextMessages()
                    });
                }
                var result = await _documentResultService.ListDocTypeCToExportQRCode(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi danh sách phiếu C để export qrcode.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.getTreeGroupQRCodeFilters)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> GetTreeGroupQRCodeFilters()
        {
            try
            {
                var result = await _inventoryService.GetTreeGroupQRCodeFilters();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện dropdown in cụm QRCode");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }

        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.getTreeGroupInventoryErrorFilters)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> GetTreeGroupInventoryErrorFilters()
        {
            try
            {
                var result = await _inventoryService.GetTreeGroupInventoryErrorFilters();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện dropdown xuất sai số");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }

        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.listDocumentToInventoryError)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListDocumentToInventoryError([FromBody] ListDocToExportInventoryErrorDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = ModelState.ErrorTextMessages()
                    });
                }
                var result = await _documentResultService.ListDocumentToInventoryError(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi export file xuất sai số.");
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryWeb.importMSLDataUpdate)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ImportMSLDataUpdate([FromForm] IFormFile file, Guid inventoryId)
        {
            try
            {
                var result = await _documentResultService.ImportMSLDataUpdate(file, inventoryId);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi import file cập nhật dữ liệu MSL.");
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ImportResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

    }
}
