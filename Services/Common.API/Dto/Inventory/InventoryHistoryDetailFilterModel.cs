namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class InventoryHistoryDetailFilterModel
    {
        public string InventoryId { get; set; }
        public string HistoryId { get; set; }
        public string SearchTerm { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int Draw { get; set; }
    }
}
