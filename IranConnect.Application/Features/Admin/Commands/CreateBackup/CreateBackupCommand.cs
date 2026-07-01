using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.CreateBackup;

public record CreateBackupCommand() : IRequest<Result<BackupFileInfo>>;
