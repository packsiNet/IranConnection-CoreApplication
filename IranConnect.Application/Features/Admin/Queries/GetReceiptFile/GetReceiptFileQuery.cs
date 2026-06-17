using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetReceiptFile;

public record GetReceiptFileQuery(Guid ReceiptId) : IRequest<Result<ReceiptFileInfo>>;

public record ReceiptFileInfo(string StoredFileName, string OriginalFileName);
