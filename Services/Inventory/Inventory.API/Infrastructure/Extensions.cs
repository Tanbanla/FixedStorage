using Inventory.API.Service.ErrorInvestigation;
using Microsoft.AspNetCore.Http.Features;

public static class Extensions
{
    public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        static void ConfigureSqlOptions(SqlServerDbContextOptionsBuilder sqlOptions)
        {
            sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);

            // Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 

            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        };
        services.AddDbContextFactory<InventoryContext>(options =>
        {
            var connectionString = configuration.GetRequiredConnectionString("InventoryDB");
            options.UseSqlServer(connectionString, ConfigureSqlOptions);
        });
        services.AddDbContext<InventoryContext>(options =>
        {
            var connectionString = configuration.GetRequiredConnectionString("InventoryDB");

            options.UseSqlServer(connectionString, ConfigureSqlOptions).EnableSensitiveDataLogging();
        });

        //services.AddDbContext<IntegrationEventLogContext>(options =>
        //{
        //    var connectionString = configuration.GetRequiredConnectionString("InventoryDB");

        //    options.UseSqlServer(connectionString, ConfigureSqlOptions);
        //});

        return services;
    }

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Move to the new problem details middleware
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details."
                };

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json", "application/problem+xml" }
                };
            };
        });

        return services;
    }

    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        services.AddTransient<IInventoryService, InventoryService>();
        services.AddTransient<IInternalService, InternalService>();

        services.AddTransient<IInventoryWebService, InventoryWebService>();
        services.AddTransient<ILocationService, LocationService>();

        services.AddTransient<IAuditTargetWebService, AuditTargetWebService>();
        services.AddTransient<IDataAggregationService, DataAggregationService>();

        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(10000));
        services.AddHostedService<InventoryReportingService>();
        services.AddHostedService<InventoryCheckingService>();


        services.AddTransient<IDocumentResultService, DocumentResultService>();
        services.AddTransient<IInventoryHistoryService, InventoryHistoryService>();
        services.AddTransient<IReportService, ReportService>();
        services.AddTransient<IErrorInvestigationService, ErrorInvestigationService>();
        services.AddTransient<IErrorInvestigationWebService, ErrorInvestigationWebService>();
        return services;
    }


    public static IServiceCollection AddInventoryApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = Constants.HttpContextModel.AuthorizationKey,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header "

            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                         new string[] {}
                    }
                });

            o.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
                if (controllerActionDescriptor != null)
                {
                    return new[] { controllerActionDescriptor.ControllerName };
                }

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });
            o.DocInclusionPredicate((name, api) => true);
        });

        //Response key: camelCase
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", pol =>
            pol.SetIsOriginAllowed(x => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.Configure<FormOptions>(c =>
        {
            c.MultipartBodyLengthLimit = (1024 * 1024) * 5;
        });

        //Rest client
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(configuration.GetSection("IdentityServiceUrl:Address").Value);
        services.AddSingleton<IRestClient>(new RestClient(httpClient));
        services.AddSingleton(new RestClientFactory(configuration));

        services.AddScoped<IDatabaseFactoryService>(s =>
        {
            return new DatabaseFactoryService(configuration.GetSection("ConnectionStrings:InventoryDB").Value);
        });


        services.AddMemoryCache();



        return services;
    }
}
