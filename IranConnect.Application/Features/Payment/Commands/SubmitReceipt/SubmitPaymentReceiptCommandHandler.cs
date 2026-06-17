using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainPaymentReceipt = IranConnect.Domain.Entities.PaymentReceipt;

namespace IranConnect.Application.Features.Payment.Commands.SubmitReceipt;

public class SubmitPaymentReceiptCommandHandler
    : IRequestHandler<SubmitPaymentReceiptCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public SubmitPaymentReceiptCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Result<string>> Handle(
        SubmitPaymentReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);

        if (!userExists)
            return Result<string>.Failure("کاربر یافت نشد", 404);

        var pendingCount = await _context.PaymentReceipts
            .CountAsync(
                r => r.UserId == request.UserId
                  && r.Status == Domain.Enums.PaymentReceiptStatus.Pending,
                cancellationToken);

        if (pendingCount >= 3)
            return Result<string>.Failure(
                "شما در حال حاضر ۳ رسید در انتظار بررسی دارید. لطفاً منتظر بمانید", 429);

        var storedFileName = await _fileStorage.SaveAsync(
            request.FileBytes,
            request.FileName,
            cancellationToken);

        var receipt = DomainPaymentReceipt.Create(
            request.UserId,
            request.PayerFullName,
            request.LastFourDigits,
            storedFileName,
            request.FileName,
            request.DurationDays);

        _context.PaymentReceipts.Add(receipt);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("رسید پرداخت با موفقیت ثبت شد. پس از بررسی، اشتراک شما تمدید خواهد شد", 201);
    }
}
