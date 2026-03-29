using Asp.Versioning;
using WeatherMonitor.Api.Extensions;

var v1 = new ApiVersion(majorVersion: 1, minorVersion: 0);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPermissiveCors();
builder.Services.AddApiVersioning(v1);
builder.Services.AddKeycloak(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApiDocument(builder.Configuration);
}

var app = builder.Build();

app.UseRouting();
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