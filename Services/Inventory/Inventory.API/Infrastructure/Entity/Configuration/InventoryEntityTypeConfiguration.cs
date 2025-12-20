namespace BIVN.FixedStorage.Inventory.API.Infrastructure.Entity.Configuration
{
    class InventoryEntityTypeConfiguration : IEntityTypeConfiguration<BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Inventory>
    {
        public void Configure(EntityTypeBuilder<BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Inventory> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.InventoryDocs)
                .WithOne(x => x.Inventory)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.AuditTargets)
               .WithOne(x => x.Inventory)
               .HasForeignKey(x => x.InventoryId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Name )
                    .IsClustered(false)
                    .HasName("Inventory_name");

        }
    }
}
