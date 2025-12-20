#nullable enable
namespace Inventory.API.Infrastructure.Entity
{
    public class ReportingAudit : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        public string? LocationtName { get; set; }
        public int TotalDoc { get; set; }
        public int TotalTodo { get; set; }
        public int TotalPass { get; set; }
        public int TotalFail { get; set; }

        public string? DepartmentName { get; set; }
        public string? AuditorName { get; set; }
        public ReportingAuditType? Type { get; set; }

    }
}
