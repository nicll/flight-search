using Confluent.Kafka;
using FlightImportService.Util;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FlightImportService.Controllers;

[ApiController]
[Route("api/Import")]
public class ImportController : ControllerBase
{
    private readonly string _aviationstackEndpoint = "http://api.aviationstack.com/v1/flights?access_key="; //missing API Key
    private readonly string _openskynetworkEndpoint = "https://opensky-network.org/api/states/all";
    private readonly string _topicName = ""; //missing topic name
    private readonly HttpClient _httpClient;
    private readonly IProducer<string, string> _kafkaProducer;
    public ImportController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };
        _kafkaProducer = new ProducerBuilder<string, string>(config).Build();
    }
    [HttpGet("ImportAviationStackData")]
    public async Task<IActionResult> ImportAviationStackData()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_aviationstackEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                List<Flight> flightDataList = JsonConvert.DeserializeObject<FlightDataWrapper>(jsonContent)!.Data;



                foreach (var flight in flightDataList)
                {
                    var flightJson = JsonConvert.SerializeObject(flight);
                    var message = new Message<string, string>
                    {
                        Key = null,
                        Value = flightJson
                    };
                    await _kafkaProducer.ProduceAsync(_topicName, message);
                }
                return Ok("Flight data sent to Kafka successfully");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch flight data");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    [HttpGet("ImportOpenskynetworkData")]
    public async Task<IActionResult> ImportOpenskynetworkData()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_openskynetworkEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                dynamic jsonObject = JsonConvert.DeserializeObject(jsonContent)!;
                var statesArray = jsonObject.states;
                List<State> statesList = DeserializeStates(statesArray);



                foreach (var state in statesList)
                {
                    var flightJson = JsonConvert.SerializeObject(state);
                    var message = new Message<string, string>
                    {
                        Key = null,
                        Value = flightJson
                    };
                    await _kafkaProducer.ProduceAsync(_topicName, message);
                }
                return Ok(statesList.First());
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch flight data");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex}");
        }
    }
    private static List<State> DeserializeStates(dynamic statesArray)
    {
        List<State> states = [];
        foreach (var stateArray in statesArray)
        {
            State state = new()
            {
                Icao24 = stateArray[0],
                Callsign = stateArray[1],
                OriginCountry = stateArray[2],
                TimePosition = stateArray[3] ?? null,
                LastContact = stateArray[4] ?? null,
                Longitude = stateArray[5],
                Latitude = stateArray[6],
                BaroAltitude = stateArray[7],
                OnGround = stateArray[8],
                Velocity = stateArray[9],
                TrueTrack = stateArray[10],
                VerticalRate = stateArray[11],
                Sensors = stateArray[12],
                GeoAltitude = stateArray[13],
                Squawk = stateArray[14],
                Spi = stateArray[15],
                PositionSource = stateArray[16]
            };
            states.Add(state);
        }
        return states;
    }
}