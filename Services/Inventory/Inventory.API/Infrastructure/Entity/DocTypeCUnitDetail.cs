namespace Inventory.API.Infrastructure.Entity
{
    public class DocTypeCUnitDetail:AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        public Guid DocTypeCUnitId { get; set; }
        [MaxLength(9)]
        public string ComponentCode { get; set; }
        [MaxLength (10)]
        public string ModelCode { get; set; }
        public int QuantityOfBOM { get; set; }
        public int QuantityPerBOM { get; set; }
        public bool IsHighLight { get; set; }
        [MaxLength(10)]
        public string DirectParent { get; set; }
        public virtual DocTypeCUnit DocTypeCUnit { get; set; }

    }
}
