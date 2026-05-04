using System.Text.Json.Serialization;

namespace WeatherMonitor.Api.Infrastructure.Clients.Responses;

public sealed class CityResponseModel
{
    [JsonPropertyName("id")]
    int Id { get; set; }
    
    [JsonPropertyName("name")] 
    string Name  { get; set; } = string.Empty;
    
    [JsonPropertyName("estado")] 
    string State { get; set; } = string.Empty;
    
    [JsonPropertyName("regiao")] 
    string Region { get; set; } = string.Empty;
}