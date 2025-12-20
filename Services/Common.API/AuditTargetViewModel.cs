namespace BIVN.FixedStorage.Services.Common.API
{
    public class AuditTargetViewModel
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }
        public string PositionCode { get; set; }
        public string SaleOrderNo { get; set; }
        public string Plant { get; set; }
        public Guid AssignedAccountId { get; set; }
        public Guid InventoryId { get; set; }
        public string WHLOC { get; set; }
        public string AssigneeName { get; set; }
    }
}
