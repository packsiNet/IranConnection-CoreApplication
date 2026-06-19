using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.SetBandwidthLimit;

public record SetBandwidthLimitCommand(
    Guid TargetUserId,
    long? LimitBytes) : IRequest<Result<string>>;
