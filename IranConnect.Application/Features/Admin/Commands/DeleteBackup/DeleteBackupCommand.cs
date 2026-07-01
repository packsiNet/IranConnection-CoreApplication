using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.DeleteBackup;

public record DeleteBackupCommand(string FileName) : IRequest<Result<string>>;
