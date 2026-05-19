using FluentValidation;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

internal sealed class GetWeatherConditionByCodeQueryValidator : AbstractValidator<WeatherConditionRequest>
{
    public GetWeatherConditionByCodeQueryValidator()
    {
        RuleFor(query => query.Code)
            .NotEmpty()
            .NotNull()
            .WithMessage("Code cannot be empty or null");
    }
}