namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class DocumentDetailModel
    {
        public string DocCode { get; set; }
        public int Status { get; set; }
        public string InventoryBy { get; set; }
        public DateTime? InventoryAt { get; set; }
        public string SalesOrder { get; set; }
        public string Note { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string PositionCode { get; set; }
        public int? DocType { get; set; }

        //Thông tin riêng phần phiếu C
        public string ModelCode { get; set; }
        public string MachineModel { get; set; }
        public string MachineType { get; set; }
        public string LineName { get; set; }
        public string StageNumber { get; set; }
        public string StageName { get; set; }
        public string ConfirmedBy { get; set; }

        public IEnumerable<DocHistoriesModel> DocHistories { get; set; } = Enumerable.Empty<DocHistoriesModel>();
        public IEnumerable<DocComponentABE> DocComponentABEs { get; set; } = Enumerable.Empty<DocComponentABE>();
        public IEnumerable<DocComponentC> DocComponentCs { get; set; } = Enumerable.Empty<DocComponentC>();

        public int DocCTotalPages { get; set; } = 0;

    }

    public class DocHistoriesModel
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public string? Comment { get; set; }
        public int Action { get; set; }
        public string? EvicenceImg { get; set; }
        public string? EvicenceImgTitle { get; set; }
        public Guid InventoryDocId { get; set; }
        public ChangeLogModel ChangeLogModel { get; set; }
        public DateTime CreatedAt { get; set; }
        //Bảng DocHistories thì cột createby là lưu mã nhân viên
        public string CreatedBy { get; set; }
        public int Status { get; set; }
    }

    //View model của linh kiện trong phiếu ABE => Số thùng, số lượng trên một thùng
    public class DocComponentABE
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid InventoryDocId { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
    }

    //View model của linh kiện trong phiếu C => Số thùng, số lượng trên một thùng
    public class DocComponentC
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid InventoryDocId { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public bool IsHighLight { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
        public int? No { get; set; }
    }
}
