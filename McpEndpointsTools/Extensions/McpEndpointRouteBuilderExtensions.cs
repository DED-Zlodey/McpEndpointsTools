using McpEndpointsTools.Infrastructure;
using McpEndpointsTools.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace McpEndpointsTools.Extensions;

/// <summary>
/// Provides extension methods for configuring MCP-specific endpoint routing in an ASP.NET Core application.
/// </summary>
public static class McpEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the MCP server endpoints to the specified <see cref="IEndpointRouteBuilder"/> using the configured options.
    /// </summary>
    /// <param name="endpoints">
    /// The endpoint route builder to which the MCP server endpoints are mapped.
    /// </param>
    /// <returns>
    /// The <see cref="IEndpointRouteBuilder"/> instance with MCP server endpoints mapped.
    /// </returns>
    public static IEndpointRouteBuilder MapMcpEndpointsServer(this IEndpointRouteBuilder endpoints)
    {
        var opts = endpoints
            .ServiceProvider
            .GetRequiredService<IOptions<ServerOptions>>()
            .Value;
        

        var instance = endpoints.ServiceProvider.GetRequiredService<ToolsEndpointHandler>();
        
        endpoints.MapGet($"{opts.McpEndpoint}{opts.EndpointOptions.Endpoint}", instance.Handle);

        endpoints.MapMcp(opts.McpEndpoint);

        return endpoints;
    }
}