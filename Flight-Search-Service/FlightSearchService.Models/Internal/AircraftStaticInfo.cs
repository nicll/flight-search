using ProtoBuf;

namespace FlightSearchService.Models.Internal;

[ProtoContract]
public class AircraftStaticInfo
{
    [ProtoMember(1)]
    public required string CallSign { get; init; }

    [ProtoMember(2)]
    public required DateTimeOffset LastUpdated { get; init; }

    [ProtoMember(3)]
    public required string Icao24 { get; init; }
}
