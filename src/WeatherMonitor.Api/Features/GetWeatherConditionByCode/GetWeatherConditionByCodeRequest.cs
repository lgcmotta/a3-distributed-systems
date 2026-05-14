using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.Features.GetWeatherConditions;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

[UsedImplicitly]
internal record GetWeatherConditionByCodeRequest(
    [property: FromQuery(Name = "code")] string Code
) : IRequest<WeatherConditionByCodeResponse>;