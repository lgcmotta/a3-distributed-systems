using MediatR;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Monitors.Exceptions;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

internal sealed partial class GetWeatherConditionByCodeQueryHandler(ILogger<GetWeatherConditionByCodeQueryHandler> logger) : IRequestHandler<WeatherConditionRequest, WeatherConditionResponse>
{
    public Task<WeatherConditionResponse> Handle(WeatherConditionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var weatherCondition = Enumeration.Enumerate<WeatherCondition>()
            .FirstOrDefault(condition => condition.Code == request.Code);

        if (weatherCondition is not null)
            return Task.FromResult(
                new WeatherConditionResponse(weatherCondition.Code, weatherCondition.Description));

        LogErrorWeatherConditionCodeNotFound(request.Code);
        throw new WeatherConditionCodeNotFoundException(request.Code);
    }

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Weather condition not found for code: {code}")]
    private partial void LogErrorWeatherConditionCodeNotFound(string code);
}