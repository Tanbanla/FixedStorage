public class StorageContext : DbContext
{
    public StorageContext(DbContextOptions<StorageContext> options) : base(options)
    {
    }
    public DbSet<BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity.Factory> Factories { get; set; }
    public DbSet<BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity.Storage> Storages { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<PositionHistory> PositionHistories { get; set; }
    public DbSet<InputFromBwin> InputFromBwins { get; set; }
    public DbSet<InputDetail> InputDetails { get; set; }
    public DbSet<TemporaryStore> TemporaryStores { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new FactoryEntityTypeConfiguration());
        builder.ApplyConfiguration(new PositionEntityTypeConfiguration());
        builder.ApplyConfiguration(new StorageEntityTypeConfiguration());
        builder.ApplyConfiguration(new InputFromBwinEntityTypeConfiguration());
        builder.ApplyConfiguration(new PositionHistoryEntityTypeConfiguration());
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
