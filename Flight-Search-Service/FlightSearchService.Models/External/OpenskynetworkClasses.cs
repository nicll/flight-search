namespace FlightSearchService.Models.External;

public class State
{
    public string? Icao24 { get; set; }
    public string? Callsign { get; set; }
    public string? OriginCountry { get; set; }
    public long? TimePosition { get; set; }
    public long? LastContact { get; set; }
    public float? Longitude { get; set; }
    public float? Latitude { get; set; }
    public float? BaroAltitude { get; set; }
    public bool OnGround { get; set; }
    public float? Velocity { get; set; }
    public float? TrueTrack { get; set; }
    public float? VerticalRate { get; set; }
    public double[]? Sensors { get; set; }
    public float? GeoAltitude { get; set; }
    public string? Squawk { get; set; }
    public bool Spi { get; set; }
    public double? PositionSource { get; set; }
}

public class RootObject
{
    public int? Time { get; set; }
    public List<State> States { get; set; }
}

