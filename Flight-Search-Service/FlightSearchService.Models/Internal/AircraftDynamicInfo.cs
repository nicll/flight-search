using ProtoBuf;

namespace FlightSearchService.Models.Internal;

[ProtoContract]
public class AircraftDynamicInfo
{
    [ProtoMember(1)]
    public required string CallSign { get; init; }

    [ProtoMember(2)]
    public required DateTimeOffset LastUpdated { get; init; }

    [ProtoMember(3)]
    public required double Latitude { get; init; }

    [ProtoMember(4)]
    public required double Longitude { get; init; }

    [ProtoMember(5)]
    public required double? Altitude { get; init; }

    [ProtoMember(6)]
    public required double? Direction { get; init; }
}
