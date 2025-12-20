namespace BIVN.FixedStorage.Storage.API.Infrastructure.Entity.Configuration
{
    class InputFromBwinEntityTypeConfiguration : IEntityTypeConfiguration<InputFromBwin>
    {
        public void Configure(EntityTypeBuilder<InputFromBwin> builder)
        {
            builder
            .HasMany(inputFromBwin => inputFromBwin.InputDetails) // Một InputFromBwin có nhiều InputDetail
            .WithOne(inputDetail => inputDetail.InputFromBwin)  // Mỗi InputDetail thuộc về một InputFromBwin
            .HasForeignKey(inputDetail => inputDetail.InputId); // Khóa ngoại trong InputDetail
        }
    }
}
