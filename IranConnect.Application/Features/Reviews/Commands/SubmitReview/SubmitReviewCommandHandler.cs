using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using DomainReview = IranConnect.Domain.Entities.Review;

namespace IranConnect.Application.Features.Reviews.Commands.SubmitReview;

public class SubmitReviewCommandHandler
    : IRequestHandler<SubmitReviewCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICountryLookupService _countryLookup;

    public SubmitReviewCommandHandler(
        IApplicationDbContext context,
        ICountryLookupService countryLookup)
    {
        _context = context;
        _countryLookup = countryLookup;
    }

    public async Task<Result<string>> Handle(
        SubmitReviewCommand request,
        CancellationToken cancellationToken)
    {
        string? countryCode = null;
        if (!string.IsNullOrWhiteSpace(request.IpAddress))
        {
            countryCode = await _countryLookup.GetCountryCodeAsync(
                request.IpAddress, cancellationToken);
        }

        var review = DomainReview.Create(
            request.FullName,
            request.Rating,
            request.Comment,
            request.IpAddress,
            countryCode);

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(
            "نظر شما با موفقیت ثبت شد و پس از تأیید مدیر نمایش داده خواهد شد", 201);
    }
}
