
using BIVN.FixedStorage.Inventory.API.Infrastructure.Entity.Configuration;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Configuration;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
    {
    }
    public DbSet<BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Inventory> Inventories { get; set; }
    public DbSet<InventoryDoc> InventoryDocs { get; set; }
    public DbSet<AuditTarget> AuditTargets { get; set; }
    public DbSet<DocOutput> DocOutputs { get; set; }
    public DbSet<DocTypeCDetail> DocTypeCDetails { get; set; }
    public DbSet<DocHistory> DocHistories { get; set; }
    public DbSet<HistoryOutput> HistoryOutputs { get; set; }
    public DbSet<HistoryTypeCDetail> HistoryTypeCDetails { get; set; }

    public DbSet<InventoryAccount> InventoryAccounts { get; set; }
    public DbSet<InventoryLocation> InventoryLocations { get; set; }
    public DbSet<DocTypeCComponent> DocTypeCComponents { get; set; }
    public DbSet<DocTypeCUnit> DocTypeCUnits { get; set; }
    public DbSet<DocTypeCUnitDetail> DocTypeCUnitDetails { get; set; }
    public DbSet<ReportingDepartment> ReportingDepartments { get; set; }
    public DbSet<ReportingLocation> ReportingLocations { get; set; }
    public DbSet<ReportingDocType> ReportingDocTypes { get; set; }
    public DbSet<ReportingAudit> ReportingAudits { get; set; }
    public DbSet<AccountLocation> AccountLocations { get; set; }
    public DbSet<ErrorInvestigationHistory> ErrorInvestigationHistories { get; set; }
    public DbSet<ErrorInvestigation> ErrorInvestigations { get; set; }
    public DbSet<ErrorInvestigationInventoryDoc> ErrorInvestigationInventoryDocs { get; set; }
    public DbSet<GeneralSetting> GeneralSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new InventoryEntityTypeConfiguration());
        builder.ApplyConfiguration(new InventoryDocEntityTypeConfiguration());
        builder.ApplyConfiguration(new InventoryLocationTypeConfiguration());
        builder.ApplyConfiguration(new InventoryDocTypeCUnitTypeConfiguration());
        builder.ApplyConfiguration(new InventoryAccountLocationTypeConfiguration());
        builder.ApplyConfiguration(new ErrorInvestigationHistoryTypeConfiguration());
        builder.ApplyConfiguration(new ErrorInvestigationTypeConfiguration());
        builder.ApplyConfiguration(new ErrorInvestigationInventoryDocTypeConfiguration());
        builder.ApplyConfiguration(new ReportAuditTypeConfiguration());
        builder.ApplyConfiguration(new GeneralSettingTypeConfiguration());

        //Soft delete InventoryDocs:
        builder.Entity<InventoryDoc>().HasQueryFilter(d => d.IsDeleted.Value != true);

    }
}


//public class ValueItemContextDesignFactory : IDesignTimeDbContextFactory<StorageContext>
//{
//    public StorageContext CreateDbContext(string[] args)
//    {
//        var optionsBuilder = new DbContextOptionsBuilder<StorageContext>()
//            .UseSqlServer("Server=tcp:10.4.0.112,1433;Initial Catalog=BIVN.FixedStorage.Services.StorageDb;User Id=sa;Password=Tinhvan@2024;TrustServerCertificate=True;Encrypt=false");

//        return new StorageContext(optionsBuilder.Options);
//    }
//}
