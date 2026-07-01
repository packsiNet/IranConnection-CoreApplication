using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Queries.GetBackups;

public record GetBackupsQuery() : IRequest<Result<List<BackupFileInfo>>>;
