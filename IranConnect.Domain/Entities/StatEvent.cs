using IranConnect.Domain.Common;
using IranConnect.Domain.Enums;

namespace IranConnect.Domain.Entities;

public class StatEvent : BaseEntity
{
    public StatEventType EventType { get; private set; }
    public string? Metadata { get; private set; }
    public string? IpAddress { get; private set; }

    private StatEvent() { }

    public static StatEvent Create(
        StatEventType eventType,
        string? metadata = null,
        string? ipAddress = null)
    {
        return new StatEvent
        {
            EventType = eventType,
            Metadata = metadata,
            IpAddress = ipAddress
        };
    }
}
