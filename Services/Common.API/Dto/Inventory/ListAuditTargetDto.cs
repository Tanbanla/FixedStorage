namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListAuditTargetDto
    {
        public string ComponentCode { get; set; }
        public string SaleOrderNo { get; set; }
        public string Position { get; set; }
        public string AssigneeAccount { get; set; }
        public List<string> Statuses { get; set; }
        public List<string> Departments { get; set; }
        public List<string> Locations { get; set; }

        public bool IsExport { get; set; } = false;
        public int Skip { get; set; }
        public int Take { get; set; }

    }
}
