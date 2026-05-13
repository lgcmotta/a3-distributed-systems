using MediatR;
using Microsoft.EntityFrameworkCore;
using Refit;
using WeatherMonitor.Api.Infrastructure.Clients.Exceptions;
using WeatherMonitor.Api.Infrastructure.Clients.Interfaces;
using WeatherMonitor.Api.Infrastructure.Clients.Responses;
using WeatherMonitor.Api.Infrastructure.Persistence;
using WeatherMonitor.Domain.Monitors;
using WeatherMonitor.Domain.Monitors.Exceptions;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace WeatherMonitor.Api.Features.CreateMonitor;

internal partial class CreateMonitorCommandHandler(
    ILogger<CreateMonitorCommandHandler> logger,
    IBrasilApiClient api,
    AppDbContext context
) : IRequestHandler<CreateMonitorRequest, MonitorResponse>
{
    public async Task<MonitorResponse> Handle(CreateMonitorRequest request, CancellationToken cancellationToken)
    {
        var citiesResponse = await api.SearchCitiesAsync(request.City, cancellationToken);

        if (citiesResponse is { IsSuccessStatusCode: false })
        {
            LogErrorCityLookupUnavailable(request.City);

            throw new CityLookupUnavailableException(request.City);
        }

        if (citiesResponse is { Content: null })
        {
            LogErrorCityLookupFailed(request.City);

            throw new CityLookupFailedException(request.City);
        }

        var city = citiesResponse.FindCityOrDefault(request.City, request.State);

        if (city is null)
        {
            LogErrorFoundCitiesNotMatchedRequest(request.City, request.State);

            throw new MonitorCityNotFoundException(request.City, request.State);
        }

        if (await context.HasDuplicateMonitorAsync(request, city.Id, cancellationToken))
        {
            LogErrorDuplicatedMonitorAttempt(request.ClientId, request.City, request.State, request.Time, request.TimeZoneId);

            throw new DuplicateWeatherMonitorException(request.City, request.State, request.Time, request.TimeZoneId);
        }

        var monitor = new WeatherMonitorConfiguration(
            request.ClientId,
            city.Id,
            city.Name,
            city.State,
            request.WeatherConditionCode,
            request.WebhookUrl,
            request.Time,
            request.TimeZoneId,
            request.AccessToken
        );

        await context.Monitors.AddAsync(monitor, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new MonitorResponse
        {
            MonitorId = monitor.Id,
            CityCode = monitor.Location.CityCode,
            CityName = monitor.Location.CityName,
            State = monitor.Location.State.Value,
            WeatherConditionCode = monitor.WeatherCondition.Code,
            WeatherConditionDescription = monitor.WeatherCondition.Description,
            WebhookUrl = monitor.Webhook.Url,
            Time = monitor.Webhook.ScheduleFor,
            TimeZoneId = monitor.Webhook.TimeZoneId,
            Enabled = monitor.Enabled
        };
    }


    [LoggerMessage(Level = LogLevel.Error,
        Message = "Unable to resolve city {CityName} because the city lookup service returned an unsuccessful response")]
    private partial void LogErrorCityLookupUnavailable(string cityName);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Unable to resolve city {CityName} because the city lookup response did not include city data")]
    private partial void LogErrorCityLookupFailed(string cityName);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "None of the found cities with name {CityName} appears to belong to state {State}")]
    private partial void LogErrorFoundCitiesNotMatchedRequest(string cityName, string state);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Duplicated monitor prevent. Clint ID {ClientId} already has a monitor configured for {CityName}, {State}, {Time} and {TimeZoneId}")]
    private partial void LogErrorDuplicatedMonitorAttempt(string clientId, string cityName, string state, TimeOnly time, string timeZoneId);
}

file static class CreateMonitorCommandHandlerExtensions
{
    extension(AppDbContext context)
    {
        internal Task<bool> HasDuplicateMonitorAsync(CreateMonitorRequest request, int cityCode, CancellationToken cancellationToken = default)
        {
            return context.Monitors.AnyAsync(
                monitor => monitor.ClientId == request.ClientId &&
                           monitor.Location.CityCode == cityCode &&
                           monitor.Webhook.ScheduleFor == request.Time &&
                           monitor.Webhook.TimeZoneId == request.TimeZoneId,
                cancellationToken);
        }
    }

    extension(IApiResponse<IEnumerable<BrasilApiCity>> cities)
    {
        internal BrasilApiCity? FindCityOrDefault(string cityName, string state)
        {
            return cities.Content?.FirstOrDefault(city => city.Name.Equals(cityName, StringComparison.InvariantCultureIgnoreCase) &&
                                                          city.State.Equals(state, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}