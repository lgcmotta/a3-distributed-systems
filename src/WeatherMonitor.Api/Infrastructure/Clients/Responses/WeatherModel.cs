using System.Text.Json.Serialization;

namespace WeatherMonitor.Api.Infrastructure.Clients.Responses;

public sealed class WeatherModel
{
    [JsonPropertyName("data")]
    public DateTime Date { get; set; } = new();
    
    [JsonPropertyName("condicao")]
    public string Condition { get; set; } = string.Empty;
    
    [JsonPropertyName("min")]
    public int Min { get; set; }
    
    [JsonPropertyName("max")]
    public int Max { get; set; }
    
    [JsonPropertyName("indice_uv")]
    public int UvIndex { get; set; }
    
    [JsonPropertyName("condition_desc")]
    public string Description { get; set; } =  string.Empty;
}