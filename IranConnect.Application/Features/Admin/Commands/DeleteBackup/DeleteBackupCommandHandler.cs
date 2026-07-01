using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.DeleteBackup;

public class DeleteBackupCommandHandler
    : IRequestHandler<DeleteBackupCommand, Result<string>>
{
    private readonly IDatabaseBackupService _backupService;

    public DeleteBackupCommandHandler(IDatabaseBackupService backupService)
        => _backupService = backupService;

    public Task<Result<string>> Handle(
        DeleteBackupCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = _backupService.DeleteBackup(request.FileName);
            if (!deleted)
                return Task.FromResult(Result<string>.Failure("فایل بک‌آپ پیدا نشد", 404));

            return Task.FromResult(
                Result<string>.Success($"بک‌آپ '{request.FileName}' حذف شد"));
        }
        catch (ArgumentException)
        {
            return Task.FromResult(Result<string>.Failure("نام فایل نامعتبر است", 400));
        }
    }
}
