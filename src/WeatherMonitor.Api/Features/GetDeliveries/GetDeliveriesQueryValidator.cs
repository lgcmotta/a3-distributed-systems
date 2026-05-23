using FluentValidation;

namespace WeatherMonitor.Api.Features.GetDeliveries;

internal sealed class GetDeliveriesQueryValidator : AbstractValidator<GetDeliveriesRequest>
{
    public GetDeliveriesQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("must be greater than 0");

        RuleFor(query => query.Size)
            .GreaterThanOrEqualTo(1)
            .WithMessage("must be greater than 0")
            .LessThanOrEqualTo(50)
            .WithMessage("must be less than or equal to 50");

        RuleFor(query => query)
            .Must(query => !query.Start.HasValue || !query.End.HasValue || query.Start.Value <= query.End.Value)
            .WithName("DateRange")
            .WithMessage("start must be less than or equal to end");
    }
}