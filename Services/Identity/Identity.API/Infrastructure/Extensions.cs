namespace BIVN.FixedStorage.Identity.API.Infrastructure
{
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

            services.AddDbContext<IdentityContext>(options =>
            {
                var connectionString = configuration.GetRequiredConnectionString("IdentityDB");

                options.UseSqlServer(connectionString);
            });

            //services.AddDbContext<IntegrationEventLogContext>(options =>
            //{
            //    var connectionString = configuration.GetRequiredConnectionString("IdentityDB");

            //    options.UseSqlServer(connectionString, ConfigureSqlOptions);
            //});

            services.AddDbContext<DeviceTokenContext>();
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
            //Register for DI
            //services.AddTransient<SignInManager<AppUser>, SignInManager<AppUser>>();
            //services.AddTransient<UserManager<AppUser>, UserManager<AppUser>>();
            //services.AddTransient<RoleManager<AppRole>, RoleManager<AppRole>>();
            //services.AddTransient<IdentityContextSeed>();            

            //services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
            //    sp => (DbConnection c) => new IntegrationEventLogService(c));

            //services.AddTransient<IIdentityIntegrationEventService, IdentityIntegrationEventService>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IUserService, UserService>();            
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IDepartmentService, DepartmentService>();
            services.AddTransient<IInternalService, InternalUserService>();

            return services;
        }
    }
}
