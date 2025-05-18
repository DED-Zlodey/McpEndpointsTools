using System.Reflection;
using McpEndpointsTools.Attributes;
using McpEndpointsTools.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace McpEndpointsTools.Infrastructure;

/// Represents an operation registrar responsible for scanning assemblies
/// to discover and register tools and resources in the server context.
public class OperationRegistrar
{
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
    

    /// Represents an instance of the IServiceScopeFactory used to create new service scope instances.
    /// It facilitates dependency injection for resolving scoped services during the registration
    /// and initialization of controllers or related resources. This enables each operation or
    /// registration process to utilize its own dependency scope.
    private readonly IServiceScopeFactory _scopeFactory;

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
        _xml = xmlCommentsProvider;
        _basePath = basePath.TrimEnd('/');
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
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
            var methods = GetValidMethods(ctrlType);

            using var scope = _scopeFactory.CreateScope();
            var instance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, ctrlType);
            foreach (var method in methods)
            {
                var http = GetHttpMethod(method);
                if (http == null) continue;

                var methodRoute = http.Template
                                  ?? method.GetCustomAttribute<RouteAttribute>()?.Template
                                  ?? string.Empty;
                var xmlName = XmlCommentsNameHelper.GetMemberNameForMethod(method);
                var desc = _xml.GetSummary(xmlName) ?? string.Empty;
                var controllerPart = ctrlType.Name.Replace("Controller", "", StringComparison.Ordinal)
                    .ToLowerInvariant();

                if (!string.IsNullOrEmpty(classRoute))
                {
                    var absoluteUri = CombineRoutes(_basePath, classRoute, methodRoute);
                    var toolName = GenerateToolName(controllerPart, method);

                    // Create handler
                    var handler = new EndpointHandler(_scopeFactory, ctrlType, method);
                    var handleMethod = typeof(EndpointHandler).GetMethod(nameof(EndpointHandler.Handle));

                    var opts = new McpServerToolCreateOptions
                    {
                        Name = toolName,
                        Description = desc,
                        Title = toolName
                    };
                    
                    if (handleMethod != null)
                    {
                        var universalTool = CreateUniversalTool(handleMethod, handler, opts);
                        var schemaTool = McpServerTool.Create(method, instance, opts);
                        var finalTool = new McpServerToolWithDifferentImpl(universalTool, schemaTool);
                        _tools.Add(finalTool);
                    }
                }
            }
        }
    }

    /// Creates a universal tool that wraps an endpoint handler method and associates it with
    /// the provided options for creating a tool within the server context. This method determines
    /// whether the handler method is asynchronous or synchronous and creates the appropriate delegate
    /// for tool execution.
    /// <param name="handleMethod">
    /// The endpoint handler method, represented by a MethodInfo instance, that implements the logic of the tool.
    /// </param>
    /// <param name="handler">
    /// The object instance containing the handler method. This is used as the target when creating the delegate for the method.
    /// </param>
    /// <param name="opts">
    /// Options for creating the McpServerTool, including metadata such as the tool's name, title, and description.
    /// </param>
    /// <returns>
    /// A new instance of McpServerTool, representing the universal tool that was created based on the provided handler method and options.
    /// </returns>
    private McpServerTool CreateUniversalTool(MethodInfo handleMethod, object handler, McpServerToolCreateOptions opts)
    {
        bool isAsync = typeof(Task).IsAssignableFrom(handleMethod.ReturnType);
        if (isAsync)
        {
            var del = (Func<RequestContext<CallToolRequestParams>, Task<object?>>)
                handleMethod.CreateDelegate(typeof(Func<RequestContext<CallToolRequestParams>, Task<object?>>), handler);
            return McpServerTool.Create(del, opts);
        }
        else
        {
            var del = (Func<RequestContext<CallToolRequestParams>, object?>)
                handleMethod.CreateDelegate(typeof(Func<RequestContext<CallToolRequestParams>, object?>), handler);
            return McpServerTool.Create(del, opts);
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
    private string GetControllerRoute(Type controllerType)
    {
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        if (routeAttr != null)
        {
            var template = routeAttr.Template;
            if (!string.IsNullOrWhiteSpace(template))
            {
                var controllerName = controllerType.Name.Replace("Controller", "", StringComparison.OrdinalIgnoreCase);
                return template.Replace("[controller]", controllerName.ToLowerInvariant());
            }
        }

        // По умолчанию: имя контроллера
        return controllerType.Name.Replace("Controller", "", StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
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

    /// Retrieves the list of tools registered with the operation registrar.
    /// <return>
    /// A read-only list of tools of type McpServerTool registered with the operation registrar.
    /// </return>
    public IReadOnlyList<McpServerTool> GetTools() => _tools;
}