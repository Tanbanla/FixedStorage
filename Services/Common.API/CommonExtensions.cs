using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using @constant = BIVN.FixedStorage.Services.Common.API.Constants;

namespace BIVN.FixedStorage.Services.Common.API;

public static class CommonExtensions
{
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        // Shared configuration via key vault
        //builder.Configuration.AddKeyVault();

        // Shared app insights configuration
        //builder.Services.AddApplicationInsights(builder.Configuration);

        // Default health checks assume the event bus and self health checks
        //builder.Services.AddDefaultHealthChecks(builder.Configuration);

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        builder.Services.Configure<JsonSerializerSettingsConfig>(options =>
        {
            options.JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        });

        // Add the event bus
        //builder.Services.AddEventBus(builder.Configuration);

        //builder.Services.AddDefaultAuthentication(builder.Configuration);

        builder.Services.AddDefaultOpenApi(builder.Configuration);

        builder.Services.AddHttpContextAccessor();
        // Add the accessor

        return builder;
    }

    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }

        var pathBase = app.Configuration["PATH_BASE"];

        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
            app.UseRouting();

            //var identitySection = app.Configuration.GetSection("Identity");

            //if (identitySection.Exists())
            //{
            //    // We have to add the auth middleware to the pipeline here
            //    app.UseAuthentication();
            //    app.UseAuthorization();
            //}
        }

        app.UseDefaultOpenApi(app.Configuration);

        //app.MapDefaultHealthChecks();

        return app;
    }

    //public static async Task<bool> CheckHealthAsync(this WebApplication app)
    //{
    //    app.Logger.LogInformation("Running health checks...");

    //    // Do a health check on startup, this will throw an exception if any of the checks fail
    //    var report = await app.Services.GetRequiredService<HealthCheckService>().CheckHealthAsync();

    //    if (report.Status == HealthStatus.Unhealthy)
    //    {
    //        app.Logger.LogCritical("Health checks failed!");
    //        foreach (var entry in report.Entries)
    //        {
    //            if (entry.Value.Status == HealthStatus.Unhealthy)
    //            {
    //                app.Logger.LogCritical("{Check}: {Status}", entry.Key, entry.Value.Status);
    //            }
    //        }

    //        return false;
    //    }

    //    return true;
    //}

    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app, IConfiguration configuration)
    {
        var openApiSection = configuration.GetSection("OpenApi");

        if (!openApiSection.Exists())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(setup =>
        {
            /// {
            ///   "OpenApi": {
            ///     "Endpoint: {
            ///         "Name": 
            ///     },
            ///     "Auth": {
            ///         Constants.HttpContextModel.ClientIdKey: ..,
            ///         "AppName": ..
            ///     }
            ///   }
            /// }

            var pathBase = configuration["PATH_BASE"];
            var authSection = openApiSection.GetSection("Auth");
            var endpointSection = openApiSection.GetRequiredSection("Endpoint");
            var urlsSection = configuration.GetSection("Urls");
            foreach (var item in urlsSection.GetChildren())
            {
                var swaggerUrl = $"{item.Value}/swagger/v1/swagger.json";

                setup.SwaggerEndpoint(swaggerUrl,item.Key);
            }
           

            if (authSection.Exists())
            {
                setup.OAuthClientId(authSection.GetRequiredValue(Constants.HttpContextModel.ClientIdKey));
                setup.OAuthAppName(authSection.GetRequiredValue("AppName"));
            }
        });

        // Add a redirect from the root of the app to the swagger endpoint
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

        return app;
    }

    public static IServiceCollection AddDefaultOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        var openApi = configuration.GetSection("OpenApi");

        if (!openApi.Exists())
        {
            return services;
        }

        services.AddEndpointsApiExplorer();

        return services.AddSwaggerGen(options =>
        {
            /// {
            ///   "OpenApi": {
            ///     "Document": {
            ///         "Title": ..
            ///         "Version": ..
            ///         "Description": ..
            ///     }
            ///   }
            /// }
            var document = openApi.GetRequiredSection("Document");

            var version = document.GetRequiredValue("Version") ?? "v1";
            options.ResolveConflictingActions(des=>des.First());
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = document.GetRequiredValue("Title"),
                Version = version,
                Description = document.GetRequiredValue("Description")
            });

            var identitySection = configuration.GetSection("Identity");

            if (!identitySection.Exists())
            {
                // No identity section, so no authentication open api definition
                return;
            }

            // {
            //   "Identity": {
            //     "ExternalUrl": "http://identity",
            //     "Scopes": {
            //         "basket": "Basket API"
            //      }
            //    }
            // }

            var identityUrlExternal = identitySection["ExternalUrl"] ?? identitySection.GetRequiredValue("Url");
            var scopes = identitySection.GetRequiredSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value);

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{identityUrlExternal}/connect/authorize"),
                        TokenUrl = new Uri($"{identityUrlExternal}/connect/token"),
                        Scopes = scopes,
                    }
                }
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
            options.OperationFilter<AddHeaderParameterFilter>();
        });
    }

    public static IServiceCollection AddDefaultAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // {
        //   "Identity": {
        //     "Url": "http://identity",
        //     "Audience": "basket"
        //    }
        // }

        var identitySection = configuration.GetSection("Identity");

        if (!identitySection.Exists())
        {
            // No identity section, so no authentication
            return services;
        }

        // prevent from mapping "sub" claim to nameidentifier.
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        services.AddAuthentication().AddJwtBearer(options =>
        {
            var identityUrl = identitySection.GetRequiredValue("Url");
            var audience = identitySection.GetRequiredValue("Audience");

            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = audience;
            options.TokenValidationParameters.ValidateAudience = false;
        });

        return services;
    }

    //public static ConfigurationManager AddKeyVault(this ConfigurationManager configuration)
    //{
    //    // {
    //    //   "Vault": {
    //    //     "Name": "myvault",
    //    //     "TenantId": "mytenantid",
    //    //     Constants.HttpContextModel.ClientIdKey: "myclientid",
    //    //    }
    //    // }

    //    var vaultSection = configuration.GetSection("Vault");

    //    if (!vaultSection.Exists())
    //    {
    //        return configuration;
    //    }

    //    var credential = new ClientSecretCredential(
    //        vaultSection.GetRequiredValue("TenantId"),
    //        vaultSection.GetRequiredValue(Constants.HttpContextModel.ClientIdKey),
    //        vaultSection.GetRequiredValue(Constants.HttpContextModel.ClientSecretKey));

    //    var name = vaultSection.GetRequiredValue("Name");

    //    configuration.AddAzureKeyVault(new Uri($"https://{name}.vault.azure.net/"), credential);

    //    return configuration;
    //}

    //public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    //{
    //    var appInsightsSection = configuration.GetSection("ApplicationInsights");

    //    // No instrumentation key, so no application insights
    //    if (string.IsNullOrEmpty(appInsightsSection["InstrumentationKey"]))
    //    {
    //        return services;
    //    }

    //    services.AddApplicationInsightsTelemetry(configuration);
    //    return services;
    //}

    //public static IHealthChecksBuilder AddDefaultHealthChecks(this IServiceCollection services, IConfiguration configuration)
    //{
    //    var hcBuilder = services.AddHealthChecks();

    //    // Health check for the application itself
    //    hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

    //    // {
    //    //   "EventBus": {
    //    //     "ProviderName": "ServiceBus | RabbitMQ",
    //    //   }
    //    // }

    //    var eventBusSection = configuration.GetSection("EventBus");

    //    if (!eventBusSection.Exists())
    //    {
    //        return hcBuilder;
    //    }

    //    return eventBusSection["ProviderName"]?.ToLowerInvariant() switch
    //    {
    //        "servicebus" => hcBuilder.AddAzureServiceBusTopic(
    //                _ => configuration.GetRequiredConnectionString("EventBus"),
    //                _ => "fixstock_event_bus",
    //                name: "servicebus",
    //                tags: new string[] { "ready" }),

    //        _ => hcBuilder.AddRabbitMQ(
    //                _ => $"amqp://{configuration.GetRequiredConnectionString("EventBus")}",
    //                name: "rabbitmq",
    //                tags: new string[] { "ready" })
    //    };
    //}

    //public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    //{
    //    //  {
    //    //    "ConnectionStrings": {
    //    //      "EventBus": "..."
    //    //    },

    //    // {
    //    //   "EventBus": {
    //    //     "ProviderName": "ServiceBus | RabbitMQ",
    //    //     ...
    //    //   }
    //    // }

    //    // {
    //    //   "EventBus": {
    //    //     "ProviderName": "ServiceBus",
    //    //     "SubscriptionClientName": "fixstock_event_bus"
    //    //   }
    //    // }

    //    // {
    //    //   "EventBus": {
    //    //     "ProviderName": "RabbitMQ",
    //    //     "SubscriptionClientName": "...",
    //    //     "UserName": "...",
    //    //     "Password": "...",
    //    //     "RetryCount": 1
    //    //   }
    //    // }

    //    var eventBusSection = configuration.GetSection("EventBus");

    //    if (!eventBusSection.Exists())
    //    {
    //        return services;
    //    }

    //    //if (string.Equals(eventBusSection["ProviderName"], "ServiceBus", StringComparison.OrdinalIgnoreCase))
    //    //{
    //    //    services.AddSingleton<IServiceBusPersisterConnection>(sp =>
    //    //    {
    //    //        var serviceBusConnectionString = configuration.GetRequiredConnectionString("EventBus");

    //    //        return new DefaultServiceBusPersisterConnection(serviceBusConnectionString);
    //    //    });

    //    //    services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
    //    //    {
    //    //        var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
    //    //        var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
    //    //        var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
    //    //        string subscriptionName = eventBusSection.GetRequiredValue("SubscriptionClientName");

    //    //        return new EventBusServiceBus(serviceBusPersisterConnection, logger,
    //    //            eventBusSubscriptionsManager, sp, subscriptionName);
    //    //    });
    //    //}
    //    //else
    //    {
    //        services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
    //        {
    //            var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

    //            var factory = new ConnectionFactory()
    //            {
    //                HostName = configuration.GetRequiredConnectionString("EventBus"),
    //                DispatchConsumersAsync = true,
    //                //UserName="admin",
    //                //Password="Tinhvan!123"
    //            };

    //            if (!string.IsNullOrEmpty(eventBusSection["UserName"]))
    //            {
    //                factory.UserName = eventBusSection["UserName"];
    //            }

    //            if (!string.IsNullOrEmpty(eventBusSection["Password"]))
    //            {
    //                factory.Password = eventBusSection["Password"];
    //            }
    //            factory.VirtualHost = "/";
    //            var retryCount = eventBusSection.GetValue("RetryCount", 5);

    //            return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
    //        });

    //        services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
    //        {
    //            var subscriptionClientName = eventBusSection.GetRequiredValue("SubscriptionClientName");
    //            var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
    //            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
    //            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
    //            var retryCount = eventBusSection.GetValue("RetryCount", 5);

    //            return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, sp, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
    //        });
    //    }

    //    services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
    //    return services;
    //}

    //public static void MapDefaultHealthChecks(this IEndpointRouteBuilder routes)
    //{
    //    routes.MapHealthChecks("/hc", new HealthCheckOptions()
    //    {
    //        Predicate = _ => true,
    //        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    //    });

    //    routes.MapHealthChecks("/liveness", new HealthCheckOptions
    //    {
    //        Predicate = r => r.Name.Contains("self")
    //    });
    //}

    public static ValidateTokenResultDto CurrentUser(this HttpContext httpContext)
    {
        return (ValidateTokenResultDto)httpContext?.Items[Constants.HttpContextModel.UserKey];
    }
    public static IEnumerable<RoleClaimDto> UserPermissions(this HttpContext httpContext)
    {
        var currentUser = httpContext?.CurrentUser();
        return currentUser?.RoleClaims;
    }

    public static bool TryGetFactoriesFromUserRole(this HttpContext httpContext, out IEnumerable<RoleClaimDto> permissions)
    {
        permissions = Enumerable.Empty<RoleClaimDto>();

        var currentUser = httpContext?.CurrentUser();
        if (currentUser == null) return false;

        permissions = currentUser?.RoleClaims?.Where(x => x.ClaimType == @constant.Permissions.FACTORY_DATA_INQUIRY);
        return permissions?.Any() ?? false; 
    }

    public static bool TryGetDepartmentsFromUserRole(this HttpContext httpContext, out IEnumerable<RoleClaimDto> permissions)
    {
        permissions = Enumerable.Empty<RoleClaimDto>();

        var currentUser = httpContext?.CurrentUser();
        if (currentUser == null) return false;

        permissions = currentUser?.RoleClaims?.Where(x => x.ClaimType == @constant.Permissions.DEPARTMENT_DATA_INQUIRY);
        return permissions?.Any() ?? false;
    }
}
