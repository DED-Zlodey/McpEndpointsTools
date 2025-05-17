using System.Reflection;
using McpEndpointsTools.Attributes;
using McpEndpointsTools.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace McpEndpointsTools.Infrastructure;

/// Represents an operation registrar responsible for scanning assemblies
/// to discover and register tools and resources in the server context.
public class OperationRegistrar
{
    /// <summary>
    /// Represents the service provider used for creating instances of controller types dynamically
    /// during the scanning and registration of operations.
    /// </summary>
    private readonly IServiceProvider _sp;

    /// Represents an instance of the XmlCommentsProvider used to retrieve XML documentation comments
    /// such as summaries and parameter descriptions for methods and members.
    /// It assists in extracting metadata for tools and resources during operation registration.
    private readonly XmlCommentsProvider _xml;

    /// <summary>
    /// Represents a private collection of server tools registered during the scanning
    /// of assemblies for controller methods. This variable holds instances of
    /// <see cref="McpServerTool"/> that are created based on the discovered methods and their attributes.
    /// </summary>
    /// <remarks>
    /// This collection is populated during the execution of the <c>ScanAssembly</c> method, which analyzes
    /// the types in an assembly, detects controllers and their methods, generates the corresponding tools,
    /// and adds them to the list. The tools are configured using method metadata like HTTP attributes,
    /// custom attributes, XML comments, and routing information.
    /// </remarks>
    private readonly List<McpServerTool> _tools = new();

    /// <summary>
    /// A private list that holds instances of <see cref="McpServerResource"/>.
    /// It is used to store server resource definitions created during the scanning
    /// and processing of assemblies and their associated API endpoints.
    /// This list is populated within the <c>ScanAssembly</c> method,
    /// where resources are created based on method routes, HTTP attributes,
    /// and XML documentation comments for summary descriptions.
    /// </summary>
    private readonly List<McpServerResource> _resources = new();

    private readonly List<ResourceModel> _resourcesModel = new();

    /// <summary>
    /// Represents the base path for routing and constructing URI templates within the application.
    /// This value is used as the root URL prefix for defining API endpoints.
    /// </summary>
    private readonly string _basePath;

    /// Represents a class responsible for registering operations, tools, and resources
    /// for a server application. It provides methods to scan assemblies and collect
    /// relevant information about controllers, tools, and resources. This class interacts
    /// with an `XmlCommentsProvider` to retrieve XML documentation comments and uses
    /// a service provider for dependency resolution.
    public OperationRegistrar(IServiceProvider serviceProvider, XmlCommentsProvider xmlCommentsProvider,
        string basePath)
    {
        _sp = serviceProvider;
        _xml = xmlCommentsProvider;
        _basePath = basePath.TrimEnd('/');
    }

    /// Scans the provided assembly for controllers and their associated actions, extracting metadata and registering
    /// tools and resources accordingly.
    /// This includes retrieving route templates, HTTP method attributes, and XML comment summaries to create structured
    /// tools and resources that describe available API endpoints.
    /// <param name="asm">The assembly to be scanned for API controllers and actions.</param>
    public void ScanAssembly(Assembly asm)
    {
        var controllers = GetValidControllers(asm);

        foreach (var ctrlType in controllers)
        {
            var classRoute = GetControllerRoute(ctrlType);
            var instance = ActivatorUtilities.CreateInstance(_sp, ctrlType);
            var methods = GetValidMethods(ctrlType);

            foreach (var method in methods)
            {
                var http = GetHttpMethod(method);
                if (http == null) continue;

                var methodRoute = http.Template ?? string.Empty;
                var xmlName = XmlCommentsNameHelper.GetMemberNameForMethod(method);
                var desc = _xml.GetSummary(xmlName) ?? string.Empty;
                var controllerPart = ctrlType.Name.Replace("Controller", "", StringComparison.Ordinal)
                    .ToLowerInvariant();

                if (!string.IsNullOrEmpty(classRoute))
                {
                    var absoluteUri = CombineRoutes(_basePath, classRoute, methodRoute);
                    var toolName = GenerateToolName(controllerPart, method);

                    RegisterTool(method, instance, toolName, desc);
                    RegisterResource(method, instance, toolName, absoluteUri, desc);
                    RegisterResourceModel(method, xmlName, toolName, absoluteUri, desc, http);
                }
            }
        }
    }

    /// Retrieves a collection of valid controller types from the specified assembly.
    /// A controller is considered valid if it is a non-abstract class annotated with
    /// the `ApiControllerAttribute` or inherits from `ControllerBase`, and if it
    /// does not have the `McpIgnoreAttribute` or `AuthorizeAttribute`.
    /// <param name="asm">The assembly to scan for valid controllers.</param>
    /// <returns>An enumerable collection of `Type` objects representing valid controllers.</returns>
    private IEnumerable<Type> GetValidControllers(Assembly asm)
    {
        return asm.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                        (t.GetCustomAttribute<ApiControllerAttribute>() != null ||
                         typeof(ControllerBase).IsAssignableFrom(t)))
            .Where(t => t.GetCustomAttribute<McpIgnoreAttribute>() == null &&
                        t.GetCustomAttribute<AuthorizeAttribute>() == null);
    }

    /// Retrieves the route template for a given controller type by analyzing its name
    /// and associated route attributes.
    /// <param name="controllerType">The type of the controller to extract the route from.</param>
    /// <returns>A string representing the route template of the controller if found, otherwise null.</returns>
    private string? GetControllerRoute(Type controllerType)
    {
        return controllerType.Name.Replace("Controller", "") ??
               controllerType.GetCustomAttribute<RouteAttribute>()?.Template;
    }

    /// Retrieves a collection of valid methods from a specified controller type. Valid methods are
    /// public instance methods that are not marked with the `McpIgnoreAttribute` or `AuthorizeAttribute`
    /// and have at least one HTTP method attribute (`HttpMethodAttribute`) applied.
    /// <param name="controllerType">
    /// The type representing the controller from which methods will be retrieved.
    /// </param>
    /// <returns>
    /// A collection of `MethodInfo` objects representing the valid methods of the given controller type.
    /// </returns>
    private IEnumerable<MethodInfo> GetValidMethods(Type controllerType)
    {
        return controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<McpIgnoreAttribute>() == null &&
                        m.GetCustomAttribute<AuthorizeAttribute>() == null &&
                        m.GetCustomAttributes().OfType<HttpMethodAttribute>().Any());
    }

    /// Retrieves the HTTP method attribute associated with the given method, if available.
    /// This method looks for attributes of the type ` HttpMethodAttribute ` on the provided `MethodInfo`
    /// and returns the first occurrence.
    /// <param name="method">
    /// The method metadata information for which to retrieve the associated HTTP method attribute.
    /// </param>
    /// <returns>
    /// An instance of `HttpMethodAttribute` if found; otherwise, null.
    /// </returns>
    private HttpMethodAttribute? GetHttpMethod(MethodInfo method)
    {
        return method.GetCustomAttributes().OfType<HttpMethodAttribute>().FirstOrDefault();
    }

    /// Combines multiple route segments into a single route string, ensuring proper formatting and
    /// trimming of slashes. If the combined result is an empty string, it returns the base path.
    /// <param name="basePath">The base path of the server where the routes are rooted.</param>
    /// <param name="classRoute">The route associated with the class or controller.</param>
    /// <param name="methodRoute">The route associated with the specific method or operation.</param>
    /// <return>A combined route string derived from the base path, class route, and method route.</return>
    private string CombineRoutes(string basePath, string classRoute, string methodRoute)
    {
        var routeTemplate = string.Join("/",
            new[] { classRoute.Trim('/'), methodRoute.Trim('/') }
                .Where(p => !string.IsNullOrEmpty(p)));

        return string.IsNullOrEmpty(routeTemplate) ? basePath : $"{basePath}/{routeTemplate}";
    }

    /// Generates a tool name based on the provided controller part and method information.
    /// The generated tool name is typically used for unique identification of tools within the application.
    /// <param name="controllerPart">
    /// A string representing the name of the controller, typically in lowercase and without the "Controller" suffix.
    /// </param>
    /// <param name="method">
    /// The `MethodInfo` instance representing the method for which the tool name is being generated.
    /// </param>
    /// <returns>
    /// A string representing the generated tool name, which combines the controller part and the method name in a
    /// standardized format.
    /// </returns>
    private string GenerateToolName(string controllerPart, MethodInfo method)
    {
        return $"{controllerPart}-{method.Name.ToLowerInvariant()}";
    }

    /// Registers a new tool by associating it with the specified method, instance, tool name, and description.
    /// This method creates and adds a new `McpServerTool` instance to the internal list of tools.
    /// <param name="method">The method information to associate with the tool.</param>
    /// <param name="instance">The instance of the controller or object containing the method.</param>
    /// <param name="toolName">The unique name to assign to the tool being registered.</param>
    /// <param name="desc">The description associated with the tool, typically used for documentation or display purposes.</param>
    private void RegisterTool(MethodInfo method, object instance, string toolName, string desc)
    {
        var opts = new McpServerToolCreateOptions
        {
            Name = toolName,
            Description = desc,
            Title = toolName,
        };

        _tools.Add(McpServerTool.Create(method, instance, opts));
    }

    /// Registers a resource in the server context based on the provided method information,
    /// instance, tool name, URI template, and description.
    /// This method creates and adds a new server resource to the internal resource collection.
    /// <param name="method">The method information used to define the resource.</param>
    /// <param name="instance">The instance of the controller or handler associated with the resource.</param>
    /// <param name="toolName">The name of the tool associated with the resource.</param>
    /// <param name="uri">The URI template that represents the resource endpoint.</param>
    /// <param name="desc">A description of the resource for documentation purposes.</param>
    private void RegisterResource(MethodInfo method, object instance, string toolName, string uri, string desc)
    {
        _resources.Add(McpServerResource.Create(method, instance, new McpServerResourceCreateOptions
        {
            UriTemplate = uri,
            Name = $"{toolName}-resource",
            Description = desc,
            MimeType = "application/json"
        }));
    }

    /// Registers a resource model by utilizing method metadata and XML comments to form
    /// a structured representation of a resource. This method processes the method's
    /// parameters, retrieves their descriptions if available from XML documentation,
    /// and adds the information to an internal list of resource models for further use.
    /// <param name="method">
    /// The method info object providing metadata about the method being registered.
    /// </param>
    /// <param name="xmlName">
    /// The name of the XML documentation node associated with the method.
    /// </param>
    /// <param name="toolName">
    /// A string representing the name of the tool associated with the resource.
    /// </param>
    /// <param name="uri">
    /// The URI of the resource.
    /// </param>
    /// <param name="desc">
    /// A brief description of the resource.
    /// </param>
    /// <param name="http">
    /// An attribute providing information about the HTTP method associated with the resource.
    /// </param>
    private void RegisterResourceModel(MethodInfo method, string xmlName, string toolName, string uri, string desc,
        HttpMethodAttribute http)
    {
        var paramDescriptions = _xml.GetParamDescriptions(xmlName);
        var methodParams = new List<PropertyResourceModel>();

        foreach (var param in method.GetParameters())
        {
            var isComplex = !(param.ParameterType.IsPrimitive || param.ParameterType == typeof(string));

            if (isComplex)
            {
                // Сам параметр (например: "model")
                var paramModel = new PropertyResourceModel
                {
                    Name = param.Name!,
                    Type = IsArray(param.ParameterType) ? "array" : "object",
                    Description = paramDescriptions.TryGetValue(param.Name!, out var descText) ? descText : null,
                    Children = ExtractPropertiesFromType(param.ParameterType, new HashSet<Type>())
                };

                methodParams.Add(paramModel);
            }
            else
            {
                methodParams.Add(new PropertyResourceModel
                {
                    Name = param.Name!,
                    Type = param.ParameterType.Name,
                    Description = paramDescriptions.TryGetValue(param.Name!, out var descText) ? descText : null
                });
            }
        }

        _resourcesModel.Add(new ResourceModel
        {
            Uri = uri,
            Name = toolName,
            Description = desc,
            HttpMethod = http.HttpMethods.FirstOrDefault() ?? "GET",
            MimeType = "application/json",
            Params = methodParams
        });
    }

    /// Retrieves the list of tools registered with the operation registrar.
    /// <return>
    /// A read-only list of tools of type McpServerTool registered with the operation registrar.
    /// </return>
    public IReadOnlyList<McpServerTool> GetTools() => _tools;

    /// Retrieves the list of resources registered with the operation registrar.
    /// <returns>
    /// A read-only list of McpServerResource objects that represent the resources registered with the operation registrar.
    /// </returns>
    public IReadOnlyList<McpServerResource> GetResources() => _resources;

    /// Retrieves the collection of internal resource models that have been registered
    /// within the operation registrar. These resource models contain metadata about
    /// specific API resources, including their names, descriptions, URIs, HTTP methods,
    /// and associated properties.
    /// <returns>A read-only list of internal resource models.</returns>
    public IReadOnlyList<ResourceModel> GetInternalResources() => _resourcesModel;

    /// Registers a tool within the operation registrar.
    /// <param name="tool">
    /// The instance of <see cref="McpServerTool"/> to be registered. Represents the tool
    /// with its associated metadata and functionality to be added to the registrar.
    /// </param>
    public void RegisterTool(McpServerTool tool)
    {
        _tools.Add(tool);
    }

    private List<PropertyResourceModel> ExtractPropertiesFromType(Type type, HashSet<Type>? visited = null)
    {
        visited ??= new HashSet<Type>();

        if (visited.Contains(type))
            return new List<PropertyResourceModel>(); // prevent circular refs

        visited.Add(type);

        var properties = new List<PropertyResourceModel>();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            var propType = prop.PropertyType;
            var typeName = GetSimpleTypeName(propType);
            var description = _xml.GetSummary($"P:{prop.DeclaringType.FullName}.{prop.Name}");

            var model = new PropertyResourceModel
            {
                Name = prop.Name,
                Type = typeName,
                Description = description,
            };

            if (typeName == "object")
            {
                model.Children = ExtractPropertiesFromType(propType, visited);
            }
            else if (typeName == "array")
            {
                var elementType = GetElementType(propType);
                var elementTypeName = GetSimpleTypeName(elementType);

                if (elementTypeName == "object")
                    model.Children = ExtractPropertiesFromType(elementType, visited);
                else
                    model.Children = new List<PropertyResourceModel> {
                        new PropertyResourceModel {
                            Name = "item",
                            Type = elementTypeName,
                            Description = null
                        }
                    };
            }

            properties.Add(model);
        }

        return properties;
    }

    /// Determines whether the specified type is an array or implements the `IEnumerable<>` generic interface.
    /// <param name="type">The type to evaluate.</param>
    /// <returns>true if the specified type is an array or a generic enumerable; otherwise, false.
    private bool IsArray(Type type)
    {
        return type.IsArray || 
               (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
    }

    private Type GetElementType(Type type)
    {
        return type.IsArray
            ? type.GetElementType()!
            : type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
    }
    private string GetSimpleTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte)) return "integer";
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "number";
        if (type == typeof(DateTime) || type == typeof(Guid) || type == typeof(TimeSpan)) return "string";
        if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            return "array";
        if (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum))
            return "object";
        return "unknown";
    }
}