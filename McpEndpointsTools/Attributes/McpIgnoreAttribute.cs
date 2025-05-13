namespace McpEndpointsTools.Attributes;

/// <summary>
/// Indicates that the marked method should be ignored for certain processes or functionality.
/// </summary>
/// <remarks>
/// This attribute can be applied to methods to exclude them from specific operations or processing logic.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class McpIgnoreAttribute : Attribute
{
}