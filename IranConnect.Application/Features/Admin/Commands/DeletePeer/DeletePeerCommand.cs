using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.DeletePeer;

public record DeletePeerCommand(Guid TargetUserId)
    : IRequest<Result<string>>;
