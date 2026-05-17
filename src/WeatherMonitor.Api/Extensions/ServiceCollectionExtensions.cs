using Asp.Versioning;
using FluentValidation;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Refit;
using System.Reflection;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.Customizer;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using WeatherMonitor.Api.Behaviors;
using WeatherMonitor.Api.Diagnostics;
using WeatherMonitor.Api.Features.MonitorProcessing;
using WeatherMonitor.Api.Infrastructure.Clients.Handlers;
using WeatherMonitor.Api.Infrastructure.Clients.Interfaces;
using WeatherMonitor.Api.Infrastructure.Keycloak;
using WeatherMonitor.Api.Infrastructure.Persistence;
using WeatherMonitor.Api.Infrastructure.Persistence.Interceptors;
using WeatherMonitor.Api.Middlewares;
using WeatherMonitor.Api.OpenApi;

namespace WeatherMonitor.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddExceptionHandling()
        {
            services.AddTransient<GlobalExceptionHandlerMiddleware>();

            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        internal IServiceCollection AddPermissiveCors()
        {
            return services.AddCors(options => options.AddPolicy(
                name: "Permissive",
                configurePolicy: cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader())
            );
        }

        internal IServiceCollection AddKeycloak(IConfiguration configuration)
        {
            var keycloak = configuration.GetKeycloakOptions<KeycloakOAuth2Options>();

            services.AddKeycloakWebApiAuthentication(configuration, configureJwtBearerOptions: options =>
            {
                options.RequireHttpsMetadata = keycloak?.RequireHttpsMetadata ?? true;
            });

            services.AddKeycloakAuthorization(options =>
            {
                options.EnableRolesMapping = RolesClaimTransformationSource.ResourceAccess;
                options.RolesResource = keycloak?.Resource ?? "weather_api";
            });

            return services;
        }

        internal IServiceCollection AddOpenApiDocument(IConfiguration configuration)
        {
            var keycloak = configuration.GetKeycloakOptions<KeycloakOAuth2Options>();

            services.AddOpenApi(options =>
            {
                if (keycloak is null)
                {
                    return;
                }

                options.AddOAuth2SecurityScheme(oauth2 =>
                {
                    oauth2.WithAuthorizationUrl(keycloak.KeycloakAuthEndpoint)
                        .WithTokenUrl(keycloak.KeycloakTokenEndpoint)
                        .WithOpenIdScope()
                        .WithEmailScope()
                        .WithProfileScope();
                });
            });

            return services;
        }

        internal IServiceCollection AddApiVersioning(ApiVersion version)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.DefaultApiVersion = version;
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        internal IServiceCollection AddCQRS()
        {
            var assembly = Assembly.GetCallingAssembly();

            services.AddMediatR(options =>
            {
                options.AddOpenBehavior(typeof(LoggingBehavior<,>));
                options.AddOpenBehavior(typeof(ValidationBehavior<,>));
                options.RegisterServicesFromAssembly(assembly);
            });

            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

            return services;
        }

        internal IServiceCollection AddCaching(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis");

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

            services.AddHybridCache();

            return services;
        }

        internal IServiceCollection AddBrasilApiClient(IConfiguration configuration)
        {
            var brasilApiUrl = configuration.GetValue<string>("BrasilApiUrl");

            ArgumentException.ThrowIfNullOrEmpty(brasilApiUrl);

            services.AddTransient<CachingHandler>();

            services
                .AddRefitClient<IBrasilApiClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(brasilApiUrl))
                .AddHttpMessageHandler<CachingHandler>()
                .AddStandardResilienceHandler(options =>
                {
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(1);
                    options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(1);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(2);
                });

            return services;
        }

        internal IServiceCollection AddWebhookDispatcherHttpClient()
        {
            services.AddHttpClient(nameof(WebhookMonitorDispatcher), configureClient: client => client.Timeout = TimeSpan.FromMinutes(1));

            return services;
        }

        internal IServiceCollection AddTimeProvider()
        {
            return services.AddSingleton(TimeProvider.System);
        }

        internal IServiceCollection AddAppDbContext()
        {
            services.AddSingleton<IInterceptor, AuditableEntitySaveChangesInterceptor>();

            services.AddDbContext<AppDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();

                var connectionString = configuration.GetConnectionString("WeatherMonitorDB");

                var retries = configuration.GetValue("WeatherMonitorDB:Retries", 3);

                options.UseNpgsql(connectionString, builder => builder.EnableRetryOnFailure(retries));

                var interceptors = provider.GetServices<IInterceptor>();

                if (interceptors.TryGetNonEnumeratedCount(out var count) && count > 0)
                {
                    options.AddInterceptors(interceptors);
                }
            });

            return services;
        }

        internal IServiceCollection AddScheduledJobs(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(WeatherMonitorProcessorOptions.SectionName);

            services.AddOptions<WeatherMonitorProcessorOptions>()
                .Bind(section);

            var jobOptions = section.Get<WeatherMonitorProcessorOptions>() ?? WeatherMonitorProcessorOptions.Default();

            services.AddTickerQ(options =>
            {
                options.AddDashboard();
                options.AddOperationalStore(ef => ef.UseApplicationDbContext<AppDbContext>(ConfigurationType.IgnoreModelCustomizer));
            });

            services.MapTicker<WeatherMonitorProcessor>()
                .WithCron(jobOptions.ProcessorCronExpression)
                .WithMaxConcurrency(jobOptions.MaxConcurrency);

            services.MapTicker<WebhookMonitorDispatcher, WebhookDeliveryEnvelope>();

            return services;
        }
    }
}