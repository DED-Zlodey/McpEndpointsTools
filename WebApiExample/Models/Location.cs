namespace WebApiExample.Models;

/// <summary>
/// Represents a geographical location with associated metadata.
/// </summary>
public class Location
{
    /// <summary>
    /// Gets or sets the latitude of the location.
    /// </summary>
    /// <remarks>
    /// Latitude is a geographic coordinate that specifies the north–south position of a point on the Earth's surface.
    /// Values range from -90.0 (south pole) to 90.0 (north pole).
    /// </remarks>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude component of the geographical location.
    /// </summary>
    /// <remarks>
    /// Represents the east-west position on the Earth's surface, measured in decimal degrees.
    /// Values range from -180.0 (west) to 180.0 (east).
    /// </remarks>
    public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets the description of the location.
    /// This property provides additional information or context about the location.
    /// </summary>
    public string? Description { get; set; }
}