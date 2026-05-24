using System.ComponentModel.DataAnnotations;

namespace WeatherMonitor.Api.Infrastructure.Clients.Options;

public record CachingHandlerOptions
{
    [Required]
    public required TimeSpan Expiration { get; init; } = TimeSpan.FromHours(12);
}