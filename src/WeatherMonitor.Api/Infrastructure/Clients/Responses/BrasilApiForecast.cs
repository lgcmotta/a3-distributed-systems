using System.Text.Json.Serialization;

namespace WeatherMonitor.Api.Infrastructure.Clients.Responses;

public sealed record BrasilApiForecast
{
    [JsonPropertyName("cidade")]
    public string City { get; init; } = string.Empty;

    [JsonPropertyName("estado")]
    public string State { get; init; } = string.Empty;

    [JsonPropertyName("atualizado_em")]
    public DateTime UpdatedAt { get; init; } = new();

    [JsonPropertyName("clima")]
    public List<BrasilApiWeather> Weather { get; init; } = [];
}