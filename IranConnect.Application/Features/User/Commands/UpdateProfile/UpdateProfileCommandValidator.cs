using FluentValidation;

namespace IranConnect.Application.Features.User.Commands.UpdateProfile;

public class UpdateProfileCommandValidator
    : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(128)
            .When(x => x.FullName is not null);

        RuleFor(x => x.NewPassword)
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[0-9]")
            .When(x => x.NewPassword is not null)
            .WithMessage("پسورد جدید باید حداقل ۸ کاراکتر، یک حرف بزرگ و یک عدد داشته باشد");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .When(x => x.NewPassword is not null)
            .WithMessage("برای تغییر پسورد، پسورد فعلی الزامی است");
    }
}
