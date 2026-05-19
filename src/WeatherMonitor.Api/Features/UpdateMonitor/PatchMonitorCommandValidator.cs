using FluentValidation;

namespace WeatherMonitor.Api.Features.UpdateMonitor;

internal sealed class PatchMonitorCommandValidator : AbstractValidator<PatchMonitorRequest>
{
    public PatchMonitorCommandValidator()
    {
        RuleFor(request => request.ClientId)
            .NotEmpty()
            .WithMessage("must not be empty");

        RuleFor(request => request.MonitorId)
            .NotEmpty()
            .WithMessage("must not be empty");

        RuleFor(request => request)
            .Must(HaveAtLeastOnePatchableProperty)
            .WithMessage("must include at least one property to patch")
            .OverridePropertyName("body");

        When(request => request.WebhookUrl is not null, () =>
        {
            RuleFor(request => request.WebhookUrl!)
                .NotEmpty()
                .WithMessage("must not be empty")
                .MaximumLength(500)
                .WithMessage("must be at most 500 characters")
                .Must(BeWellFormedAbsoluteUri)
                .WithMessage("must be a well-formed absolute URI")
                .Must(BeHttpOrHttps)
                .WithMessage("must use HTTP or HTTPS scheme");
        });

        When(request => request.AccessToken is not null && !string.IsNullOrWhiteSpace(request.AccessToken), () =>
        {
            RuleFor(request => request.AccessToken!)
                .MaximumLength(500)
                .WithMessage("must be at most 500 characters");
        });

        When(request => request.TimeZoneId is not null, () =>
        {
            RuleFor(request => request.TimeZoneId!)
                .NotEmpty()
                .WithMessage("must not be empty")
                .MaximumLength(50)
                .WithMessage("must be at most 50 characters")
                .Must(BeKnownTimeZoneId)
                .WithMessage("must be a valid time zone ID");
        });
    }

    private static bool HaveAtLeastOnePatchableProperty(PatchMonitorRequest request)
    {
        return request.WebhookUrl is not null ||
               request.AccessToken is not null ||
               request.Time is not null ||
               request.TimeZoneId is not null ||
               request.Enabled is not null;
    }

    private static bool BeWellFormedAbsoluteUri(string value) => Uri.IsWellFormedUriString(value, UriKind.Absolute);

    private static bool BeHttpOrHttps(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                                                       (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static bool BeKnownTimeZoneId(string value) => TimeZoneInfo.TryFindSystemTimeZoneById(value, out _);
}