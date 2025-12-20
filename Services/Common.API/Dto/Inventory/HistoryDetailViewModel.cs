namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class HistoryDetailViewModel
    {
        //public int DocumentType { get; set; }
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid InventoryDocId { get; set; }
        public int? DocType { get; set; }
        public string? DocName { get; set; }
        public string ConfirmBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public int Status { get; set; }
        public string Comment { get; set; }
        public string EvicenceImg { get; set; }
        public string EvicenceImgTitle { get; set; }
        public string Note { get; set; }
        public int Action { get; set; }
        public string InventoryBy { get; set; }
        public ChangeLogModel ChangeLogModel { get; set; } = new();
        public IEnumerable<DocComponentABE> DocOutputs { get; set; } = Enumerable.Empty<DocComponentABE>();
        public IEnumerable<HistoryDetailABE> HistoryDetailABEs { get; set; } = Enumerable.Empty<HistoryDetailABE>();
        public IEnumerable<HistoryOutputModel> HistoryOutputs { get; set; } = Enumerable.Empty<HistoryOutputModel>();
        public IEnumerable<HistoryDetailC> HistoryDetailCs { get; set; } = Enumerable.Empty<HistoryDetailC>();
        public int DocCTotalPages { get; set; } = 0;

        public string MachineModel { get; set; }
        public string MachineType { get; set; }
        public string LineName { get; set; }
        public string StageName { get; set; }

        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
    }

    public class HistoryDetailABE
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid HistoryId { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
    }

    public class HistoryDetailC
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid HistoryId { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
        public bool IsHighLight { get; set; }
    }
}
