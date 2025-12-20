namespace BIVN.FixedStorage.Storage.API.Infrastructure.Entity.Configuration
{
    class FactoryEntityTypeConfiguration : IEntityTypeConfiguration<Services.Storage.API.Infrastructure.Entity.Factory>
    {
        public void Configure(EntityTypeBuilder<Services.Storage.API.Infrastructure.Entity.Factory> builder)
        {
            builder.ToTable("Factory");

            builder.HasKey(vi => vi.Id);

            //builder.HasMany(x => x.Storages).WithOne().HasForeignKey(x => x.FactoryId);
        }
    }
}
