using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.DeleteApp;

public record DeleteAppCommand(Guid Id) : IRequest<Result<string>>;
