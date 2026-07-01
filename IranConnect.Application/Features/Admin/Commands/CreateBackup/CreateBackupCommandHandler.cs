using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IranConnect.Application.Features.Admin.Commands.CreateBackup;

public class CreateBackupCommandHandler
    : IRequestHandler<CreateBackupCommand, Result<BackupFileInfo>>
{
    private readonly IDatabaseBackupService _backupService;
    private readonly ILogger<CreateBackupCommandHandler> _logger;

    public CreateBackupCommandHandler(
        IDatabaseBackupService backupService,
        ILogger<CreateBackupCommandHandler> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    public async Task<Result<BackupFileInfo>> Handle(
        CreateBackupCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var info = await _backupService.CreateBackupAsync(cancellationToken);
            return Result<BackupFileInfo>.Success(info, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database backup failed");
            return Result<BackupFileInfo>.Failure("ساخت بک‌آپ ناموفق بود", 500);
        }
    }
}
