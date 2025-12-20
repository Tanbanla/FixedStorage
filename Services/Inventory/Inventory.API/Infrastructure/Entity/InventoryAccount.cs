namespace Inventory.API.Infrastructure.Entity
{
    public class InventoryAccount : AuditEntity<Guid>
    {
        public Guid UserId { get; set; }
        [MaxLength(100)]
        public string UserName { get; set; }
        public InventoryAccountRoleType? RoleType { get; set; }
        public Guid? LocationId { get; set; }
        public virtual InventoryLocation InventoryLocation { get; set; }

        public ICollection<AccountLocation> AccountLocations { get; set; }
    }
}
