namespace BIVN.FixedStorage.Inventory.API.Infrastructure.Entity.Configuration
{
    class InventoryLocationTypeConfiguration : IEntityTypeConfiguration<InventoryLocation>
    {
        public void Configure(EntityTypeBuilder<InventoryLocation> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.InventoryAccounts)
                .WithOne(x => x.InventoryLocation)
                .HasForeignKey(x => x.LocationId);

        }
    }
}
