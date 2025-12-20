

namespace BIVN.FixedStorage.Identity.API
{
    public class DeviceTokenContext:DbContext
    {
        protected override void OnConfiguring
      (DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "DeviceTokenDb");
        }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
    }
}
