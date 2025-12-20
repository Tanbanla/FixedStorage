namespace BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Configuration
{
    public class ReportAuditTypeConfiguration : IEntityTypeConfiguration<ReportingAudit>
    {
        public void Configure(EntityTypeBuilder<ReportingAudit> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.InventoryId).IsRequired();
            builder.Property(x => x.LocationtName).HasMaxLength(50);
            builder.Property(x => x.TotalDoc).IsRequired();
            builder.Property(x => x.TotalTodo).IsRequired();
            builder.Property(x => x.TotalPass).IsRequired();
            builder.Property(x => x.TotalFail).IsRequired();
            builder.Property(x => x.DepartmentName).HasMaxLength(50);
            builder.Property(x => x.AuditorName).HasMaxLength(50);
            builder.Property(x => x.Type);

            builder.HasIndex(x => x.InventoryId)
                .IsClustered(false)
                .HasDatabaseName($"IDX_{nameof(ReportingAudit)}_{nameof(ReportingAudit.InventoryId)}");


            builder.HasIndex(x => new { x.InventoryId, x.DepartmentName, x.LocationtName, x.AuditorName })
              .IsClustered(false)
              .HasDatabaseName($"IDX_InventoryId_DepartmentName_LocationtName_AuditorName_COMPOSITION");
        }
    }
}
