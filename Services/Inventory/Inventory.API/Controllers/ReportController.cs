using BIVN.FixedStorage.Services.Common.API.Dto.Report;

namespace Inventory.API.Controllers
{
    [Route("api/inventory/report")]
    [ApiExplorerSettings(GroupName = "Report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IReportService _reportService;

        public ReportController(ILogger<ReportController> logger,
                                  IReportService reportService
                                )
        {
            _logger = logger;
            _reportService = reportService;
        }

        [HttpPost("progress")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ProgressReport([FromBody] ProgressReportDto progressReport)
        {
            try
            {
                var result = await _reportService.ProgressReport(progressReport);
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


        [HttpPost("audit")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> AuditReport([FromBody] ProgressReportDto progressReport)
        {
            //Validate ##########

            try
            {
                var result = await _reportService.AggregateAuditReport(progressReport);
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

        [HttpPost("audits")]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> AuditReports([FromBody] AuditReportModel auditReportModel)
        {
            //Validate ##########

            try
            {
                var result = await _reportService.AuditReports(auditReportModel);
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


    }
}
