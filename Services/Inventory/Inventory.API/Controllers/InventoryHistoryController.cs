namespace Inventory.API.Controllers
{
    [Route(commonAPIConstant.Endpoint.InventoryService.InventoryHistory.root)]
    [ApiController]
    public class InventoryHistoryController : ControllerBase
    {
        private readonly ILogger<InventoryHistoryController> _logger;
        private readonly IInventoryHistoryService _inventoryHistoryService;

        public InventoryHistoryController(ILogger<InventoryHistoryController> logger,
                                            IInventoryHistoryService inventoryHistoryService
                                          )
        {
            _logger = logger;
            _inventoryHistoryService = inventoryHistoryService;
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.InventoryHistory.historyDetail)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> HistoryDetail()
        {
            var getDraw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();

            var searchValue = Request.Form["searchTerm"].FirstOrDefault();
            int take = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int draw = getDraw != null ? Convert.ToInt32(getDraw) : 0;

            var inventoryId = Request.Form["inventoryId"].FirstOrDefault();
            var historyId = Request.Form["historyId"].FirstOrDefault();

            InventoryHistoryDetailFilterModel filterModel = new();
            filterModel.InventoryId = inventoryId;
            filterModel.HistoryId = historyId;
            filterModel.Skip = skip;
            filterModel.Take = take;
            filterModel.Draw = draw;
            filterModel.SearchTerm = string.IsNullOrEmpty(searchValue) ? string.Empty : searchValue;

            var result = await _inventoryHistoryService.Detail(filterModel);

            return Ok(result);
        }
    }
}
