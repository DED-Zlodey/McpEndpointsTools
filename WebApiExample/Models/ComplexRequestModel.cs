namespace WebApiExample.Models;

/// <summary>
/// Represents a request model that contains detailed and complex data for a specific operation.
/// </summary>
public class ComplexRequestModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the request.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Gets or sets the username associated with the request.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the query parameters associated with the request.
    /// Provides configuration details such as minimum and maximum values
    /// and filter options for refining the request.
    /// </summary>
    public QueryParameters Parameters { get; set; } = new();

    /// <summary>
    /// A collection of tags associated with the request, used to categorize or label the request with relevant metadata.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of geographical locations associated with the request.
    /// Each location in the collection provides latitude, longitude, and optional descriptive metadata.
    /// </summary>
    public List<Location> Locations { get; set; } = new();
}