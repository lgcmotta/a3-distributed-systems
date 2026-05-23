using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.GetDeliveries;

[UsedImplicitly]
internal record GetDeliveriesRequest(
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "size")] int Size = 10,
    [property: FromQuery(Name = "start")] DateOnly? Start = null,
    [property: FromQuery(Name = "end")] DateOnly? End = null
) : IRequest<(DeliveryResponse[], PagedResponse)>
{
    internal required string ClientId { get; init; } = string.Empty;
}