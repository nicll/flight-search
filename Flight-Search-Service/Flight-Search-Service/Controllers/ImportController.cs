using Confluent.Kafka;
using Flight_Search_Service.Util;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Flight_Search_Service.Controllers;

[ApiController]
[Route("api/Import")]
public class ImportController : ControllerBase
{
    private readonly string _endpoint = "http://api.aviationstack.com/v1/flights?access_key="; //missing API Key
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
            HttpResponseMessage response = await _httpClient.GetAsync(_endpoint);
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
}