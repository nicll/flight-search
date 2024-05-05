using ProtoBuf;

namespace FlightSearchWebService.Components.Model
{
    public class AircraftCombined
    {
        public required string CallSign { get; init; }

        public required string LastUpdated { get; init; }

        public required string Icao24 { get; init; }

        public required string Latitude { get; init; }

        public required string Longitude { get; init; }

        public required string Altitude { get; init; }

        public required string Direction { get; init; }
    }
}
