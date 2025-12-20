namespace Inventory.API.Infrastructure.Entity
{
    [Index(nameof(MainModelCode))]
    [Index(nameof(UnitModelCode))]
    [Index(nameof(InventoryId))]
    [Index(nameof(InventoryDocId))]
    public class DocTypeCComponent : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        public Guid? InventoryDocId { get; set; }
        [MaxLength(9)]
        public string ComponentCode { get; set; }
        [MaxLength(10)]
        public string MainModelCode { get; set; }
        [MaxLength(10)]
        public string UnitModelCode { get; set; }
        public double QuantityOfBOM { get; set; }
        public double QuantityPerBOM { get; set; }
        public double TotalQuantity { get; set; }
        [MaxLength(10)]
        public string Plant { get; set; }
        [MaxLength(10)]
        public string WarehouseLocation { get; set; }
        public double SyntheticQuantity { get; set; }
    }
}
