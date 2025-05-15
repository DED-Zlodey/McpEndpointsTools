using System.Reflection;
using McpEndpointsTools.Infrastructure;
using Test.Models;

namespace Test;

/// <summary>
/// Contains unit tests for the <c>XmlCommentsNameHelper</c> class,
/// specifically focusing on verifying the proper generation of XML documentation
/// names for various kinds of methods and class members.
/// </summary>
/// <remarks>
/// This test class addresses methods with different parameter configurations,
/// including parameterless methods, methods with single or multiple parameters,
/// generic methods, and methods in nested classes. These tests ensure compliance
/// with the standard XML documentation name generation format.
/// </remarks>
public class XmlCommentsNameHelperTests
{
    /// <summary>
    /// Retrieves information about a method from a specified type, based on its name and parameters.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> that contains the method.</param>
    /// <param name="methodName">The name of the method to retrieve.</param>
    /// <param name="parameterTypes">An array specifying the types of the method's parameters.</param>
    /// <returns>The <see cref="MethodInfo"/> object representing the method matching the provided name and parameters.</returns>
    private static MethodInfo GetMethod(Type type, string methodName, params Type[] parameterTypes)
    {
        return type.GetMethod(methodName, parameterTypes)!;
    }

    /// <summary>
    /// Unit test to verify the behavior of the <c>XmlCommentsNameHelper.GetMemberNameForMethod</c>
    /// providing a method with no parameters.
    /// </summary>
    /// <remarks>
    /// The test checks that the generated XML documentation name is
    /// correctly formatted for a parameterless method, using the format:
    /// "M:Namespace.Class.Method".
    /// </remarks>
    [Fact]
    public void GetMemberNameForMethod_NoParameters()
    {
        var method = GetMethod(typeof(SampleClass), nameof(SampleClass.NoParams));
        var result = XmlCommentsNameHelper.GetMemberNameForMethod(method);
        Assert.Equal("M:Test.Models.SampleClass.NoParams", result);
    }

    /// <summary>
    /// Unit test to verify the behavior of the <c>XmlCommentsNameHelper.GetMemberNameForMethod</c>
    /// providing a method with a single parameter.
    /// </summary>
    /// <remarks>
    /// The test ensures that the generated XML documentation name is
    /// correctly formatted for a method that takes one parameter. The method signature
    /// includes both the name of the method and the fully qualified type of the parameter.
    /// The expected format is: "M:Namespace.Class.Method(ParameterType)".
    /// </remarks>
    [Fact]
    public void GetMemberNameForMethod_WithSingleParameter()
    {
        var method = GetMethod(typeof(SampleClass), nameof(SampleClass.WithOneParam), new[] { typeof(int) });
        var result = XmlCommentsNameHelper.GetMemberNameForMethod(method);
        Assert.Equal("M:Test.Models.SampleClass.WithOneParam(System.Int32)", result);
    }

    /// <summary>
    /// Unit test to verify the behavior of the <c>XmlCommentsNameHelper.GetMemberNameForMethod</c>
    /// when provided a method with multiple parameters.
    /// </summary>
    /// <remarks>
    /// The test ensures that the generated XML documentation name correctly represents
    /// the method name and all its parameter types in the format:
    /// "M:Namespace.Class.Method(Type1,Type2,...)".
    /// </remarks>
    [Fact]
    public void GetMemberNameForMethod_WithMultipleParameters()
    {
        var method = GetMethod(typeof(SampleClass), nameof(SampleClass.WithTwoParams), new[] { typeof(string), typeof(double) });
        var result = XmlCommentsNameHelper.GetMemberNameForMethod(method);
        Assert.Equal("M:Test.Models.SampleClass.WithTwoParams(System.String,System.Double)", result);
    }

    /// <summary>
    /// Unit test to verify the behavior of the <c>XmlCommentsNameHelper.GetMemberNameForMethod</c>
    /// when provided a method with a generic parameter.
    /// </summary>
    /// <remarks>
    /// This test ensures that the generated XML documentation name correctly represents
    /// a method with a generic parameter. The format includes the method name, type name,
    /// and the fully qualified type of the generic parameter. For example:
    /// "M:Namespace.Class.Method(Namespace.List{Namespace.Type})".
    /// </remarks>
    [Fact]
    public void GetMemberNameForMethod_GenericParameter()
    {
        var method = GetMethod(typeof(SampleClass), nameof(SampleClass.WithGeneric), new[] { typeof(List<int>) });
        var result = XmlCommentsNameHelper.GetMemberNameForMethod(method);
        Assert.Equal("M:Test.Models.SampleClass.WithGeneric(System.Collections.Generic.List{System.Int32})", result);
    }

    /// <summary>
    /// Unit test to validate the behavior of the <c>XmlCommentsNameHelper.GetMemberNameForMethod</c>
    /// when applied to a method inside a nested class.
    /// </summary>
    /// <remarks>
    /// This test ensures that the generated XML documentation name correctly reflects
    /// the format for methods within nested classes, including the nested class's name
    /// in the fully qualified method path. The expected format is:
    /// "M:Namespace.OuterClass.NestedClass.Method".
    /// </remarks>
    [Fact]
    public void GetMemberNameForMethod_NestedClass()
    {
        var method = GetMethod(typeof(SampleClass.Nested), nameof(SampleClass.Nested.InnerMethod));
        var result = XmlCommentsNameHelper.GetMemberNameForMethod(method);
        Assert.Equal("M:Test.Models.SampleClass.Nested.InnerMethod", result);
    }
}
