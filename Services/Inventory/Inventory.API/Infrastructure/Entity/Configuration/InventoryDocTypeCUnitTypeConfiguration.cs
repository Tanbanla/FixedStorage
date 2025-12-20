namespace BIVN.FixedStorage.Inventory.API.Infrastructure.Entity.Configuration
{
    class InventoryDocTypeCUnitTypeConfiguration : IEntityTypeConfiguration<DocTypeCUnit>
    {
        public void Configure(EntityTypeBuilder<DocTypeCUnit> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.DocTypeCUnitDetails)
                .WithOne(x => x.DocTypeCUnit)
                .HasForeignKey(x => x.DocTypeCUnitId);

        }
    }
}
