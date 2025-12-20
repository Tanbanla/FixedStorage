using BIVN.FixedStorage.Services.Common.API.Enum;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class DocumentDetailFilterModel
    {
        public Guid InventoryId {get;set;}
        public Guid AccountId {get;set;}
        public Guid DocumentId { get; set; }
        public string SearchTerm { get; set; }
        public int Page { get; set; }
        public InventoryActionType? ActionType { get; set; }
    }
}
