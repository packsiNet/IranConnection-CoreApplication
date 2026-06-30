using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using DomainStatEvent = IranConnect.Domain.Entities.StatEvent;

namespace IranConnect.Application.Features.Stats.Commands.RecordEvent;

public class RecordEventCommandHandler
    : IRequestHandler<RecordEventCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RecordEventCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        RecordEventCommand request,
        CancellationToken cancellationToken)
    {
        var statEvent = DomainStatEvent.Create(
            request.EventType,
            request.Metadata,
            request.IpAddress);

        _context.StatEvents.Add(statEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("ثبت شد");
    }
}
