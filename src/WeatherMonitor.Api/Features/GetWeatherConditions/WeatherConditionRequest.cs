using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.GetWeatherConditions;

[UsedImplicitly]
internal record WeatherConditionRequest(
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "size")] int Size = 10
) : IRequest<(WeatherConditionResponse[], PagedResponse)>;