using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetWeatherConditions;

[UsedImplicitly]
internal record GetWeatherConditionsRequest(
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "size")] int Size = 10
) : IRequest<(IEnumerable<WeatherConditionResponse>, PagedResponse)>;