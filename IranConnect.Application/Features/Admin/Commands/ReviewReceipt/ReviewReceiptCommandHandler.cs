using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using IranConnect.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainSubscription = IranConnect.Domain.Entities.Subscription;

namespace IranConnect.Application.Features.Admin.Commands.ReviewReceipt;

public class ReviewReceiptCommandHandler
    : IRequestHandler<ReviewReceiptCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public ReviewReceiptCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        ReviewReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var receipt = await _context.PaymentReceipts
            .FirstOrDefaultAsync(r => r.Id == request.ReceiptId, cancellationToken);

        if (receipt is null)
            return Result<string>.Failure("رسید پرداخت یافت نشد", 404);

        if (receipt.Status != PaymentReceiptStatus.Pending)
            return Result<string>.Failure("این رسید قبلاً بررسی شده است", 409);

        if (!request.Approved)
        {
            receipt.Reject(request.AdminId, request.Note);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success("رسید رد شد");
        }

        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == receipt.UserId, cancellationToken);

        if (user is null)
            return Result<string>.Failure("کاربر یافت نشد", 404);

        string successMessage;

        if (receipt.ReceiptType == PaymentReceiptType.AdsRemoval)
        {
            if (user.Subscription is null)
            {
                var subscription = DomainSubscription.CreateFree(user.Id);
                subscription.RemoveAds();
                _context.Subscriptions.Add(subscription);
                user.AttachSubscription(subscription);
            }
            else
            {
                user.Subscription.RemoveAds();
            }
            successMessage = "رسید تأیید شد و تبلیغات از حساب کاربر حذف گردید";
        }
        else
        {
            if (user.Subscription is null)
            {
                var subscription = DomainSubscription.CreatePremium(user.Id, receipt.RequestedDurationDays);
                _context.Subscriptions.Add(subscription);
                user.AttachSubscription(subscription);
            }
            else
            {
                user.Subscription.Upgrade(receipt.RequestedDurationDays);
            }
            successMessage = $"رسید تأیید شد و اشتراک کاربر برای {receipt.RequestedDurationDays} روز تمدید گردید";
        }

        receipt.Approve(request.AdminId);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(successMessage);
    }
}
