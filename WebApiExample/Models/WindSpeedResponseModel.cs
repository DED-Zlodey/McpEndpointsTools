namespace WebApiExample.Models;

/// <summary>
/// Represents the response model for wind speed data.
/// </summary>
public class WindSpeedResponseModel
{
    /// <summary>
    /// Gets or sets the wind speed data.
    /// </summary>
    /// <value>
    /// The speed represents the magnitude of wind measured in specific units.
    /// </value>
    public decimal Speed { get; set; }
}