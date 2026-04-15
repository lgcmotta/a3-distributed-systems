using Asp.Versioning;
using WeatherMonitor.Api.Extensions;
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

await app.RunAsync();