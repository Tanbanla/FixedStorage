namespace Inventory.API.Infrastructure.Entity
{
    public class HistoryTypeCDetail : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }

        [MaxLength(50)]
        public string ComponentCode { get; set; }
        [MaxLength(150)]
        public string ModelCode { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
        public bool IsHighlight { get; set; }

        public Guid? HistoryId { get; set; }
        public DocHistory DocHistory { get; set; }
    }
}
