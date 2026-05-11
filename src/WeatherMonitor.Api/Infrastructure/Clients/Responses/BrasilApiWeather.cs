using System.Text.Json.Serialization;

namespace WeatherMonitor.Api.Infrastructure.Clients.Responses;

public sealed record BrasilApiWeather
{
    [JsonPropertyName("data")]
    public DateTime Date { get; init; } = new();

    [JsonPropertyName("condicao")]
    public string Condition { get; init; } = string.Empty;

    [JsonPropertyName("min")]
    public int Min { get; init; } = 0;

    [JsonPropertyName("max")]
    public int Max { get; init; } = 0;

    [JsonPropertyName("indice_uv")]
    public int UvIndex { get; init; } = 0;

    [JsonPropertyName("condition_desc")]
    public string Description { get; init; } = string.Empty;
}