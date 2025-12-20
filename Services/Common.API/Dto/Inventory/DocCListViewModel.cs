namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class DocCListViewModel
    {
        public List<DocCInfoModel> DocCInfoModels { get; set; } = new();
        //Trạng thái hoàn thành tổng thể vế trái => Liệt kê số linh kiện đã thao tác
        public int FinishCount { get; set; }
        //Trạng thái hoàn thành tổng thể vế phải => Liệt kê tổng số linh kiện
        public int TotalCount { get; set; }
    }

    public class DocCInfoModel
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid AccountId { get; set; }
        public int Status { get; set; }
        public int DocType { get; set; }
        public string DocCode { get; set; }
        public string ModelCode { get; set; }
        public string MachineModel { get; set; }
        public string MachineType { get; set; }
        public string LineName { get; set; }
        public string LineType { get; set; }
        public string StageNumber { get; set; }
        public string StageName { get; set; }

        public string InventoryBy { get; set; }
        public string AuditedBy { get; set; }
        public string ConfirmedBy { get; set; }
        public string Note { get; set; }
        public int DocStatusOrder { get; set; }
    }

    public class DocBListViewModel
    {
        public List<DocBInfoModel> DocBInfoModels { get; set; } = new();
        //Trạng thái hoàn thành tổng thể vế trái => Liệt kê số linh kiện đã thao tác
        public int FinishCount { get; set; }
        //Trạng thái hoàn thành tổng thể vế phải => Liệt kê tổng số linh kiện
        public int TotalCount { get; set; }
    }

    public class DocBInfoModel : DocCInfoModel 
    {
        public string ComponentCode { get; set; }
        public string PositionCode { get; set; }
    }

    public class DocAEListViewModel
    {
        public List<DocAEInfoModel> DocAEInfoModels { get; set; } = new();
        //Trạng thái hoàn thành tổng thể vế trái => Liệt kê số linh kiện đã thao tác
        public int FinishCount { get; set; }
        //Trạng thái hoàn thành tổng thể vế phải => Liệt kê tổng số linh kiện
        public int TotalCount { get; set; }
    }

    public class DocAEInfoModel : DocCInfoModel
    {
        public string ComponentCode { get; set; }
        public string PositionCode { get; set; }
    }

}
