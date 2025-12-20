namespace BIVN.FixedStorage.Storage.API.Infrastructure.Entity.Configuration
{
    class PositionEntityTypeConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable("Positions");
            builder.HasMany(x => x.PositionHistories).WithOne().HasForeignKey(x => x.PositionId);
            //builder.HasMany(x => x.BwinHistories).WithOne().HasForeignKey(x => x.PositionId);

        }
    }
}
