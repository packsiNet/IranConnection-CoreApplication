using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.ReviewReceipt;

public record ReviewReceiptCommand(
    Guid ReceiptId,
    Guid AdminId,
    bool Approved,
    string? Note = null
) : IRequest<Result<string>>;
