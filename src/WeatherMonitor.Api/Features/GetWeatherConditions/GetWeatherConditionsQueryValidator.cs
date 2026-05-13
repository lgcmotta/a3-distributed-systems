using FluentValidation;

namespace WeatherMonitor.Api.Features.GetWeatherConditions;

internal sealed class GetWeatherConditionsQueryValidator : AbstractValidator<GetWeatherConditionsRequest>
{
    public GetWeatherConditionsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("must be greater than 0");

        RuleFor(query => query.Size)
            .GreaterThanOrEqualTo(1)
            .WithMessage("must be greater than 0")
            .LessThanOrEqualTo(50)
            .WithMessage("must be less than or equal to 50");
    }
}