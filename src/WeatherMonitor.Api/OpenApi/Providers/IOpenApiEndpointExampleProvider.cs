namespace WeatherMonitor.Api.OpenApi.Providers;

internal interface IOpenApiEndpointExampleProvider
{
    OpenApiMediaTypeExample? CreateBodyExample();
    OpenApiMediaTypeExample? Create200OkExample();
    OpenApiMediaTypeExample? Create201CreatedExample();
    OpenApiMediaTypeExample? Create400BadRequestExample(string? path);
    OpenApiMediaTypeExample? Create404NotFoundExample(string? path);
    OpenApiMediaTypeExample? Create409ConflictExample(string? path);
    OpenApiMediaTypeExample? Create500InternalServerErrorExample(string? path);
    OpenApiMediaTypeExample? Create503ServiceUnavailableExample(string? path);
}