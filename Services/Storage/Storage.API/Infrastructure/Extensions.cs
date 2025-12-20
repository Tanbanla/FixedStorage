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

        services.AddDbContext<StorageContext>(options =>
        {
            var connectionString = configuration.GetRequiredConnectionString("StorageDB");

            options.UseSqlServer(connectionString, ConfigureSqlOptions);
        });

        //services.AddDbContext<IntegrationEventLogContext>(options =>
        //{
        //    var connectionString = configuration.GetRequiredConnectionString("StorageDB");

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

    public static IServiceCollection AddRequiredServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Add DB context
        services.AddDbContexts(configuration);

        //Add Swagger 
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
        });

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });
        

        //Disable Auto Model validation
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddSingleton<IDatabaseFactoryService>(s =>
        {
            return new DatabaseFactoryService(configuration.GetSection("ConnectionStrings:StorageDB").Value);
        });


        //Add http client
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(configuration.GetSection("IdentityServiceUrl:Address").Value);
        services.AddSingleton<IRestClient>(new RestClient(httpClient));

        //Add CORS
        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", pol => pol.SetIsOriginAllowed(x => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        //services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
        //    sp => (DbConnection c) => new IntegrationEventLogService(c));

        //services.AddTransient<IStorageItemIntegrationEventService, StorageItemIntegrationEventService>();
        services.AddTransient<IStorageService, StorageService>();
        services.AddTransient<IPositionService, PositionService>();
        services.AddTransient<IStorageService, StorageService>();
        services.AddTransient<IFactoryService, FactoryService>();
        services.AddTransient<IHistoryService, HistoryService>();
        services.AddTransient<IInternalService, InternalService>();
        return services;
    }
}



        
