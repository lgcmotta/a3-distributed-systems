using MediatR;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Api.Features.GetWeatherConditions;

internal sealed class GetWeatherConditionsQueryHandler : IRequestHandler<WeatherConditionRequest, (WeatherConditionResponse[], PagedResponse)>
{
    public Task<(WeatherConditionResponse[], PagedResponse)> Handle(WeatherConditionRequest request, CancellationToken cancellationToken)
    {
        var weatherConditionCodes = Enumeration.Enumerate<WeatherCondition>()
            .OrderBy(weatherCondition => weatherCondition.Code, StringComparer.Ordinal)
            .ToArray();

        var total = weatherConditionCodes.Length;

        var totalPages = (int)Math.Ceiling(total / (double)request.Size);

        var response = weatherConditionCodes
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(condition => new WeatherConditionResponse(condition.Code, condition.Description))
            .ToArray();

        var pagination = new PagedResponse
        {
            Page = request.Page,
            Size = request.Size,
            Previous = request.Page > 1 ? request.Page - 1 : 0,
            Next = request.Page < totalPages ? request.Page + 1 : 0,
            Total = total,
            TotalPages = totalPages
        };

        return Task.FromResult<(WeatherConditionResponse[], PagedResponse)>((response, pagination));
    }
}