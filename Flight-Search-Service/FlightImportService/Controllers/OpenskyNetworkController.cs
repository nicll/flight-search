using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using FlightImportService.Util;
using Microsoft.AspNetCore.Mvc;

namespace FlightImportService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenskyNetworkController : ControllerBase
{
    private readonly string _openskynetworkEndpoint = "https://opensky-network.org/api/states/all";
    private readonly string _topicName = "import.openskynetworks";
    private readonly HttpClient _httpClient;
    private readonly IProducer<string, string> _kafkaProducer;

    public OpenskyNetworkController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };
        _kafkaProducer = new ProducerBuilder<string, string>(config).Build();
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Import()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_openskynetworkEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await JsonNode.ParseAsync(await response.Content.ReadAsStreamAsync());
                var statesArray = JsonSerializer.Deserialize<List<List<object>>>(json!["states"])!;
                List<State> statesList = DeserializeStates(statesArray);

                foreach (var state in statesList)
                {
                    var flightJson = JsonSerializer.Serialize(state);
                    var message = new Message<string, string>
                    {
                        Key = state.Callsign,
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

    private static List<State> DeserializeStates(List<List<object>> statesArray)
    {
        return statesArray
            .Select(stateArray => new State()
            {
                Icao24 = stateArray[0] as string,
                Callsign = stateArray[1] as string,
                OriginCountry = stateArray[2] as string,
                TimePosition = stateArray[3] as long?,
                LastContact = stateArray[4] as long?,
                Longitude = stateArray[5] as float?,
                Latitude = stateArray[6] as float?,
                BaroAltitude = stateArray[7] as float?,
                OnGround = stateArray[8] as bool? ?? false,
                Velocity = stateArray[9] as float?,
                TrueTrack = stateArray[10] as float?,
                VerticalRate = stateArray[11] as float?,
                Sensors = stateArray[12] as double[],
                GeoAltitude = stateArray[13] as float?,
                Squawk = stateArray[14] as string,
                Spi = stateArray[15] as bool? ?? false,
                PositionSource = stateArray[16] as double?
            })
            .ToList();
    }
}
