using McpEndpointsTools.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Test;

/// <summary>
/// Provides a suite of unit tests for verifying the behavior of the <see cref="McpEndpointRouteBuilderExtensions"/> class.
/// </summary>
/// <remarks>
/// This test class is designed to validate the proper functionality of the extension methods in <see cref="McpEndpointRouteBuilderExtensions"/>,
/// including error scenarios where dependency injection setup is incomplete.
/// </remarks>
public class McpEndpointRouteBuilderExtensionsTests
{
    /// <summary>
    /// Tests that an <see cref="InvalidOperationException"/> is thrown when attempting to map MCP server endpoints
    /// using <see cref="McpEndpointRouteBuilderExtensions.MapMcpEndpointsServer"/> without registering the required options
    /// via dependency injection.
    /// </summary>
    /// <remarks>
    /// This method verifies that the method <see cref="McpEndpointRouteBuilderExtensions.MapMcpEndpointsServer"/>
    /// depends on the <see cref="IOptions{TOptions}"/> for <see cref="ServerOptions"/> being registered in
    /// the dependency injection container. If these options are not registered, the method should throw an exception.
    /// </remarks>
    [Fact]
    public void MapMcpEndpointsServer_ThrowsIfOptionsNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var endpointBuilderMock = new Mock<IEndpointRouteBuilder>();
        endpointBuilderMock.SetupGet(e => e.ServiceProvider).Returns(provider);

        var endpoints = endpointBuilderMock.Object;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            McpEndpointRouteBuilderExtensions.MapMcpEndpointsServer(endpoints));
    }
}
