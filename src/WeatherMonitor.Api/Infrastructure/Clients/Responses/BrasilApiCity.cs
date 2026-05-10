using System.Text.Json.Serialization;

namespace WeatherMonitor.Api.Infrastructure.Clients.Responses;

public sealed record BrasilApiCity
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("nome")]
    public string Name  { get; init; } = string.Empty;
    [JsonPropertyName("estado")]
    public string State { get; init; } = string.Empty;
    [JsonPropertyName("regiao")]
    public string Region { get; init; } = string.Empty;
}