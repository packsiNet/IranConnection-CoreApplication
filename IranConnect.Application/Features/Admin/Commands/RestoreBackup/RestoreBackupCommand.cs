using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.RestoreBackup;

public record RestoreBackupCommand(string FileName) : IRequest<Result<string>>;
