namespace WebApiExample.Models;

/// <summary>
/// Represents filtering options for data retrieval or query processes.
/// </summary>
public class FilterOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the current filter is active.
    /// </summary>
    /// <remarks>
    /// This property determines if the filter should be applied based on its status.
    /// It typically reflects whether the filter is enabled or disabled.
    /// </remarks>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the start date for filtering operations.
    /// </summary>
    /// <remarks>
    /// This property represents the starting point of a date range that can be used
    /// to filter data within the application. Ensure to set the value to a valid DateTime object
    /// as required by the filtering logic.
    /// </remarks>
    public DateTime StartDate { get; set; }
}