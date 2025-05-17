namespace McpEndpointsTools.Models;

/// <summary>
/// Represents a property model used for defining resources in the McpEndpointsTools.
/// </summary>
/// <remarks>
/// This class is designed to encapsulate individual properties of a resource, typically reflecting
/// a parameter or field with metadata such as name, type, and optional description.
/// </remarks>
public class PropertyResourceModel
{
    /// Gets or sets the name of the property resource.
    /// Represents the unique identifier or label for the property in question.
    public string Name { get; set; } = null!;

    /// Gets or sets the data type of the property in the resource model.
    /// Typically used to define the type of a property, such as "string", "int",
    /// "bool", or other supported data types.
    /// The value should reflect the underlying .NET type or the type representation
    /// used in the context of the resource model.
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the property resource.
    /// This provides additional details or context about the property,
    /// which can be used for documentation or descriptive purposes.
    /// </summary>
    public string? Description { get; set; }
    public List<PropertyResourceModel>? Children { get; set; }
}