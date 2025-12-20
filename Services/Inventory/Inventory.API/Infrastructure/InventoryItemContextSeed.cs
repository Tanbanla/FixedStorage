namespace BIVN.FixedStorage.Services.Infrastructure;

public class InventoryItemContextSeed
{
    public async Task SeedAsync(InventoryContext context, IWebHostEnvironment env, ILogger<InventoryItemContextSeed> logger)
    {
        var policy = CreatePolicy(logger, nameof(InventoryItemContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            //var inventory = new BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Inventory
            //{
            //    Id = Guid.NewGuid(),
            //    CreatedAt = DateTime.Now,
            //    CreatedBy = Guid.NewGuid().ToString()
            //};

            //context.Inventories.Add(inventory);

            //await context.SaveChangesAsync();
        });
    }
   
    private AsyncRetryPolicy CreatePolicy(ILogger<InventoryItemContextSeed> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<SqlException>().
            WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Error seeding database (attempt {retry} of {retries})", prefix, retry, retries);
                }
            );
    }
}
