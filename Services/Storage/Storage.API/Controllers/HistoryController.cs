namespace Storage.API.Controllers
{
    [Route("api/storage/histories")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly ILogger<HistoryController> _logger;
        private readonly IHistoryService _historyService;

        public HistoryController(ILogger<HistoryController> logger,
                                IHistoryService historyService
                                )
        {
            _logger = logger;
            _historyService = historyService;
        }

        [HttpPost]
        [Authorize(Constants.Permissions.HISTORY_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetHistories()
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

            var userId = Request.Form["userId"].FirstOrDefault();
            var userName = Request.Form["userName"].FirstOrDefault();
            var componentCode = Request.Form["componentCode"].FirstOrDefault();
            var positionCode = Request.Form["positionCode"].FirstOrDefault();
            var quantityFrom = Request.Form["quantityFrom"].FirstOrDefault();
            var quantityTo = Request.Form["quantityTo"].FirstOrDefault();
            var dateFrom = Request.Form["dateFrom"].FirstOrDefault();
            var dateTo = Request.Form["dateTo"].FirstOrDefault();

            var types = Request.Form["types[]"].ToList();
            var factories = Request.Form["factories[]"].ToList();
            var layouts = Request.Form["layouts[]"].ToList();

            var isAllLayouts = Request.Form["layouts"].FirstOrDefault() == "-1" ? true : false;

            var isAllDepartments = Request.Form["departments"].FirstOrDefault() == "-1" ? true : false;
            var departments = Request.Form["departments[]"].ToList();

            HistoryFilterModel historyFilterModel = new();
            historyFilterModel.UserId = userId;
            historyFilterModel.UserName = userName.Trim();
            historyFilterModel.ComponentCode = componentCode.Trim();
            historyFilterModel.PositionCode = positionCode.Trim();
            historyFilterModel.QuantityFrom = !string.IsNullOrEmpty(quantityFrom) ? Convert.ToInt32(quantityFrom.Replace(",", "").Trim()) : -1;
            historyFilterModel.QuantityTo = !string.IsNullOrEmpty(quantityTo) ? Convert.ToInt32(quantityTo.Replace(",", "").Trim()) : -1;

            if (!string.IsNullOrEmpty(dateFrom))
            {
                if(DateTime.TryParseExact(dateFrom, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateFrom))
                {
                    historyFilterModel.dateFrom = parsedDateFrom;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                    historyFilterModel.dateFrom = null;
                }
            }

            if (!string.IsNullOrEmpty(dateTo))
            {
                if (DateTime.TryParseExact(dateTo, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTo))
                {
                    historyFilterModel.dateTo = parsedDateTo;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                    historyFilterModel.dateTo = null;
                }
            }

            historyFilterModel.Types = types;
            historyFilterModel.Layouts = layouts;
            historyFilterModel.Factories = factories;
            historyFilterModel.Departments = departments;
            historyFilterModel.isAllLayouts = isAllLayouts;

            historyFilterModel.Skip = skip;
            historyFilterModel.PageSize = pageSize;

            var result = await _historyService.GetHistories(historyFilterModel);

            if (result?.Data != null)
            {
                var recordsTotal = result.Data.TotalCount;
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = result.Data.Items };
                return Ok(jsonData);
            }

            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = Enumerable.Empty<HistoryResultModel>() };
            return Ok(jsonData2);
        }


        [HttpGet("{historyId}/{userId}")]
        [Authorize(Constants.Permissions.HISTORY_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> Detail(string historyId, string userId)
        {
            if(string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError(nameof(userId), "Id người dùng không hợp lệ");
            }
            if (string.IsNullOrEmpty(historyId) || !Guid.TryParse(historyId, out _))
            {
                ModelState.AddModelError(nameof(historyId), "Id lịch sử không hợp lệ");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Message = "Dữ liệu không hợp lệ",
                });
            }

            var result = await _historyService.GetHistoryDetail(userId, historyId);
            if(result.Code == 200)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("export-excel")]
        [Authorize(Constants.Permissions.HISTORY_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetHistoriesToExportExcel([FromBody] HistoryInOutExportDto historyFilterModel)
        {
            historyFilterModel.UserName = historyFilterModel.UserName != null ? historyFilterModel.UserName.Trim() : null;
            historyFilterModel.ComponentCode = historyFilterModel.ComponentCode != null ? historyFilterModel.ComponentCode.Trim() : null;
            historyFilterModel.PositionCode = historyFilterModel.PositionCode != null ? historyFilterModel.PositionCode.Trim() : null;

            var result = await _historyService.GetHistoriesToExportExcel(historyFilterModel);
            if (result?.Data?.Data != null)
            {
                //var recordsTotal = result.Data.TotalRecords;
                var jsonData = new {data = result.Data.Data };
                return Ok(jsonData);
            }

            var jsonData2 = new { data = Enumerable.Empty<HistoryInOutExportResultDto>() };
            return Ok(jsonData2);
        }

    }
}
