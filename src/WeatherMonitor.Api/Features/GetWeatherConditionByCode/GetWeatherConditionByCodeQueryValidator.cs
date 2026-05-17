using FluentValidation;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

internal sealed class GetWeatherConditionByCodeQueryValidator : AbstractValidator<GetWeatherConditionByCodeRequest>
{
    public GetWeatherConditionByCodeQueryValidator()
    {
        RuleFor(query => query.Code)
            .NotEmpty()
            .NotNull()
            .WithMessage("Code cannot be empty or null");
    }
}