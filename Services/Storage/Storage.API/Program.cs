var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.Services.AddApplicationOptions(builder.Configuration);
builder.Services.AddRequiredServices(builder.Configuration);
builder.Services.AddIntegrationServices();





//======================================================================================================================
var app = builder.Build();
app.UseCors("CorsPolicy");
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ValidateTokenMiddlware>();
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();


//seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StorageContext>();
    var logger = app.Services.GetService<ILogger<StorageItemContextSeed>>();
    await context.Database.MigrateAsync();

    //await new StorageItemContextSeed().SeedAsync(context, app.Environment, logger);
    //var integrationEventLogContext = scope.ServiceProvider.GetRequiredService<IntegrationEventLogContext>();
    //await integrationEventLogContext.Database.MigrateAsync();
}


await app.RunAsync();
