using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace Flight_Search_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KafkaProducerController : ControllerBase
{
    private readonly ProducerConfig _producerConfig;
    public KafkaProducerController()
    {
        _producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };
    }
    [HttpPost("send/{topicName}")]
    public IActionResult SendMessage(string topicName, [FromBody] string message)
    {
        try
        {
            using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
            producer.Produce(topicName, new Message<Null, string> { Value = message });
            producer.Flush(TimeSpan.FromSeconds(10));
            return Ok("Message sent successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}