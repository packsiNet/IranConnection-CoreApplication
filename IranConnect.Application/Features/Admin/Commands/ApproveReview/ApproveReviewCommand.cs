using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.ApproveReview;

public record ApproveReviewCommand(Guid ReviewId, bool Approve) : IRequest<Result<string>>;
