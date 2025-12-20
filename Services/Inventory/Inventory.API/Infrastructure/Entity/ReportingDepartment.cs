namespace Inventory.API.Infrastructure.Entity
{
    public class ReportingDepartment : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        public string DepartmentName { get; set; }
        public int TotalDoc { get; set; }
        public int TotalTodo { get; set; }
        public int TotalInventory { get; set; }
        public int TotalConfirm { get; set; }
        public CaptureTimeType CaptureTimeType { get; set; }
    }
}
