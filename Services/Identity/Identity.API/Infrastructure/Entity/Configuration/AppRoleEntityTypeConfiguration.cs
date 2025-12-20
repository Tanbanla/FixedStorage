namespace BIVN.FixedStorage.Identity.API.Infrastructure.Entity.Configuration
{
    class AppRoleEntityTypeConfiguration : IEntityTypeConfiguration<AppRole>
    {
        public void Configure(EntityTypeBuilder<AppRole> builder)
        {
            builder.ToTable("AppRoles");
            builder.HasKey(vi => vi.Id);
            builder.Property(c => c.Name).HasMaxLength(254).IsRequired();
            builder.HasIndex(c => c.Name).IsUnique();
            builder.Property(c => c.Description).HasMaxLength(254);
            builder.Property(x => x.CreatedBy).HasMaxLength(36);
            builder.Property(x => x.UpdatedBy).HasMaxLength(36);
            builder.Property(x => x.DeletedBy).HasMaxLength(36);
        }
    }
}
