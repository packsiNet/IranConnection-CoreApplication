using FluentValidation;

namespace IranConnect.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidator
    : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد الزامی است")
            .Matches(@"^\d{6}$").WithMessage("کد باید ۶ رقم باشد");
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[0-9]");
    }
}
