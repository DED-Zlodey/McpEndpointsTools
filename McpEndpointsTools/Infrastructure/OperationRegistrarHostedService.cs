using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace McpEndpointsTools.Infrastructure;

/// Represents a hosted service that facilitates the registration of operations
/// by utilizing the `OperationRegistrar` during the application's startup lifecycle.
internal class OperationRegistrarHostedService : IHostedService
{
    /// Holds an instance of the `OperationRegistrar` class which is responsible for
    /// scanning assemblies to discover and register tools, resources, and API endpoints
    /// in the server application.
    private readonly OperationRegistrar _registrar;

    /// A hosted service responsible for registering operations by scanning the entry assembly
    /// during the application's startup phase.
    /// This service interacts with the `OperationRegistrar` to scan for controllers and actions,
    /// extracting metadata, and preparing tools and resources that describe available API endpoints.
    public OperationRegistrarHostedService(OperationRegistrar registrar)
        => _registrar = registrar;

    /// Starts the hosted service and performs the required operation registration.
    /// When invoked, it scans the entry assembly for specified attributes, controllers, or tools
    /// and registers associated metadata for application use.
    /// <param name="ct">A cancellation token that can be used to signal cancellation of the operation.</param>
    /// <return>A task representing the asynchronous operation.</return>
    public Task StartAsync(CancellationToken ct)
    {
        _registrar.ScanAssembly(Assembly.GetEntryAssembly()!);
        return Task.CompletedTask;
    }

    /// Stops the hosted service and performs necessary cleanup operations.
    /// This method is invoked during the shutdown process.
    /// <param name="ct">
    /// A cancellation token that indicates whether the stop operation should be canceled.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous stop operation.
    /// </returns>
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}