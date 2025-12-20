namespace BIVN.FixedStorage.Inventory.API.Infrastructure.Entity.Configuration
{
    class InventoryAccountLocationTypeConfiguration : IEntityTypeConfiguration<AccountLocation>
    {
        public void Configure(EntityTypeBuilder<AccountLocation> builder)
        {
            builder.HasKey(x => new {x.AccountId, x.LocationId});

            //Account - locations
            builder.HasOne(x1 => x1.InventoryAccount)
                    .WithMany(x => x.AccountLocations)
                    .HasForeignKey(x1 => x1.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.InventoryLocation)
                    .WithMany(x => x.AccountLocations)
                    .HasForeignKey(x => x.LocationId);

        }
    }
}
