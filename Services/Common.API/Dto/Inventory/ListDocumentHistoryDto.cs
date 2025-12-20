namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListDocumentHistoryDto
    {
        [MaxLength(10, ErrorMessage = "Mã phiếu tối đa 10 ký tự.")]
        public string DocCode { get; set; }
        [MaxLength(9, ErrorMessage = "Mã phiếu tối đa 9 ký tự.")]
        public string ComponentCode { get; set; }
        [MaxLength(10, ErrorMessage = "Mã phiếu tối đa 10 ký tự.")]
        public string ModelCode { get; set; }
        [MaxLength(8, ErrorMessage = "Mã phiếu tối đa 8 ký tự.")]
        public string AssigneeAccount { get; set; }

        public List<string> InventoryNames { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public List<string> Locations { get; set; } = new();
        public List<string> DocTypes { get; set; } = new();

        public string IsCheckAllDepartment { get; set; }
        public string IsCheckAllLocation { get; set; }
        public string IsCheckAllDocType { get; set; }
        public string IsCheckAllInventoryName { get; set; }

        public bool IsExport { get; set; } = false;
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
