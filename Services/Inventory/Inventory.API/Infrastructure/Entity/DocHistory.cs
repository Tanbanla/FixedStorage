namespace Inventory.API.Infrastructure.Entity
{
    public class DocHistory : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        public double OldQuantity { get; set; }
        public double NewQuantity { get; set; }
        public InventoryDocStatus Status { get; set; }
        public InventoryDocStatus OldStatus { get; set; }
        public InventoryDocStatus NewStatus { get; set; }
        public bool IsChangeCDetail { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
        public DocHistoryActionType Action { get; set; }

        [MaxLength(500)]
        public string? EvicenceImg { get; set; }

        public Guid? InventoryDocId { get; set; }
        public InventoryDoc InventoryDoc { get; set; }
    }
}
