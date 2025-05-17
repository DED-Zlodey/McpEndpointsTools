namespace WebApiExample.Models;

/// <summary>
/// Represents query parameters for a request, including range limits and filtering options.
/// </summary>
public class QueryParameters
{
    /// <summary>
    /// Gets or sets the minimum value constraint for the query parameter.
    /// This property typically represents the lower bound in filtering operations,
    /// ensuring that the returned data includes values greater than or equal to the specified value.
    /// </summary>
    public int Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value parameter.
    /// </summary>
    /// <remarks>
    /// This property is used to define the upper limit for a specified range
    /// within the query parameters.
    /// </remarks>
    public int Max { get; set; }

    /// Represents the filter options used to customize the query parameters in a request.
    /// It provides a structure for defining specific criteria such as activity status or date range,
    /// allowing for more refined query results.
    public FilterOptions Filters { get; set; } = new();
}