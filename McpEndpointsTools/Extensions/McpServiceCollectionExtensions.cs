using System.Reflection;
using McpEndpointsTools.Infrastructure;
using McpEndpointsTools.Options;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol.Types;

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


        var xmlPath = string.IsNullOrWhiteSpace(opts.XmlCommentsPath)
            ? Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".xml")
            : opts.XmlCommentsPath;

        services.AddSingleton(new XmlCommentsProvider(xmlPath));

        services.AddSingleton<OperationRegistrar>(sp =>
            new OperationRegistrar(sp, sp.GetRequiredService<XmlCommentsProvider>(), opts.HostUrl)
        );

        var sp = services.BuildServiceProvider();
        var registrar = sp.GetRequiredService<OperationRegistrar>();

        registrar.ScanAssembly(Assembly.GetEntryAssembly()!);

        services.AddHostedService<OperationRegistrarHostedService>();


        var builder = services.AddMcpServer(sdkOpts =>
            {
                sdkOpts.ServerInfo = new Implementation()
                {
                    Name = opts.ServerName,
                    Version = opts.ServerVersion
                };
                sdkOpts.ServerInstructions = opts.ServerDescription;
            })
            .WithTools(registrar.GetTools())
            .WithResources(registrar.GetResources())
            .WithListResourcesHandler((ctx, _) =>
            {
                if (ctx.Services != null)
                {
                    var reg = ctx.Services.GetRequiredService<OperationRegistrar>();
                    var resources = reg.GetResources()
                        .Select(r => r.ProtocolResource!)
                        .ToList();

                    var result = new ListResourcesResult
                    {
                        Resources = resources
                    };
                    return ValueTask.FromResult(result);
                }

                return ValueTask.FromResult(new ListResourcesResult());
            })
            .WithHttpTransport();

        return builder;
    }
}