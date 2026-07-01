using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetBackups;

public class GetBackupsQueryHandler
    : IRequestHandler<GetBackupsQuery, Result<List<BackupFileInfo>>>
{
    private readonly IDatabaseBackupService _backupService;

    public GetBackupsQueryHandler(IDatabaseBackupService backupService)
        => _backupService = backupService;

    public Task<Result<List<BackupFileInfo>>> Handle(
        GetBackupsQuery request,
        CancellationToken cancellationToken)
    {
        var backups = _backupService.ListBackups().ToList();
        return Task.FromResult(Result<List<BackupFileInfo>>.Success(backups));
    }
}
