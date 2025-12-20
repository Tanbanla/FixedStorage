using @storage = BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity.Storage;

namespace BIVN.FixedStorage.Services.Infrastructure;

public class StorageItemContextSeed
{
    public async Task SeedAsync(StorageContext context, IWebHostEnvironment env, ILogger<StorageItemContextSeed> logger)
    {
        var policy = CreatePolicy(logger, nameof(StorageItemContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            var FactoryF1 = new Storage.API.Infrastructure.Entity.Factory
            { 
                Id = Guid.NewGuid(),
                Name = "F1",
                Code = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.NewGuid().ToString()
            };
            var StorageA001 = new @storage
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.NewGuid().ToString(),
                FactoryId = context.Entry(FactoryF1).Entity.Id
            };
            var PositionA = new Position
            {
                Id = Guid.NewGuid(),
                InventoryNumber = 0,
                MaxInventoryNumber = 3000,
                MinInventoryNumber = 1,
                CreatedAt= DateTime.Now,
                CreatedBy = Guid.NewGuid().ToString(),
                Layout = "5T2A1-07/02-01",
                PositionCode= Guid.NewGuid().ToString(),
                StorageId = context.Entry(StorageA001).Entity.Id,
                ComponentName = "Rulo ép dưới",
            };

            //Seeding Factory
            if(context.Factories?.Any() == false)
            {
                context.Factories.Add(FactoryF1);
            }
            //Sedding Storage
            if (context.Storages?.Any() == false)
            {
                context.Storages.Add(StorageA001);
            }
            //Sedding Position
            if (context.Positions?.Any() == false)
            {
                context.Positions.Add(PositionA);

                var history = new PositionHistory
                {
                    Id = Guid.NewGuid(),
                    PositionHistoryType = PositionHistoryType.Input,
                    CreatedAt = DateTime.Now,
                    AppUserId = Guid.NewGuid(),
                    CreatedBy = Guid.NewGuid().ToString(),
                    FactoryName = FactoryF1.Name,
                    DepartmentId = Guid.NewGuid(),
                    Quantity = 500,
                    InventoryNumber = 500,
                    PositionId = context.Entry(PositionA).Entity.Id,
                };

                context.PositionHistories.Add(history);
            }
            await context.SaveChangesAsync();
        });
    }
   
    private AsyncRetryPolicy CreatePolicy(ILogger<StorageItemContextSeed> logger, string prefix, int retries = 3)
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
