namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListAuditTargetViewModel
    {
        public Guid InventoryId { get; set; }
        public Guid AuditTargetId { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string Location { get; set; }
        public string ComponentCode { get; set; }
        public string SaleOrderNo { get; set; }
        public string Position { get; set; }
        public string ComponentName { get; set; }
        public int Status { get; set; }
        public string AssigneeAccount { get; set; }
        public string DepartmentName { get; set; }
    }
}
