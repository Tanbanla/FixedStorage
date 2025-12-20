using Microsoft.AspNetCore.Http.Features;

string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;    
});
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", pol => pol.SetIsOriginAllowed(x => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});
builder.AddServiceDefaults();

builder.Services.AddDbContexts(builder.Configuration);

//builder.Services.AddTransient<IDatabaseConnectionService>(s => { return new DatabaseConnectionService(builder.Configuration.GetConnectionString("IdentityDB")); });


// Register the Identity services.
builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

// Configure Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    //options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;


    // Lockout settings
    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    //options.Lockout.MaxFailedAccessAttempts = 3;

    // User settings
    //options.User.RequireUniqueEmail = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Identity API",
        Description = "BIVN"
    });   

    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = Constants.HttpContextModel.AuthorizationKey,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header"

    });
    s.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         Array.Empty<string>()
                    }
                });
});

// Register the worker responsible for seeding the database.
// Note: in a real world application, this step should be part of a setup script.
//builder.Services.AddHostedService<Worker>();

builder.Services.AddApplicationOptions(builder.Configuration);
builder.Services.AddIntegrationServices();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

//Rest client
var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("StorageServiceUrl:Address").Value);
builder.Services.AddSingleton<IRestClient>(new RestClient(httpClient));

builder.Services.AddSingleton(new RestClientFactory(builder.Configuration));

builder.Services.Configure<FormOptions>(c =>
{
    c.MultipartBodyLengthLimit = (1024 * 1024) * 5;
});

var app = builder.Build();
app.UseCors("CorsPolicy");
// logs
//var path = Directory.GetCurrentDirectory();
//var loggerFactory = app.Services.GetService<ILoggerFactory>();
//loggerFactory.AddFile($"{path}\\Logs\\Log.txt");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//else if (app.Environment.IsDevelopment())
{
    //implement docs for api
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = true;
    });
    app.UseSwaggerUI(s =>
    {
        s.RoutePrefix = "swagger";
        s.SwaggerEndpoint("/swagger/v1/swagger.json", "BIVN API v1.1");
        s.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        s.ShowExtensions();
    });
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RequestHandlerMiddleware>();

app.MapControllers();
//app.MapDefaultControllerRoute();



//seed data
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IdentityContext>();
    var logger = app.Services.GetService<ILogger<IdentityContextSeed>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var restClientFactory = scope.ServiceProvider.GetRequiredService<RestClientFactory>();
    await context.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetService(typeof(RoleManager<AppRole>)) as RoleManager<AppRole>;
    var userManager = (UserManager<AppUser>)scope.ServiceProvider.GetService(typeof(UserManager<AppUser>));
    await new IdentityContextSeed(roleManager, userManager, context, configuration, restClientFactory).SeedAsync(context, app.Environment, logger);
    //var integrationEventLogContext = scope.ServiceProvider.GetRequiredService<IntegrationEventLogContext>();
    //await integrationEventLogContext.Database.MigrateAsync();
}

app.Run();
//await app.RunAsync();
