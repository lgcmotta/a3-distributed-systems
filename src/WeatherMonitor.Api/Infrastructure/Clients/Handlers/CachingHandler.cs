using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mime;
using WeatherMonitor.Api.Infrastructure.Clients.Options;

namespace WeatherMonitor.Api.Infrastructure.Clients.Handlers;

internal sealed class CachingHandler(IOptionsSnapshot<CachingHandlerOptions> options, HybridCache cache) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var cacheKey = $"{request.Method}:{request.RequestUri}";

        HttpResponseMessage? upstreamResponse = null;

        var cacheOptions = new HybridCacheEntryOptions { Expiration = options.Value.Expiration };

        var cachedBody = await cache.GetOrCreateAsync<string?>(
            cacheKey,
            factory: async token =>
            {
                var response = await base.SendAsync(request, token);

                if (response is not { StatusCode: HttpStatusCode.OK, Content.Headers.ContentType.MediaType: MediaTypeNames.Application.Json })
                {
                    upstreamResponse = response;
                    return null;
                }

                using (response)
                {
                    return await response.Content.ReadAsStringAsync(token);
                }
            },
            options: cacheOptions,
            cancellationToken: cancellationToken);

        if (cachedBody is null)
        {
            return upstreamResponse ?? await base.SendAsync(request, cancellationToken);
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(cachedBody, System.Text.Encoding.UTF8, mediaType: MediaTypeNames.Application.Json),
            RequestMessage = request,
        };
    }
}