namespace BIVN.FixedStorage.Identity.API.Infrastructure.Entity.Configuration
{
    public class DepartmentEntityConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id).IsUnique();
            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ManagerId).HasMaxLength(36);
            builder.Property(x => x.CreatedBy).HasMaxLength(36);
            builder.Property(x => x.UpdatedBy).HasMaxLength(36);
            builder.Property(x => x.DeletedBy).HasMaxLength(36);            
        }
    }
}
