using FluentValidation;

namespace WeatherMonitor.Api.Features.GetMonitorById;

internal sealed class GetMonitorByIdCommandValidator : AbstractValidator<GetMonitorByIdRequest>
{
    public GetMonitorByIdCommandValidator()
    {
        RuleFor(request => request.MonitorId)
            .NotEmpty()
            .WithMessage("must not be empty")
            .NotNull()
            .WithMessage("must not be null");
    }
}