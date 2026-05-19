using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

[UsedImplicitly]
internal record WeatherConditionRequest(
    [property: FromRoute(Name = "code")] string Code
) : IRequest<WeatherConditionResponse>;