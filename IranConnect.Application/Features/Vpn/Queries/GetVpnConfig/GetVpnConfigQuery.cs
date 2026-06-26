using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Vpn.Queries.GetVpnConfig;

public record GetVpnConfigQuery(Guid UserId)
    : IRequest<Result<VpnConfigResponse>>;

public record VpnConfigResponse(
    string PrivateKey,
    string AssignedIp,
    string ServerPublicKey,
    string ServerEndpoint,
    string Dns,
    string AllowedIPs,
    // AmneziaWG obfuscation params the client must apply to its [Interface].
    AmneziaObfuscation Obfuscation);
