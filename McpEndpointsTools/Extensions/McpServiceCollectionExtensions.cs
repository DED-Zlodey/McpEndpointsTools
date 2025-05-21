using System.Reflection;
using McpEndpointsTools.Infrastructure;
using McpEndpointsTools.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;

namespace McpEndpointsTools.Extensions;

/// <summary>
/// Provides extension methods for configuring and adding MCP endpoint services to the application's dependency injection container.
/// </summary>
public static class McpServiceCollectionExtensions
{
    /// <summary>
    /// Configures and adds MCP (Model Context Protocol) endpoint services to the specified service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configure">
    /// An optional action to configure the <see cref="ServerOptions"/> used to customize the MCP server behavior.
    /// </param>
    /// <returns>
    /// An <see cref="IMcpServerBuilder"/> instance that can be used to further configure the MCP server.
    /// </returns>
    public static IMcpServerBuilder AddMcpEndpointsServer(this IServiceCollection services,
        Action<ServerOptions>? configure = null)
    {
        var opts = ConfigureServerOptions(services, configure);

        RegisterXmlCommentsProvider(services);
        RegisterOperationRegistrar(services);
        var builder = CreateMcpServerBuilder(services, opts);

        RegisterToolsAndResources(services, builder);

        return builder;
    }

    /// <summary>
    /// Configures server options for the MCP server and optionally applies the provided configuration action.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the server options are added.</param>
    /// <param name="configure">
    /// An optional action to configure the <see cref="ServerOptions"/> for customizing the MCP server's behavior.
    /// </param>
    /// <returns>
    /// A configured <see cref="ServerOptions"/> instance containing the applied settings.
    /// </returns>
    private static ServerOptions ConfigureServerOptions(IServiceCollection services, Action<ServerOptions>? configure)
    {
        var opts = new ServerOptions();
        configure?.Invoke(opts);
        if (configure != null)
            services.Configure(configure);
        return opts;
    }

    /// <summary>
    /// Registers a provider for XML comments used in the application to support documentation or metadata generation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> where the XML comments provider will be registered.</param>
    private static void RegisterXmlCommentsProvider(IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var resolvedOpts = sp.GetRequiredService<IOptions<ServerOptions>>().Value;
            var xmlPath = string.IsNullOrWhiteSpace(resolvedOpts.XmlCommentsPath)
                ? Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".xml")
                : resolvedOpts.XmlCommentsPath;

            return new XmlCommentsProvider(xmlPath);
        });
    }

    /// <summary>
    /// Registers the <see cref="OperationRegistrar"/> in the application's dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the <see cref="OperationRegistrar"/> will be added.</param>
    private static void RegisterOperationRegistrar(IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var xml = sp.GetRequiredService<XmlCommentsProvider>();
            var registrar = new OperationRegistrar(sp, xml);
            registrar.ScanAssembly(Assembly.GetEntryAssembly()!);
            return registrar;
        });
    }

    /// <summary>
    /// Creates an instance of <see cref="IMcpServerBuilder"/> configured with the specified services and server options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> containing the application's service definitions.</param>
    /// <param name="opts">The configured <see cref="ServerOptions"/> used to customize the MCP server behavior.</param>
    /// <returns>
    /// An <see cref="IMcpServerBuilder"/> instance for further server configuration.
    /// </returns>
    private static IMcpServerBuilder CreateMcpServerBuilder(IServiceCollection services, ServerOptions opts)
    {
        return services.AddMcpServer(sdkOpts =>
        {
            sdkOpts.ServerInfo = new Implementation
            {
                Name = opts.ServerName,
                Version = opts.ServerVersion
            };
            sdkOpts.ServerInstructions = opts.ServerDescription;
        });
    }

    /// <summary>
    /// Registers tools and resources for the MCP server by configuring the operation registrar,
    /// adding built-in tools, and finalizing the registration with tools, resources, and transport options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance used to manage service dependencies.</param>
    /// <param name="opts">The <see cref="ServerOptions"/> containing configuration details for the server and its endpoints.</param>
    /// <param name="builder">
    /// The <see cref="IMcpServerBuilder"/> instance used to configure and finalize the MCP server setup with tools and resources.
    /// </param>
    private static void RegisterToolsAndResources(IServiceCollection services, IMcpServerBuilder builder)
    {
        var sp = services.BuildServiceProvider();
        var registrar = sp.GetRequiredService<OperationRegistrar>();

        // Finalize registration
        builder
            .WithTools(registrar.GetTools())
            .WithHttpTransport();
    }
}