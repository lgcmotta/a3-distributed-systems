using Asp.Versioning;
using FluentValidation;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;
using Microsoft.EntityFrameworkCore;
using Refit;
using System.Reflection;
using WeatherMonitor.Api.Behaviors;
using WeatherMonitor.Api.Diagnostics;
using WeatherMonitor.Api.Infrastructure.Clients;
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
                .AddRefitClient<ICptecRefitApi>()
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

        internal IServiceCollection AddTimeProvider()
        {
            return services.AddSingleton(TimeProvider.System);
        }

        internal IServiceCollection AddAppDbContext()
        {
            services.AddDbContext<AppDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();

                var connectionString = configuration.GetConnectionString("WeatherMonitorDB");

                options.UseNpgsql(connectionString, builder => builder.EnableRetryOnFailure(3));

                var interceptors = InterceptorAssemblyScanner.Scan(provider, Assembly.GetCallingAssembly());

                if (interceptors is { Length: 0 })
                {
                    return;
                }

                options.AddInterceptors(interceptors);
            });

            return services;
        }
    }
}