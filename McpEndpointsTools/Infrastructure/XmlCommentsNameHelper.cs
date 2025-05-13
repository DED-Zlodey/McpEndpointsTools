using System.Reflection;
using System.Text;

namespace McpEndpointsTools.Infrastructure;

/// <summary>
/// Provides functionality for generating XML comments-style member names
/// for methods using reflection.
/// </summary>
internal static class XmlCommentsNameHelper
{
    /// <summary>
    /// Generates a string representation of a method's XML documentation name,
    /// as used within XML documentation files, including the method name,
    /// its declaring type, and parameter types if applicable.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo"/> object representing the method to construct the name for.</param>
    /// <returns>
    /// A string that uniquely identifies the method for XML documentation purposes,
    /// formatted as "M:Namespace.Class.Method(Type1,Type2,...)".
    /// </returns>
    public static string GetMemberNameForMethod(MethodInfo method)
    {
        const string prefix = "M:";
        
        var typeName = method.DeclaringType!.FullName!.Replace('+', '.');
        
        var methodName = method.Name;
        
        var parameters = method.GetParameters();
        
        var member = new StringBuilder($"{prefix}{typeName}.{methodName}");
        
        if (parameters.Length > 0)
        {
            var paramList = string.Join(",",
                parameters.Select(p => GetTypeName(p.ParameterType)));
            member.Append($"({paramList})");
        }

        return member.ToString();
    }

    /// <summary>
    /// Retrieves the full type name of the specified <see cref="Type"/>.
    /// If the type is a generic type, it also processes generic type arguments and returns the formatted generic type name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> for which the name is being retrieved.</param>
    /// <returns>A string representation of the type name. If the type is generic, it includes formatted generic arguments.</returns>
    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var generic = type.GetGenericTypeDefinition()
                .FullName!;
            var args = string.Join(",",
                type.GetGenericArguments().Select(GetTypeName));
            return generic.Substring(0, generic.IndexOf('`')) + $"{{{args}}}";
        }
        return type.FullName!;
    }
}