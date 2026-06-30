using IranConnect.Application.Common.Models;
using IranConnect.Domain.Enums;
using MediatR;

namespace IranConnect.Application.Features.Stats.Commands.RecordEvent;

public record RecordEventCommand(
    StatEventType EventType,
    string? Metadata = null,
    string? IpAddress = null
) : IRequest<Result<string>>;
