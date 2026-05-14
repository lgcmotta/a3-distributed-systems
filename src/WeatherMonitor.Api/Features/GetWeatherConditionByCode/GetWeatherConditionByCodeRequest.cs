using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

[UsedImplicitly]
internal record GetWeatherConditionByCodeRequest(
    [property: FromRoute(Name = "code")] string Code
) : IRequest<WeatherConditionByCodeResponse>;