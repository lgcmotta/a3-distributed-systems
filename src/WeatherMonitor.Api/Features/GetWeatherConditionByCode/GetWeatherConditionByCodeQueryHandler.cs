using MediatR;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Deliveries.ValueObjects;
using WeatherMonitor.Domain.Monitors.Exceptions;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

internal sealed partial class GetWeatherConditionByCodeQueryHandler(ILogger<GetWeatherConditionByCodeQueryHandler> logger) : IRequestHandler<GetWeatherConditionByCodeRequest, WeatherConditionByCodeResponse>
{
    public Task<WeatherConditionByCodeResponse> Handle(GetWeatherConditionByCodeRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(request.Code);

        var weatherCondition = Enumeration.Enumerate<WeatherCondition>()
            .FirstOrDefault(condition => condition.Code == request.Code);

        if (weatherCondition is not null)
        {
            return Task.FromResult(
                new WeatherConditionByCodeResponse(weatherCondition.Code, weatherCondition.Description));
        }

        LogErrorWeatherConditionCodeNotFound(request.Code);
        throw new WeatherConditionCodeNotFoundException(request.Code);

    }
    
    [LoggerMessage(Level = LogLevel.Error,
        Message = "Weather condition not found for code: {code}")]
    private partial void LogErrorWeatherConditionCodeNotFound(string code);
}