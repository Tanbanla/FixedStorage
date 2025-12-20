using BIVN.FixedStorage.Services.Common.API.Helpers;

namespace Inventory.API.Service
{
    public class InventoryHistoryService : IInventoryHistoryService
    {
        private readonly ILogger<InventoryHistoryService> _logger;
        private readonly HttpContext _httpContext;
        private readonly RestClientFactory _restClientFactory;
        private readonly IDataAggregationService _dataAggregationService;
        private readonly IConfiguration _configuration;
        private readonly InventoryContext _inventoryContext;

        public InventoryHistoryService(ILogger<InventoryHistoryService> logger,
                                        InventoryContext inventoryContext,
                                        IHttpContextAccessor httpContextAccessor,
                                        RestClientFactory restClientFactory,
                                        IDataAggregationService dataAggregationService,
                                        IConfiguration configuration
                                    )
        {
            _logger = logger;
            _inventoryContext = inventoryContext;
            _httpContext = httpContextAccessor.HttpContext;
            _restClientFactory = restClientFactory;
            _dataAggregationService = dataAggregationService;
            _configuration = configuration;
        }

        public async Task<ResponseModel<InventoryHistoryDetailViewModel>> Detail(InventoryHistoryDetailFilterModel filterModel)
        {
            var accounts = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                                                   .Select(x => new
                                                                   {
                                                                       x.UserId,
                                                                       x.UserName
                                                                   })
                                                                   .ToDictionaryAsync(x => x.UserId.ToString().ToLower(), x => x);

            var history = await _inventoryContext.DocHistories.Include(x => x.InventoryDoc)
                                                        .ThenInclude(x => x.Inventory)
                                                        .FirstOrDefaultAsync(x => x.InventoryId == Guid.Parse(filterModel.InventoryId)
                                                                                && x.Id == Guid.Parse(filterModel.HistoryId));

            if(history == null)
            {
                return new ResponseModel<InventoryHistoryDetailViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            InventoryHistoryDetailViewModel detailModel = new();
            detailModel.HistoryId = history.Id;
            detailModel.DocumentId = history.InventoryDoc.Id;
            detailModel.InventoryId = history.InventoryId;
            detailModel.InventoryName = history.InventoryDoc.Inventory.Name;
            detailModel.DepartmentName = history.InventoryDoc.DepartmentName;
            detailModel.LocationName = history.InventoryDoc.LocationName;
            detailModel.DocCode = history.InventoryDoc.DocCode;
            detailModel.ComponentCode = history.InventoryDoc.ComponentCode;
            detailModel.ModelCode = history.InventoryDoc.ModelCode;
            detailModel.ComponentName = history.InventoryDoc.DocType == InventoryDocType.C ? history.InventoryDoc.StageName : history.InventoryDoc.ComponentName;
            detailModel.ActionTitle = EnumHelper<DocHistoryActionType>.GetDisplayValue(history.Action);
            detailModel.Note = history.Comment;
            detailModel.CreateBy = history.CreatedBy;
            detailModel.CreateAt = history.CreatedAt.ToString(Constants.DefaultDateFormat);
            detailModel.DocType = (int)history.InventoryDoc.DocType;
            detailModel.ChangeLogText = BIVN.FixedStorage.Services.Inventory.API.Utilities.DisplayChangeLog(history.OldQuantity, history.NewQuantity, (int)history.OldStatus, (int)history.NewStatus, history.IsChangeCDetail);

            if (!string.IsNullOrEmpty(history?.EvicenceImg))
            {
                detailModel.EnvicenceImage =  history.EvicenceImg;
                detailModel.EnvicenceImageTitle = Path.GetFileName(history.EvicenceImg);
            }

            //DocOutputs
            detailModel.HistoryOutputs = _inventoryContext.HistoryOutputs.AsNoTracking().Where(x => x.DocHistoryId == history.Id)
                                                                         .Select(x => new HistoryDetailABE
                                                                         {
                                                                             Id = x.Id,
                                                                             HistoryId = history.Id,
                                                                             InventoryId = x.InventoryId,
                                                                             QuantityOfBom = x.QuantityOfBom,
                                                                             QuantityPerBom = x.QuantityPerBom
                                                                         });

            if(history.InventoryDoc.DocType == InventoryDocType.C)
            {
                var historyCs = _inventoryContext.HistoryTypeCDetails.AsNoTracking().Where(x => x.HistoryId.Value == history.Id)
                                                                         .OrderBy(x => x.CreatedAt)
                                                                         .Select(x => new HistoryDetailC
                                                                         {
                                                                             Id = x.Id,
                                                                             HistoryId = history.Id,
                                                                             InventoryId = x.InventoryId,
                                                                             QuantityOfBom = x.QuantityOfBom,
                                                                             QuantityPerBom = x.QuantityPerBom,
                                                                             ComponentCode = x.ComponentCode,
                                                                             ModelCode = x.ModelCode                                                                                
                                                                         });

                Func<HistoryDetailC, bool> condition = (x) =>
                {
                    bool validComponentCode = true;

                    //Nếu có từ khóa tìm kiếm
                    if (!string.IsNullOrEmpty(filterModel.SearchTerm))
                    {
                        validComponentCode = !string.IsNullOrEmpty(x.ComponentCode) && x.ComponentCode.Contains(filterModel.SearchTerm.Trim()) ||
                                             !string.IsNullOrEmpty(x.ModelCode) && x.ModelCode.Contains(filterModel.SearchTerm.Trim());
                    }
                    return validComponentCode;
                };

                detailModel.ComponentCDetail.RecordsFiltered = historyCs.Where(condition).Count();
                detailModel.ComponentCDetail.RecordsTotal = detailModel.ComponentCDetail.RecordsFiltered;
                detailModel.ComponentCDetail.Data = historyCs.Where(condition).Skip(filterModel.Skip).Take(filterModel.Take).ToList();
                detailModel.ComponentCDetail.Draw = filterModel.Draw;
            }

            return new ResponseModel<InventoryHistoryDetailViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = detailModel,
                Message = "Chi tiết lịch sử phiếu."
            };
        }
    }
}
