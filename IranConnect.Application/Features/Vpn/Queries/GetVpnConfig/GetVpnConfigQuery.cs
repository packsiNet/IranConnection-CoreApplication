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
    string Dns);
