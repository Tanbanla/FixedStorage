using InventoryStatus = Inventory.API.Infrastructure.Entity.Enums.InventoryStatus;

namespace BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity
{
    public class Inventory : AuditEntity<Guid>
    {
        [MaxLength(250)]
        public string Name { get; set; }
        public DateTime InventoryDate { get; set; }
        public InventoryStatus InventoryStatus { get; set; }
        public double AuditFailPercentage { get; set; }
        public bool IsReportRunning { get; set; }
        public DateTime? ForceAggregateAt { get; set; }

        public bool? IsLocked { get; set; }

        public ICollection<InventoryDoc> InventoryDocs { get; set; }
        public ICollection<AuditTarget> AuditTargets { get; set; }

        public ICollection<ErrorInvestigation> ErrorInvestigations { get; set; }
    }
}
