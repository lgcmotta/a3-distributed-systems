using Asp.Versioning;
using FluentValidation;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;
using System.Reflection;
using WeatherMonitor.Api.Behaviors;
using WeatherMonitor.Api.Diagnostics;
using WeatherMonitor.Api.Infrastructure.Keycloak;
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
    }
}