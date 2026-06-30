using FluentValidation;

namespace IranConnect.Application.Features.Reviews.Commands.SubmitReview;

public class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام الزامی است")
            .MaximumLength(150).WithMessage("نام حداکثر ۱۵۰ کاراکتر می‌تواند باشد");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("امتیاز باید بین ۱ تا ۵ باشد");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("متن نظر حداکثر ۱۰۰۰ کاراکتر می‌تواند باشد")
            .When(x => x.Comment is not null);
    }
}
