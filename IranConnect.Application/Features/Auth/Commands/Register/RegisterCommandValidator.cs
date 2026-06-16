using FluentValidation;

namespace IranConnect.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است")
            .EmailAddress().WithMessage("فرمت ایمیل نامعتبر است")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("پسورد الزامی است")
            .MinimumLength(8).WithMessage("پسورد باید حداقل ۸ کاراکتر باشد")
            .Matches("[A-Z]").WithMessage("پسورد باید حداقل یک حرف بزرگ داشته باشد")
            .Matches("[0-9]").WithMessage("پسورد باید حداقل یک عدد داشته باشد");

        RuleFor(x => x.FullName)
            .MaximumLength(128).When(x => x.FullName != null);
    }
}
