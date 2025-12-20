namespace Inventory.API.Infrastructure.Entity
{
    public class DocOutput : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }

        public Guid? InventoryDocId { get; set; }
        public InventoryDoc InventoryDoc { get; set; }

    }
}
