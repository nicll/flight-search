﻿namespace FlightSearchService.Models.External;

public class Departure
{
    public string Airport { get; set; }
    public string Timezone { get; set; }
    public string Iata { get; set; }
    public string Icao { get; set; }
    public object Terminal { get; set; }
    public string Gate { get; set; }
    public object Delay { get; set; }
    public DateTime Scheduled { get; set; }
    public DateTime Estimated { get; set; }
    public object Actual { get; set; }
    public object Estimated_runway { get; set; }
    public object Actual_runway { get; set; }
}

public class Arrival
{
    public string Airport { get; set; }
    public string Timezone { get; set; }
    public string Iata { get; set; }
    public string Icao { get; set; }
    public string Terminal { get; set; }
    public object Gate { get; set; }
    public object Baggage { get; set; }
    public object Delay { get; set; }
    public DateTime Scheduled { get; set; }
    public DateTime Estimated { get; set; }
    public object Actual { get; set; }
    public object Estimated_runway { get; set; }
    public object Actual_runway { get; set; }
}

public class Airline
{
    public string Name { get; set; }
    public string Iata { get; set; }
    public string Icao { get; set; }
}

public class Codeshared
{
    public string Airline_name { get; set; }
    public string Airline_iata { get; set; }
    public string Airline_icao { get; set; }
    public string Flight_number { get; set; }
    public string Flight_iata { get; set; }
    public string Flight_icao { get; set; }
}

public class Flight
{
    public DateTime Flight_date { get; set; }
    public string Flight_status { get; set; }
    public Departure Departure { get; set; }
    public Arrival Arrival { get; set; }
    public Airline Airline { get; set; }
    public FlightInfo FlightInfo { get; set; }
    public Aircraft Aircraft { get; set; }
    public Live Live { get; set; }
}

public class Aircraft
{
    public string Registration { get; set; }
    public string Iata { get; set; }
    public string Icao { get; set; }
    public string Icao24 { get; set; }
}

public class Live
{
    public DateTime Updated { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public float Altitude { get; set; }
    public float Direction { get; set; }
    public float SpeedHorizontal { get; set; }
    public float SpeedVertical { get; set; }
    public bool IsGround { get; set; }
}

public class FlightInfo
{
    public string Number { get; set; }
    public string Iata { get; set; }
    public string Icao { get; set; }
    public Codeshared Codeshared { get; set; }
}

public class FlightDataWrapper
{
    public List<Flight> Data { get; set; }
}