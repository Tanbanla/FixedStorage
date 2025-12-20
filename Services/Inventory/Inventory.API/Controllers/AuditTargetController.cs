namespace Inventory.API.Controllers
{
    [Route("api/inventory")]
    [ApiController]
    public class AuditTargetController : ControllerBase
    {
        private readonly ILogger<AuditTargetController> _logger;
        private readonly IAuditTargetWebService _auditTargetWebService;
        private readonly IInventoryService _inventoryService;

        public AuditTargetController(ILogger<AuditTargetController> logger,
                                    IAuditTargetWebService auditTargetWebService,
                                    IInventoryService inventoryService
                                    )
        {
            _logger = logger;
            _auditTargetWebService = auditTargetWebService;
            _inventoryService = inventoryService;
        }

        [HttpGet("{inventoryId}/audit-target/{auditTargetId}")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> AuditTargetDetail(Guid inventoryId, Guid auditTargetId)
        {
            var userId = HttpContext.CurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.TryAddModelError(nameof(userId), commonAPIConstant.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(inventoryId.ToString(), out _))
            {
                ModelState.TryAddModelError(nameof(inventoryId), commonAPIConstant.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(auditTargetId.ToString(), out _))
            {
                ModelState.TryAddModelError(nameof(auditTargetId), commonAPIConstant.ResponseMessages.InvalidId);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = BIVN.FixedStorage.Services.Inventory.API.Utilities.ErrorMessages(ModelState),
                    Message = commonAPIConstant.ResponseMessages.InValidValidationMessage
                });
            }

            try
            {
                var result = await _auditTargetWebService.GetAuditTargetDetail(inventoryId, auditTargetId);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }


        [HttpPut("{inventoryId}/audit-target/{auditTargetId}")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> UpdateAuditTarget(Guid inventoryId, Guid auditTargetId, [FromBody] UpdateAuditTargetDto updateAuditTargetDto)
        {
            if (!Guid.TryParse(inventoryId.ToString(), out _))
            {
                ModelState.TryAddModelError(nameof(inventoryId), Constants.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(auditTargetId.ToString(), out _))
            {
                ModelState.TryAddModelError(nameof(auditTargetId), Constants.ResponseMessages.InvalidId);
            }
            if(updateAuditTargetDto == null) {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }
            
            try
            {
                var result = await _auditTargetWebService.UpdateAuditTarget(inventoryId, auditTargetId, updateAuditTargetDto);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                var convertDict = result?.Data?.ToDictionary(x => x.Key.ToCamelCase(), x => x.Value) ?? new();
                result.Data = convertDict;

                return BadRequest(result);
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

        [HttpPost]
        [Route("{inventoryId}/audit-target/export")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListAuditTargetToExport([FromBody] ListAuditTargetDto listAuditTargetDto, [FromRoute]Guid inventoryId)
        {
            //if (string.IsNullOrEmpty(inventoryId))
            //{
            //    return BadRequest(new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = commonAPIConstant.ResponseMessages.InValidValidationMessage,
            //    });
            //}
            listAuditTargetDto.IsExport = true;
            var result = await _auditTargetWebService.ListAuditTarget(listAuditTargetDto, inventoryId);
            return Ok(result);
        }

        [HttpPost]
        [Route("{inventoryId}/audit-target")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ListAuditTarget([FromRoute]Guid inventoryId)
        {
            //if (string.IsNullOrEmpty(inventoryId))
            //{
            //    return BadRequest(new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = commonAPIConstant.ResponseMessages.InValidValidationMessage,
            //    });
            //}

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            
            var listAuditTargetDto = new ListAuditTargetDto();
            listAuditTargetDto.ComponentCode = Request.Form["ComponentCode"].FirstOrDefault();
            listAuditTargetDto.SaleOrderNo = Request.Form["SaleOrderNo"].FirstOrDefault();
            listAuditTargetDto.Position = Request.Form["Position"].FirstOrDefault();
            listAuditTargetDto.AssigneeAccount = Request.Form["AssigneeAccount"].FirstOrDefault();


            listAuditTargetDto.Statuses = Request.Form["Statuses[]"].ToList();
            listAuditTargetDto.Departments = Request.Form["Departments[]"].ToList();
            listAuditTargetDto.Locations = Request.Form["Locations[]"].ToList();

            listAuditTargetDto.Skip = skip;
            listAuditTargetDto.Take = pageSize;

            var result = await _auditTargetWebService.ListAuditTarget(listAuditTargetDto, inventoryId);

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

        [HttpPost("{inventoryId}/audit-target/import")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ImportCreateAuditTarget(Guid inventoryId, [FromForm]IFormFile importFile)
        {
            try
            {
                var result = await _inventoryService.ImportAuditTargetAsync(importFile, inventoryId);

                if(result.Code == StatusCodes.Status200OK)
                {
                    var currDate = DateTime.Now.ToString(Constants.DatetimeFormat);
                    var fileType = Constants.FileResponse.ExcelType;
                    var fileName = string.Format("Danhsachgiamsat_Fileloi_{0}.xlsx", currDate);

                    return Ok(new
                    {
                        Bytes = result.Data.Result,
                        FileType = fileType,
                        FileName = fileName,
                        SuccessCount = result.Data.SuccessCount,
                        FailCount = result.Data.FailCount,
                    });
                }

                //Nếu file sai định dạng
                if(result.Code == (int)HttpStatusCodes.InvalidFileExcel)
                {
                    return BadRequest(result);
                }

                return BadRequest(result);
            }
            catch(Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }

        [HttpDelete("{inventoryId}/audit-target")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DeleteAuditTargets(Guid inventoryId, [FromBody] List<Guid> IDs, bool deleteAll = false)
        {
            try
            {
                var result = await _inventoryService.DeleteAuditTargets(inventoryId, IDs, deleteAll);
                if(result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi hệ thống khi thực hiện xóa phiếu giám sát");
                _logger.LogHttpContext(HttpContext, ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = commonAPIConstant.ResponseMessages.InternalServer
                });
            }
        }
    }
}
