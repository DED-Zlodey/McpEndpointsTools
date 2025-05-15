using System.Reflection;
using McpEndpointsTools.Attributes;

namespace Test;

/// <summary>
/// A test class for validating the <see cref="McpIgnoreAttribute"/> functionality and its correct application
/// to classes and methods.
/// </summary>
/// <remarks>
/// This class contains unit tests to verify the behavior, usage, and validity of the <see cref="McpIgnoreAttribute"/>.
/// It ensures that the attribute can be successfully applied to targets like classes and methods, and checks
/// its usage constraints and expected outcomes.
/// </remarks>
public class McpIgnoreAttributeTests
{
    /// <summary>
    /// Tests whether the <see cref="McpIgnoreAttribute"/> can be correctly applied to a class and retrieved via reflection.
    /// </summary>
    /// <remarks>
    /// This method validates that the <see cref="McpIgnoreAttribute"/> is present on the specified class by using reflection.
    /// It ensures that the attribute is not null when queried from the target class.
    /// </remarks>
    /// <exception cref="Xunit.Sdk.NotNullException">
    /// Thrown if the <see cref="McpIgnoreAttribute"/> is not found on the target class.
    /// </exception>
    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        var attr = typeof(SampleIgnoredClass).GetCustomAttribute<McpIgnoreAttribute>();
        Assert.NotNull(attr);
    }

    /// <summary>
    /// Validates that the <see cref="McpIgnoreAttribute"/> can be applied to methods.
    /// </summary>
    /// <remarks>
    /// This test ensures that the <see cref="McpIgnoreAttribute"/> is correctly recognized
    /// when applied to a method and that the attribute can be retrieved using reflection.
    /// </remarks>
    [Fact]
    public void Attribute_CanBeAppliedToMethod()
    {
        var method = typeof(SampleIgnoredMethodClass).GetMethod(nameof(SampleIgnoredMethodClass.IgnoredMethod))!;
        var attr = method.GetCustomAttribute<McpIgnoreAttribute>();
        Assert.NotNull(attr);
    }

    /// <summary>
    /// Verifies that the <see cref="McpIgnoreAttribute"/> has the correct usage targets defined.
    /// </summary>
    /// <remarks>
    /// Ensures that the <see cref="AttributeTargets"/> for <see cref="McpIgnoreAttribute"/>
    /// includes classes and methods, as specified by its <see cref="AttributeUsageAttribute"/>.
    /// </remarks>
    [Fact]
    public void Attribute_HasCorrectUsageTargets()
    {
        var usage = typeof(McpIgnoreAttribute).GetCustomAttribute<AttributeUsageAttribute>()!;
        Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Class));
        Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Method));
    }

    /// <summary>
    /// Represents a class that is marked with the <see cref="McpIgnoreAttribute"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="McpIgnoreAttribute"/> is applied to this class, indicating
    /// that it should be excluded from specific processing pipelines or tools.
    /// </remarks>
    [McpIgnore]
    private class SampleIgnoredClass { }

    /// <summary>
    /// Represents a class that demonstrates the application of the <see cref="McpIgnoreAttribute" />
    /// to its methods for exclusion from specific processing pipelines or tools.
    /// </summary>
    private class SampleIgnoredMethodClass
    {
        /// <summary>
        /// A method marked with the <see cref="McpIgnoreAttribute"/>, indicating that it should
        /// be excluded from specific automated processing pipelines or behaviors.
        /// </summary>
        /// <remarks>
        /// The method is typically ignored by systems that scan, process, or register endpoints
        /// unless explicitly indicated otherwise.
        /// </remarks>
        [McpIgnore]
        public void IgnoredMethod() { }
    }
}