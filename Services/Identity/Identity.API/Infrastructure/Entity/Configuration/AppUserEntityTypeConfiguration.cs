namespace BIVN.FixedStorage.Identity.API.Infrastructure.Entity.Configuration
{
    class AppUserEntityTypeConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("AppUsers");
            builder.HasKey(vi => vi.Id);
            //builder.Property(x => x.DepartmentId).HasMaxLength(36).IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(36);
            builder.Property(x => x.UpdatedBy).HasMaxLength(36);
            builder.Property(x => x.DeletedBy).HasMaxLength(36);
            builder.Property(x => x.Code).HasMaxLength(50);
            builder.Property(x => x.Address).HasMaxLength(254);
            builder.Property(x => x.FullName).HasMaxLength(254);
            builder.Property(x => x.Avatar).HasMaxLength(1000);
            builder.Property(x => x.RefreshToken).HasMaxLength(50);
        }
    }
}
