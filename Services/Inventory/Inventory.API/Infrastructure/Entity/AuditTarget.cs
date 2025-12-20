namespace Inventory.API.Infrastructure.Entity
{
    [Index(nameof(ComponentCode))]
    public class AuditTarget : AuditEntity<Guid>
    {
        public AuditTargetStatus Status { get; set; }
        [MaxLength(100)]
        public string ComponentCode { get; set; } 
        [MaxLength(250)]
        public string ComponentName { get; set; }
        
        [MaxLength(250)]
        public string DepartmentName { get; set; }
        [MaxLength(250)]
        public string LocationName { get; set; }
        [MaxLength(150)]
        public string PositionCode { get; set; }
        [MaxLength(50)]
        public string? SaleOrderNo { get; set; }
        [MaxLength(50)]
        public string Plant { get; set; }
        public string WareHouseLocation { get; set; }


        public Guid AssignedAccountId { get; set; }



        public Guid InventoryId { get; set; }
        public virtual BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Inventory Inventory { get; set; }
    }
}
