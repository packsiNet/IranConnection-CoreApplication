using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.ResetPeer;

public record ResetPeerCommand(Guid TargetUserId)
    : IRequest<Result<string>>;
