namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListInventoryDocumentFullDto
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

        [MaxLength(20, ErrorMessage = "Tài khoản phân phát tối đa 5 ký tự.")]
        public string AssigneeAccount { get; set; }

        [MaxLength(12, ErrorMessage = "Mã linh kiện tối đa 12 ký tự.")]
        public string ComponentCode { get; set; }

        public List<string> InventoryNames { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public List<string> Locations { get; set; } = new();
        public List<string> DocTypes { get; set; } = new();
        public List<string> Statuses { get; set; } = new();

        public string IsCheckAllDepartment { get; set; }
        public string IsCheckAllLocation { get; set; }
        public string IsCheckAllDocType { get; set; }
        public string IsCheckAllInventoryName { get; set; }
        public string IsCheckAllStatus { get; set; }


        public int Skip { get; set; }
        public int Take { get; set; }

        public bool IsGetAllForExport { get; set; } = false;

        public DateTime Cursor { get; set; }
    }
}
