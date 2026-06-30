using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Subscription.Commands.RemoveAds;

public class RemoveAdsCommandHandler
    : IRequestHandler<RemoveAdsCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RemoveAdsCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        RemoveAdsCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(
                s => s.UserId == request.UserId,
                cancellationToken);

        if (subscription is null)
            return Result<string>.Failure("اشتراک یافت نشد", 404);

        if (!subscription.ShowAds)
            return Result<string>.Failure("تبلیغات قبلاً برای این حساب حذف شده است", 409);

        subscription.RemoveAds();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("تبلیغات با موفقیت از حساب شما حذف شد");
    }
}
