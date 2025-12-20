namespace Inventory.API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            //Kiem tra Folder wwwroot, chua co thi tao:
            string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
            }

            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            builder.AddServiceDefaults();
            builder.Services.AddDbContexts(builder.Configuration);
            builder.Services.AddApplicationOptions(builder.Configuration);
            builder.Services.AddIntegrationServices();
            builder.Services.AddInventoryApplicationServices(builder.Configuration);
            builder.Services.AddCors(options =>
            {
                // TODO: Read allowed origins from configuration
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            var app = builder.Build();
            app.UseCors("CorsPolicy");
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            app.UseMiddleware<ValidateTokenMiddlware>();
            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.MapControllers();

            Task.Run(async () =>
            {
                //seed data
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                    var logger = app.Services.GetService<ILogger<InventoryItemContextSeed>>();

                    await context.Database.MigrateAsync();
                    await new InventoryItemContextSeed().SeedAsync(context, app.Environment, logger);

                    //var integrationEventLogContext = scope.ServiceProvider.GetRequiredService<IntegrationEventLogContext>();
                    //await integrationEventLogContext.Database.MigrateAsync();
                }
            });

            app.Run();
        }
    }
}
