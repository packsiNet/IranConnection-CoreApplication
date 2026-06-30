using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Admin.Commands.ApproveReview;

public class ApproveReviewCommandHandler
    : IRequestHandler<ApproveReviewCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public ApproveReviewCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        ApproveReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review is null)
            return Result<string>.Failure("نظر یافت نشد", 404);

        if (request.Approve)
            review.Approve();
        else
            review.Reject();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(request.Approve ? "نظر تأیید شد" : "نظر رد شد");
    }
}
