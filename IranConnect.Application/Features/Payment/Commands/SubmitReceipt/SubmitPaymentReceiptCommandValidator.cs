using FluentValidation;

namespace IranConnect.Application.Features.Payment.Commands.SubmitReceipt;

public class SubmitPaymentReceiptCommandValidator
    : AbstractValidator<SubmitPaymentReceiptCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg", "image/jpg", "image/png", "application/pdf"
    ];

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public SubmitPaymentReceiptCommandValidator()
    {
        RuleFor(x => x.PayerFullName)
            .NotEmpty().WithMessage("نام پرداخت‌کننده الزامی است")
            .MaximumLength(200).WithMessage("نام پرداخت‌کننده حداکثر ۲۰۰ کاراکتر می‌تواند باشد");

        RuleFor(x => x.LastFourDigits)
            .NotEmpty().WithMessage("۴ رقم آخر کارت الزامی است")
            .Length(4).WithMessage("دقیقاً ۴ رقم آخر کارت را وارد کنید")
            .Matches(@"^\d{4}$").WithMessage("۴ رقم آخر کارت باید عدد باشد");

        RuleFor(x => x.FileBytes)
            .NotEmpty().WithMessage("فایل رسید الزامی است")
            .Must(b => b.Length <= MaxFileSizeBytes)
                .WithMessage("حجم فایل نباید بیشتر از ۵ مگابایت باشد");

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
            .WithMessage("فرمت فایل باید JPG، PNG یا PDF باشد");

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("مدت اشتراک باید بیشتر از صفر روز باشد")
            .LessThanOrEqualTo(365).WithMessage("مدت اشتراک نمی‌تواند بیشتر از ۳۶۵ روز باشد");
    }
}
