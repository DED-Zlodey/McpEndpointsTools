using System.Reflection;
using McpEndpointsTools.Attributes;
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
        var controllers = asm.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                        (t.GetCustomAttribute<ApiControllerAttribute>() != null ||
                         typeof(ControllerBase).IsAssignableFrom(t)));

        foreach (var ctrlType in controllers)
        {
            var classRoute = ctrlType.Name.Replace("Controller", "") ?? ctrlType
                                 .GetCustomAttribute<RouteAttribute>()?
                                 .Template;
            
            var instance = ActivatorUtilities.CreateInstance(_sp, ctrlType);

            var methods = ctrlType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<McpIgnoreAttribute>() != null)
                    continue;

                var http = method.GetCustomAttributes()
                    .OfType<HttpMethodAttribute>()
                    .FirstOrDefault();
                if (http == null)
                    continue;
                
                var methodRoute = http.Template ?? string.Empty;
                
                var xmlName = XmlCommentsNameHelper.GetMemberNameForMethod(method);
                
                var desc = _xml.GetSummary(xmlName) ?? "";

                var controllerPart = ctrlType.Name
                    .Replace("Controller", "", StringComparison.Ordinal)
                    .ToLowerInvariant();
                
                var methodPart = method.Name.ToLowerInvariant();

                var toolName = $"{controllerPart}-{methodPart}";
                
                var opts = new McpServerToolCreateOptions
                {
                    Name = toolName,
                    Description = desc,
                    Title = toolName,
                };

                var tool = McpServerTool.Create(method, instance, opts);
                

                _tools.Add(tool);

                if (classRoute != null)
                {
                    var routeTemplate = string.Join("/",
                        new[] { classRoute.Trim('/'), methodRoute.Trim('/') }
                            .Where(p => !string.IsNullOrEmpty(p))
                    );
                
                    var absolute = string.IsNullOrEmpty(routeTemplate) ? $"{_basePath}"
                        : $"{_basePath}/{routeTemplate}";
                    
                    var resOpts = new McpServerResourceCreateOptions
                    {
                        UriTemplate = absolute,
                        Name = $"{toolName}-resource",
                        Description = desc,
                        MimeType = "application/json",
                    };
                    
                    _resources.Add(McpServerResource.Create(method, instance, resOpts));
                }
            }
        }
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
}