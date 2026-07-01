using IranConnect.Application.Common.Models;
using MediatR;

namespace IranConnect.Application.Features.Admin.Commands.FactoryReset;

/// <summary>
/// Destructive full reset. Wipes all user-generated data (users, peers,
/// subscriptions, payments, receipts, reviews, stats) and resets app settings,
/// leaving only seeded data (Iranian app catalog) and admin accounts.
/// Guarded by a dedicated hashed password.
/// </summary>
public record FactoryResetCommand(string Password) : IRequest<Result<FactoryResetResult>>;

public record FactoryResetResult(
    int UsersDeleted,
    int PeersRemoved,
    int PeersRemovedFromInterface);
