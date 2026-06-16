using FluentValidation;

namespace IranConnect.Application.Features.Subscription.Commands.UpgradeSubscription;

public class UpgradeSubscriptionCommandValidator
    : AbstractValidator<UpgradeSubscriptionCommand>
{
    public UpgradeSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر الزامی است");

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("مدت اشتراک باید بیشتر از صفر باشد")
            .LessThanOrEqualTo(365).WithMessage("مدت اشتراک نمی‌تواند بیشتر از ۳۶۵ روز باشد");
    }
}
