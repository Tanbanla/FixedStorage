namespace Inventory.API.Infrastructure.Entity
{
    public class InventoryLocation : AuditEntity<Guid>
    {
        [MaxLength(100)]
        public string FactoryName { get; set; }
        [MaxLength(250)]
        public string DepartmentName { get; set; }
        [MaxLength(250)]
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<InventoryAccount> InventoryAccounts { get; set; }

        public ICollection<AccountLocation> AccountLocations { get; set; }
    }
}
