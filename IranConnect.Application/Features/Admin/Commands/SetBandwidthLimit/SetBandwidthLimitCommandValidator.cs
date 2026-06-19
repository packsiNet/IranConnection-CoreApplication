using FluentValidation;

namespace IranConnect.Application.Features.Admin.Commands.SetBandwidthLimit;

public class SetBandwidthLimitCommandValidator
    : AbstractValidator<SetBandwidthLimitCommand>
{
    public SetBandwidthLimitCommandValidator()
    {
        RuleFor(x => x.LimitBytes)
            .GreaterThan(0)
            .When(x => x.LimitBytes.HasValue)
            .WithMessage("Bandwidth limit must be greater than zero");
    }
}
