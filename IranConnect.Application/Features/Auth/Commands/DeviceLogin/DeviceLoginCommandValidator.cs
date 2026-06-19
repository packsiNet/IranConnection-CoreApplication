using FluentValidation;

namespace IranConnect.Application.Features.Auth.Commands.DeviceLogin;

public class DeviceLoginCommandValidator
    : AbstractValidator<DeviceLoginCommand>
{
    public DeviceLoginCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .MaximumLength(128)
            .WithMessage("شناسه دستگاه الزامی است");
    }
}
