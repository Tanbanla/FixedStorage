namespace Inventory.API.Infrastructure.Entity
{
    public class HistoryOutput : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }

        public Guid? DocHistoryId { get; set; }
        public DocHistory DocHistory { get; set; }

    }
}
