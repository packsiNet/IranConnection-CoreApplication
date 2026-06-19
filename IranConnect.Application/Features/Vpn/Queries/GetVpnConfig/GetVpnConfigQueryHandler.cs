using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Vpn.Queries.GetVpnConfig;

public class GetVpnConfigQueryHandler
    : IRequestHandler<GetVpnConfigQuery, Result<VpnConfigResponse>>
{
    private readonly IVpnConfigService _vpnConfigService;

    public GetVpnConfigQueryHandler(IVpnConfigService vpnConfigService)
        => _vpnConfigService = vpnConfigService;

    public async Task<Result<VpnConfigResponse>> Handle(
        GetVpnConfigQuery request,
        CancellationToken cancellationToken)
    {
        var config = await _vpnConfigService
            .GetOrCreatePeerAsync(request.UserId);

        return Result<VpnConfigResponse>.Success(
            new VpnConfigResponse(
                config.PrivateKey,
                config.AssignedIp,
                config.ServerPublicKey,
                config.ServerEndpoint,
                config.Dns));
    }
}
