namespace WebApp.Infrastructure
{
    public static class ConfigService
    {
        public static IServiceCollection AddRequiredServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<APIGateWay>(configuration.GetSection(nameof(APIGateWay)));
            services.AddHttpContextAccessor();
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddSession();

            // CookieAuthenticationDefaults.AuthenticationScheme            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
             {
                 options.Cookie.Name = "BIVN User";
                 options.LoginPath = Path.Join(Application.Constants.GlobalRoutePrefix, "/login");
                 options.ExpireTimeSpan = TimeSpan.FromDays(2);
                 options.SlidingExpiration = true;
                 options.CookieManager = new ChunkingCookieManager();
            });

            // Add services to the container.
            services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            //Add Http client
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri($"{configuration.GetSection("APIGateWay:BaseAddress").Value}");
            services.AddSingleton<IRestClient>(new RestClient(httpClient));

            services.AddAutoRegisterService(configuration);
            return services;
        }

        public static IServiceCollection AddAutoRegisterService(this IServiceCollection services, IConfiguration configuration)
        {
            //Auto register service 
            var assemblyToScan = Assembly.GetAssembly(typeof(IBaseService));
            services.RegisterAssemblyPublicNonGenericClasses(assemblyToScan)
                .Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Services"))
                .AsPublicImplementedInterfaces();

            return services;
        }
    }
}
