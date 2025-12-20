namespace BIVN.FixedStorage.Inventory.API.Infrastructure.Entity.Configuration
{
    class InventoryDocEntityTypeConfiguration : IEntityTypeConfiguration<InventoryDoc>
    {
        public void Configure(EntityTypeBuilder<InventoryDoc> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.DocHistories)
                .WithOne(x => x.InventoryDoc)
                .HasForeignKey(x => x.InventoryDocId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.DocTypeCDetails)
                .WithOne(x => x.InventoryDoc)
                .HasForeignKey(x => x.InventoryDocId);
            builder.HasMany(x => x.DocOutputs)
                .WithOne(x => x.InventoryDoc)
                .HasForeignKey(x => x.InventoryDocId);

            builder.HasIndex(x => x.CreatedAt)
                    .IsClustered(false)
                    .HasDatabaseName($"{nameof(InventoryDoc)}_{nameof(InventoryDoc.CreatedAt)}");

            builder.HasIndex(x => new { x.DocType, x.Status })
                    .IsClustered(false)
                    .IncludeProperties(x => new { x.DepartmentName, x.LocationName, x.InventoryId })
                    .HasDatabaseName($"{nameof(InventoryDoc)}_composition_doctype_status");

            builder.HasIndex(x=>new { x.ErrorQuantity, x.ErrorMoney})
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(InventoryDoc)}_composition_error_quantity_money");
        }
    }
}
