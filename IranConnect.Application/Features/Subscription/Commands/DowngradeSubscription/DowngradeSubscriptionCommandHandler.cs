using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Subscription.Commands.DowngradeSubscription;

public class DowngradeSubscriptionCommandHandler
    : IRequestHandler<DowngradeSubscriptionCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public DowngradeSubscriptionCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        DowngradeSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(
                s => s.UserId == request.UserId,
                cancellationToken);

        if (subscription is null)
            return Result<string>.Failure("اشتراک یافت نشد", 404);

        subscription.Expire();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("اشتراک به پلن رایگان تنزل یافت");
    }
}
