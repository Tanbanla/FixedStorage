namespace Inventory.API.Infrastructure.Entity
{
    [Index(nameof(AccountId))]
    [Index(nameof(LocationId))]
    public class AccountLocation : AuditEntity<Guid>
    {
        public required Guid AccountId { get; set; }
        public required Guid LocationId { get; set; }

        public virtual InventoryAccount InventoryAccount { get; set; }
        public virtual InventoryLocation InventoryLocation { get; set; }
    }
}
