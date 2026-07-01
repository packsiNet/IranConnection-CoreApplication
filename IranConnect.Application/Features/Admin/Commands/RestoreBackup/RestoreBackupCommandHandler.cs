using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IranConnect.Application.Features.Admin.Commands.RestoreBackup;

public class RestoreBackupCommandHandler
    : IRequestHandler<RestoreBackupCommand, Result<string>>
{
    private readonly IDatabaseBackupService _backupService;
    private readonly ILogger<RestoreBackupCommandHandler> _logger;

    public RestoreBackupCommandHandler(
        IDatabaseBackupService backupService,
        ILogger<RestoreBackupCommandHandler> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(
        RestoreBackupCommand request,
        CancellationToken cancellationToken)
    {
        if (!_backupService.Exists(request.FileName))
            return Result<string>.Failure("فایل بک‌آپ پیدا نشد", 404);

        try
        {
            await _backupService.RestoreBackupAsync(request.FileName, cancellationToken);
            return Result<string>.Success(
                $"دیتابیس از بک‌آپ '{request.FileName}' بازیابی شد");
        }
        catch (ArgumentException)
        {
            return Result<string>.Failure("نام فایل نامعتبر است", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database restore failed for {FileName}", request.FileName);
            return Result<string>.Failure("بازیابی دیتابیس ناموفق بود", 500);
        }
    }
}
