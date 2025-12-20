namespace Inventory.API.Infrastructure.Entity
{
    [Index(nameof(ModelCode))]
    [Index(nameof(InventoryDocId))]
    [Index(nameof(InventoryId))]
    public class DocTypeCDetail : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        [MaxLength(50)]
        public string ComponentCode { get; set; }
        [MaxLength(150)]
        public string ModelCode { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
        public bool isHighlight { get; set; }
        public string WarehouseLocation { get; set; }
        [MaxLength(10)]
        public string DirectParent { get; set; }
        public Guid? InventoryDocId { get; set; }
        public virtual InventoryDoc InventoryDoc { get; set; }
        public int? No { get; set; }
    }
}
