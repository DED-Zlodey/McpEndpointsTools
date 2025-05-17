using McpEndpointsTools.Attributes;
using McpEndpointsTools.Models;
using Microsoft.AspNetCore.Mvc;
using WebApiExample.Models;
using WebApiExample.Models.RequestModels;

namespace WebApiExample.Controllers;

/// <summary>
/// Handles weather forecast operations including retrieving weather forecast data.
/// </summary>
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    /// <summary>
    /// Provides a collection of descriptive summaries representing different weather conditions.
    /// </summary>
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// Facilitates logging operations within the WeatherForecastController, enabling the capture and output of diagnostic information.
    /// </summary>
    private readonly ILogger<WeatherForecastController> _logger;

    /// <summary>
    /// Handles weather forecast operations including retrieving weather forecast data.
    /// </summary>
    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a collection of weather forecast information for the upcoming days.
    /// </summary>
    /// <returns>
    /// An enumerable collection of <see cref="WeatherForecast"/> objects containing the forecast data for multiple days.
    /// </returns>
    [HttpGet(Name = "GetWeatherForecast")]
    public IActionResult Get()
    {
        return Ok(
            Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray());
    }

    /// <summary>
    /// Calculates the wind speed at a specific height above the ground using logarithmic wind profile theory.
    /// </summary>
    /// <param name="v">The reference wind speed measured at height h.</param>
    /// <param name="h">The reference height at which the wind speed v is measured (in meters).</param>
    /// <param name="k">The wind shear exponent, based on terrain and atmospheric conditions.</param>
    /// <returns>
    /// An HTTP response containing the calculated wind speed at the specified height.
    /// </returns>
    [HttpGet("GetWindSpeed")]
    public IActionResult GetWindSpeed([FromQuery] double v, [FromQuery] double h, [FromQuery] double k)
    {
        var res = h / 10;
        var windSpeed = v * Math.Pow(res, k);
        return Ok(new { speed = windSpeed });
    }

    /// <summary>
    /// Predicts the chance of rainfall based on the provided meteorological data.
    /// </summary>
    /// <param name="model">The data model containing meteorological inputs, such as pressure, used for predicting rainfall.</param>
    /// <returns>
    /// A string message indicating the likelihood of precipitation: "There is a high probability of precipitation.", "Precipitation is possible.", or "No precipitation is expected."
    /// </returns>
    [HttpPost("PredictRainChance")]
    public IActionResult PredictRainChance([FromBody]PredictChanceRainfallModel model)
    {
        if (model.Pressure < 1000) return Ok(new {message ="There is a high probability of precipitation."});
        if (model.Pressure < 1015) return Ok(new {message ="Precipitation is possible."});
        return Ok(new {message = "No precipitation is expected."});
    }

    /// <summary>
    /// Provides clothing advice based on the current temperature in Celsius.
    /// </summary>
    /// <param name="tempC">The temperature in Celsius for which clothing advice is needed.</param>
    /// <returns>
    /// A string containing clothing advice suitable for the specified temperature.
    /// </returns>
    [HttpGet("GetClothingAdvice")]
    [McpIgnore]
    public IActionResult GetClothingAdvice([FromQuery] double tempC)
    {
        if (tempC >= 25) return Ok("Put on a T-shirt and shorts.");
        if (tempC >= 15) return Ok("A light jacket will do.");
        if (tempC >= 5) return Ok("I need a coat.");
        return Ok("It's very cold — keep warm!");
    }

    /// <summary>
    /// Processes a complex request and returns a detailed response based on the provided model data.
    /// </summary>
    /// <param name="model">An instance of <see cref="ComplexRequestModel"/> containing the input data for processing the request.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the response with a confirmation message and echoing the processed model data.
    /// </returns>
    [HttpPost("ProcessComplexRequest")]
    public IActionResult ProcessComplexRequest([FromBody] ComplexRequestModel model)
    {
        return Ok(new { message = "Received", model });
    }
}