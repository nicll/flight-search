using System.Text.Json;
using Confluent.Kafka;
using FlightSearchService.Models.External;
using Microsoft.AspNetCore.Mvc;

namespace FlightImportService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AviationstackController : ControllerBase
{
    private readonly string _aviationstackEndpoint = "http://api.aviationstack.com/v1/flights?access_key=";
    private readonly string _topicName = "import.aviationstack";
    private readonly HttpClient _httpClient;
    private readonly IProducer<string, string> _kafkaProducer;

    public AviationstackController(IHttpClientFactory httpClientFactory)
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
            HttpResponseMessage response = await _httpClient.GetAsync(_aviationstackEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                List<Flight> flightDataList = (await JsonSerializer.DeserializeAsync<FlightDataWrapper>(await response.Content.ReadAsStreamAsync()))!.Data;

                foreach (var flight in flightDataList)
                {
                    var flightJson = JsonSerializer.Serialize(flight);
                    var message = new Message<string, string>
                    {
                        Key = flight.Aircraft.Registration.ToUpperInvariant(),
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
