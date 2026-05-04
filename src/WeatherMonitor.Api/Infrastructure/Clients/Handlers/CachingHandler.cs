using Microsoft.Extensions.Caching.Hybrid;

namespace WeatherMonitor.Api.Infrastructure.Clients.Handlers;

internal sealed class CachingHandler(HybridCache cache) : DelegatingHandler
{
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(1);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
            return await base.SendAsync(request, cancellationToken);

        var cacheKey = $"{request.Method}:{request.RequestUri}";

        var cachedBody = await cache.GetOrCreateAsync<string?>(
            cacheKey,
            async ct =>
            {
                var response = await base.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsStringAsync(ct);
            },
            new HybridCacheEntryOptions() { Expiration = Ttl },
            cancellationToken: cancellationToken);
        
        if (cachedBody is null)
            return await base.SendAsync(request, cancellationToken);
        
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(cachedBody, System.Text.Encoding.UTF8, "application/json"),
            RequestMessage = request,
        };
    }
}