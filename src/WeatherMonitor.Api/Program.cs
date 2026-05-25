using Asp.Versioning;
using WeatherMonitor.Api.Extensions;
using WeatherMonitor.Api.Features.CreateMonitor;
using WeatherMonitor.Api.Features.GetDeliveries;
using WeatherMonitor.Api.Features.GetMonitorById;
using WeatherMonitor.Api.Features.GetMonitors;
using WeatherMonitor.Api.Features.GetWeatherConditionByCode;
using WeatherMonitor.Api.Features.GetWeatherConditions;
using WeatherMonitor.Api.Features.UpdateMonitor;
using WeatherMonitor.ServiceDefaults.Extensions;

var v1 = new ApiVersion(majorVersion: 1, minorVersion: 0);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandling();
builder.Services.AddPermissiveCors();
builder.Services.AddApiVersioning(v1);
builder.Services.AddKeycloak(builder.Configuration);
builder.Services.AddCQRS();
builder.Services.AddCaching(builder.Configuration);
builder.Services.AddBrasilApiClient(builder.Configuration);
builder.Services.AddWebhookDispatcherHttpClient(builder.Configuration);
builder.Services.AddTimeProvider();
builder.Services.AddAppDbContext();
builder.Services.AddScheduledJobs(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApiDocument(builder.Configuration);
}

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseRouting();
app.UseGlobalExceptionHandler();
app.UsePermissiveCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapOpenApiUI();
}

var api = app.MapApiGroup();

api.MapGetDeliveries(v1);
api.MapGetWeatherConditionCodes(v1);
api.MapGetWeatherConditionByCode(v1);
api.MapGetMonitorById(v1);
api.MapGetMonitors(v1);
api.MapPostCreateMonitor(v1);
api.MapPatchUpdateMonitor(v1);

app.UseScheduledJobs();

await app.RunAsync();