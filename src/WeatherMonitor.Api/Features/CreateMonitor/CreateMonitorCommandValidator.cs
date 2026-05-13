using FluentValidation;
using WeatherMonitor.Api.Extensions;

namespace WeatherMonitor.Api.Features.CreateMonitor;

internal sealed class CreateMonitorCommandValidator : AbstractValidator<CreateMonitorRequest>
{
    public CreateMonitorCommandValidator()
    {
        RuleFor(request => request.City)
            .NotEmpty()
            .WithMessage("must not be empty")
            .MaximumLength(100)
            .WithMessage("must be at most 100 characters");

        RuleFor(request => request.State)
            .NotEmpty()
            .WithMessage("must not be empty")
            .Length(2)
            .WithMessage("must contain exactly 2 characters")
            .BeBrazilianStateAcronym()
            .WithMessage("must be a valid Brazilian state acronym");

        RuleFor(request => request.WeatherConditionCode)
            .NotEmpty()
            .WithMessage("must not be empty")
            .MaximumLength(3)
            .WithMessage("must be at most 3 characters")
            .Must(BeAsciiLetters)
            .WithMessage("must contain only ASCII letters");

        RuleFor(request => request.WebhookUrl)
            .NotEmpty()
            .WithMessage("must not be empty")
            .MaximumLength(500)
            .WithMessage("must be at most 500 characters")
            .Must(BeWellFormedAbsoluteUri)
            .WithMessage("must be a well-formed absolute URI")
            .Must(BeHttpOrHttps)
            .WithMessage("must use HTTP or HTTPS scheme");

        RuleFor(request => request.TimeZoneId)
            .NotEmpty()
            .WithMessage("must not be empty")
            .MaximumLength(50)
            .WithMessage("must be at most 50 characters")
            .Must(BeKnownTimeZoneId)
            .WithMessage("must be a valid time zone ID");

        RuleFor(request => request.AccessToken)
            .Must(accessToken => accessToken is null || !string.IsNullOrWhiteSpace(accessToken))
            .WithMessage("must not be empty when provided")
            .MaximumLength(500)
            .WithMessage("must be at most 500 characters");
    }

    private static bool BeAsciiLetters(string value) => value.All(char.IsAsciiLetter);

    private static bool BeWellFormedAbsoluteUri(string value) => Uri.IsWellFormedUriString(value, UriKind.Absolute);

    private static bool BeHttpOrHttps(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                                                       (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static bool BeKnownTimeZoneId(string value) => TimeZoneInfo.TryFindSystemTimeZoneById(value, out _);
}