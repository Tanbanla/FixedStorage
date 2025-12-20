namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class DocumentResultListFilterModel
    {
        [MaxLength(4, ErrorMessage = "Plant tối đa 4 ký tự.")]
        public string Plant { get; set; }

        [MaxLength(4, ErrorMessage = "WHLoc tối đa 4 ký tự.")]
        public string WHLoc { get; set; }

        [MaxLength(5, ErrorMessage = "Số phiếu tối đa 5 ký tự.")]
        public string DocNumberFrom { get; set; }

        [MaxLength(5, ErrorMessage = "Số phiếu tối đa 5 ký tự.")]
        public string DocNumberTo { get; set; }

        [MaxLength(11, ErrorMessage = "Số phiếu tối đa 11 ký tự.")]
        public string ModelCode { get; set; }

        [MaxLength(10, ErrorMessage = "Mã linh kiện tối đa 10 ký tự.")]
        public string ComponentCode { get; set; }

        public List<string> DocTypes { get; set; } = new();
        public string IsCheckAllDocType { get; set; }

        public Guid InventoryId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

        public bool IsAllForExport { get; set; } = false;

        public string OrderColumn { get; set; }
        public string OrderColumnDirection { get; set; }
    }
}
