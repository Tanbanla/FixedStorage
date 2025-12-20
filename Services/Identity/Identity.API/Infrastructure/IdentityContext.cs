namespace BIVN.FixedStorage.Identity.API.Infrastructure
{
    public class IdentityContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {

        }
       
        public DbSet<AppRole> AppRoles { set; get; }
        public DbSet<AppUser> AppUsers { set; get; }
        public DbSet<Department> Departments { set; get; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims").HasKey(x => x.Id);
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims").HasKey(x => x.Id);
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins").HasKey(x => x.UserId);
            builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRoles").HasKey(x => new { x.RoleId, x.UserId });
            builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens").HasKey(x => new { x.UserId });

            
            builder.ApplyConfiguration(new AppRoleEntityTypeConfiguration());
            builder.ApplyConfiguration(new AppUserEntityTypeConfiguration());
            builder.ApplyConfiguration(new DepartmentEntityConfiguration());

            //Soft delete Department
            builder.Entity<Department>().HasQueryFilter(d => d.IsDeleted.Value != true);
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json",
                optional: false,
                reloadOnChange: true)
            .Build();

            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            var connectionString = configuration.GetConnectionString("IdentityDB");            
            builder.UseSqlServer(connectionString)
                .EnableDetailedErrors();
            return new IdentityContext(builder.Options);
        }
    }
}
