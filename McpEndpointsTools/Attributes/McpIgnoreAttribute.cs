namespace McpEndpointsTools.Attributes;

/// <summary>
/// An attribute that can be applied to classes or methods to mark them for exclusion
/// from specific processing pipelines or tools.
/// </summary>
/// <remarks>
/// This attribute is typically used to indicate that the associated class or
/// method should be ignored by automated systems such as endpoint scanners
/// or registration mechanisms.
/// </remarks>
/// <example>
/// This attribute can be applied to a controller or an action method
/// to prevent it from being registered or processed in certain scenarios.
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class McpIgnoreAttribute : Attribute
{
}