using FluentValidation;

namespace IranConnect.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandValidator
    : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد الزامی است")
            .Matches(@"^\d{6}$").WithMessage("کد باید ۶ رقم باشد");
    }
}
