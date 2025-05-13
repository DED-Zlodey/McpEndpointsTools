namespace WebApiExample.Models;

/// <summary>
/// Represents a weather forecast for a specific date.
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Gets or sets the date for the weather forecast.
    /// </summary>
    /// <remarks>
    /// This property represents the specific date for which the weather forecast is applicable.
    /// It uses the <see cref="DateOnly"/> type to store date information without the time component.
    /// </remarks>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the temperature in degrees Celsius.
    /// </summary>
    /// <remarks>
    /// This property represents the temperature recorded or forecasted in Celsius.
    /// It can be used as an input or output value to represent the temperature in metric units.
    /// </remarks>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Gets the temperature in Fahrenheit.
    /// </summary>
    /// <remarks>
    /// This property calculates the temperature in Fahrenheit based on the value of <see cref="TemperatureC"/>.
    /// The calculation uses the formula: 32 + (int)(TemperatureC / 0.5556).
    /// </remarks>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// Gets or sets a brief description of the weather condition.
    /// </summary>
    /// <remarks>
    /// This property provides a textual summary of the weather, such as "Sunny", "Rainy", or "Cloudy".
    /// </remarks>
    public string? Summary { get; set; }
}