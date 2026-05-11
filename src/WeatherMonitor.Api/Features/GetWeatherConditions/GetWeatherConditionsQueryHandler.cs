using MediatR;
using WeatherMonitor.Api.Shared;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Api.Features.GetWeatherConditions;

internal sealed class GetWeatherConditionsQueryHandler : IRequestHandler<GetWeatherConditionsQuery, (IEnumerable<WeatherConditionResponse>, PagedResponseModel)>
{
    public Task<(IEnumerable<WeatherConditionResponse>, PagedResponseModel)> Handle(GetWeatherConditionsQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var weatherConditionCodes = Enumeration.Enumerate<WeatherCondition>()
            .OrderBy(weatherCondition => weatherCondition.Code, StringComparer.Ordinal)
            .ToList();

        var total = weatherConditionCodes.Count;

        var totalPages = (int)Math.Ceiling(total / (double)request.Size);

        var response = weatherConditionCodes
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(condition => new WeatherConditionResponse(condition.Code, condition.Description))
            .ToList();

        var pagination = new PagedResponseModel
        {
            Page = request.Page,
            Size = request.Size,
            Previous = request.Page > 1 ? request.Page - 1 : 0,
            Next = request.Page < totalPages ? request.Page + 1 : 0,
            Total = total,
            TotalPages = totalPages
        };

        return Task.FromResult<(IEnumerable<WeatherConditionResponse>, PagedResponseModel)>((response, pagination));
    }
}