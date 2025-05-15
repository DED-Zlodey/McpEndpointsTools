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
    /// Adds and configures an MCP Endpoints server to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the server to.</param>
    /// <param name="configure">
    /// An optional action to configure the <see cref="ServerOptions"/> for the MCP Endpoints server.
    /// </param>
    /// <returns>An <see cref="IMcpServerBuilder"/> instance to further configure the MCP server.</returns>
    public static IMcpServerBuilder AddMcpEndpointsServer(this IServiceCollection services,
        Action<ServerOptions>? configure = null)
    {
        var opts = new ServerOptions();
        configure?.Invoke(opts);
        if (configure != null)
            services.Configure(configure);
        
        services.AddSingleton(sp =>
        {
            var resolvedOpts = sp.GetRequiredService<IOptions<ServerOptions>>().Value;
            var xmlPath = string.IsNullOrWhiteSpace(resolvedOpts.XmlCommentsPath)
                ? Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".xml")
                : resolvedOpts.XmlCommentsPath;
            return new XmlCommentsProvider(xmlPath);
        });
        
        services.AddSingleton<OperationRegistrar>(sp =>
        {
            var xml = sp.GetRequiredService<XmlCommentsProvider>();
            var resolvedOpts = sp.GetRequiredService<IOptions<ServerOptions>>().Value;
        
            var registrar = new OperationRegistrar(sp, xml, resolvedOpts.HostUrl);
            registrar.ScanAssembly(Assembly.GetEntryAssembly()!);
            return registrar;
        });
        
        var builder = services.AddMcpServer(sdkOpts =>
        {
            sdkOpts.ServerInfo = new Implementation
            {
                Name = opts.ServerName,
                Version = opts.ServerVersion
            };
            sdkOpts.ServerInstructions = opts.ServerDescription;
        });
        
        var sp = services.BuildServiceProvider();
        var registrar = sp.GetRequiredService<OperationRegistrar>();

        builder
            .WithTools(registrar.GetTools())
            .WithResources(registrar.GetResources())
            .WithHttpTransport();

        return builder;
    }
}