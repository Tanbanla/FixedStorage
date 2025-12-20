using BIVN.FixedStorage.Services.Common.API.Enum;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListDocCFilterModel : IPagination
    {
        public Guid InventoryId { get; set; }
        public Guid AccountId { get; set; }
        public string MachineModel { get; set; }
        public string MachineType { get; set; }
        public string LineName { get; set; }
        public InventoryActionType? ActionType { get; set; }
        public string StageName { get; set; }
        public string ModelCode { get; set; }
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ListDocBFilterModel : ListDocCFilterModel, IPagination
    {
        public string ModelCode { get; set; }
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ListDocAEFilterModel : IPagination
    {
        public Guid InventoryId { get; set; }
        public Guid AccountId { get; set; }
        public InventoryActionType? ActionType { get; set; }
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ScanDocBFilterModel
    {
        public string InventoryId { get; set; }
        public string AccountId { get; set; }
        public string ComponentCode { get; set; }
        public string MachineModel { get; set; }
        public string MachineType { get; set; }
        public string LineName { get; set; }
        public string ModelCode { get; set; }
        public InventoryActionType? ActionType { get; set; }

    }

}
