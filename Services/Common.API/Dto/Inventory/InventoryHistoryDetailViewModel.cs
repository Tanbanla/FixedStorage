namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class InventoryHistoryDetailViewModel
    {
        public Guid HistoryId { get; set; }
        public Guid DocumentId { get; set; }
        public Guid InventoryId { get; set; }
        public string InventoryName { get; set; }
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }
        public string DocCode { get; set; }
        public string ComponentCode { get; set; }
        public string ModelCode { get; set; }
        public string ComponentName { get; set; }
        public string ActionTitle { get; set; }
        public string Note { get; set; }
        public string ChangeLogText { get; set; }
        public string CreateBy { get; set; }
        public string CreateAt { get; set; }

        public int DocType { get; set; }
        public string EnvicenceImage { get; set; }
        public string EnvicenceImageTitle { get; set; }

        public IEnumerable<HistoryDetailABE> HistoryOutputs { get; set; } = Enumerable.Empty<HistoryDetailABE>();

        public ComponentCDetail ComponentCDetail { get; set; } = new();
    }

    public class ComponentCDetail
    {
        public IEnumerable<HistoryDetailC> Data { get; set; } = Enumerable.Empty<HistoryDetailC>();
        public int RecordsFiltered { get; set; }
        public int TotalCount { get; set; }
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
    }
}
