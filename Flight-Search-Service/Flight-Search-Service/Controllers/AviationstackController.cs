using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Flight_Search_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AviationstackController : Controller
{
    private readonly string _baseUrl = "http://api.aviationstack.com/v1/flights";
    private readonly string _apiKey = "";
    [HttpGet("GetFlightInfo")]
    public async Task<IActionResult> GetFlightInfo(string airlineName, string flightIata)
    {
        try
        {
            string url = $"{_baseUrl}?access_key={_apiKey}&airline_name={airlineName}&flight_iata={flightIata}";
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            JsonElement dataElement = JsonDocument.Parse(responseBody).RootElement.GetProperty("data");
            string todayDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            foreach (JsonElement flightElement in dataElement.EnumerateArray())
            {
                if (flightElement.GetProperty("flight_date").GetString() == todayDate)
                {
                    string departureAirport = flightElement.GetProperty("departure").GetProperty("airport").GetString()!;
                    string arrivalAirport = flightElement.GetProperty("arrival").GetProperty("airport").GetString()!;
                    var filteredResponse = new
                    {
                        DepartureAirport = departureAirport,
                        ArrivalAirport = arrivalAirport
                    };
                    return Ok(filteredResponse);
                }
            }
            return NotFound("No flight with today's date found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
