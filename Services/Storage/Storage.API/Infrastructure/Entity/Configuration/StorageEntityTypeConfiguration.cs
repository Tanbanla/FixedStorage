
namespace BIVN.FixedStorage.Storage.API.Infrastructure.Entity.Configuration
{
    class StorageEntityTypeConfiguration : IEntityTypeConfiguration<BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity.Storage>
    {
        public void Configure(EntityTypeBuilder<BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity.Storage> builder)
        {
            builder.ToTable("Storages");

            builder.HasKey(k => k.Id);

            //builder.HasMany(x => x.Positions).WithOne().HasForeignKey(x => x.StorageId);
        }
    }
}
