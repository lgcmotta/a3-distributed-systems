namespace AppHost.Extensions;

internal static class RedisResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<RedisResource> AddRedisContainer()
        {
            return builder.AddRedis(name: "Redis", port: 6379)
                .WithImageRegistry(registry: "mirror.gcr.io")
                .WithImage(image: "redis", tag: "latest")
                .WithContainerName(name: "redis")
                .WithLifetime(lifetime: ContainerLifetime.Persistent)
                .WithDataVolume("redis_data")
                .WithOtlpExporter();
        }
    }
}