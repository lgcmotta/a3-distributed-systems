using Microsoft.Extensions.Caching.Hybrid;
using System.Net;
using System.Net.Mime;

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
        HttpResponseMessage? upstreamResponse = null;

        var cachedBody = await cache.GetOrCreateAsync<string?>(
            cacheKey,
            async ct =>
            {
                upstreamResponse = await base.SendAsync(request, ct);

                if (upstreamResponse.StatusCode != HttpStatusCode.OK)
                    return null;

                if (upstreamResponse.Content.Headers.ContentType?.MediaType != MediaTypeNames.Application.Json)
                    return null;

                return await upstreamResponse.Content.ReadAsStringAsync(ct);
            },
            new HybridCacheEntryOptions { Expiration = Ttl },
            cancellationToken: cancellationToken);

        if (cachedBody is null)
        {
            return upstreamResponse ?? await base.SendAsync(request, cancellationToken);
        }

        upstreamResponse?.Dispose();

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(cachedBody, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json),
            RequestMessage = request,
        };
    }
}