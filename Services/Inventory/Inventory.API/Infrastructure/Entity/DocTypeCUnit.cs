namespace Inventory.API.Infrastructure.Entity
{
    public class DocTypeCUnit : AuditEntity<Guid>
    {
        public Guid InventoryId { get; set; }
        [MaxLength(10)]
        public string ModelCode { get; set; }
        [MaxLength(5)]
        public string Plant { get; set; }
        [MaxLength(5)]
        public string WarehouseLocation { get; set; }
        [MaxLength(4)]
        public string MachineModel { get; set; }
        [MaxLength (1)]
        public string MachineType { get; set; }
        [MaxLength(1)]
        public string LineName { get; set; }
        [MaxLength(1)]
        public string lineType { get; set; }
        [MaxLength(100)]
        public string StageName { get; set; }
        [MaxLength(3)]
        public string StageNumeber { get; set; }

        public ICollection<DocTypeCUnitDetail> DocTypeCUnitDetails { get; set; }
    }
}
