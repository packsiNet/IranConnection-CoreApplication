using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Reviews.Commands.SubmitReview;

public record SubmitReviewCommand(
    string FullName,
    int Rating,
    string? Comment,
    string? IpAddress
) : IRequest<Result<string>>;
