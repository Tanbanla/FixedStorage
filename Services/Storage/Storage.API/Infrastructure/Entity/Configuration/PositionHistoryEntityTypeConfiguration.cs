
namespace BIVN.FixedStorage.Storage.API.Infrastructure.Entity.Configuration
{
    class PositionHistoryEntityTypeConfiguration : IEntityTypeConfiguration<PositionHistory>
    {
        public void Configure(EntityTypeBuilder<PositionHistory> builder)
        {
            builder.HasIndex(x => new { x.CreatedAt })
                   .IsClustered(false)
                   .HasName($"{nameof(PositionHistory)}_{nameof(PositionHistory.CreatedAt)}");

            builder.HasIndex(x => new { x.DepartmentId })
                   .IsClustered(false)
                   .IncludeProperties(x => new 
                                        { 
                                            x.PositionHistoryType, 
                                            x.FactoryId 
                                        })
                   .HasName($"{nameof(PositionHistory)}_{nameof(PositionHistory.DepartmentId)}");
        }
    }
}
