using System.Text.Json.Serialization;

namespace WeatherMonitor.Api.Infrastructure.Clients.Responses;

public sealed class ForecastResponseModel
{
    [JsonPropertyName("cidade")]
    public string City { get; set; } = string.Empty;
    
    [JsonPropertyName("estado")]
    public string State { get; set; } = string.Empty;
    
    [JsonPropertyName("atualizado_em")]
    public DateTime UpdatedAt { get; set; } = new();
    
    [JsonPropertyName("clima")]
    public List<WeatherModel> Weather { get; set; } = [];
}