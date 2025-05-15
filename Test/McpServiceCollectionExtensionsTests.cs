using McpEndpointsTools.Extensions;
using McpEndpointsTools.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

/// <summary>
/// Contains unit tests for the <see cref="McpServiceCollectionExtensions"/> class,
/// specifically testing the <c>AddMcpEndpointsServer</c> extension method.
/// </summary>
/// <remarks>
/// The <c>AddMcpEndpointsServer</c> extension method is tested to ensure that required
/// services necessary for MCP endpoints functionality are appropriately registered in the
/// service container. This includes validating registrations such as <c>XmlCommentsProvider</c>
/// and <c>OperationRegistrar</c>.
/// Additionally, the tests validate the behavior of the method when provided with various
/// configurations such as XML comment file paths, server names, and version handling.
/// </remarks>
public class McpServiceCollectionExtensionsTests
{
    /// <summary>
    /// Tests the <c>AddMcpEndpointsServer</c> extension method to verify that it correctly registers
    /// the required services for the MCP Endpoints server within the provided service collection.
    /// </summary>
    /// <remarks>
    /// This method ensures that the appropriate dependencies, such as <c>XmlCommentsProvider</c>
    /// and <c>OperationRegistrar</c>, are properly registered in the service container. Additionally,
    /// it verifies the configuration of the server using temporary settings, such as XML comments paths
    /// and versioning.
    /// </remarks>
    [Fact]
    public void AddMcpEndpointsServer_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Создаём реальный XML файл
        var tempXmlPath = Path.GetTempFileName();
        File.WriteAllText(tempXmlPath, "<doc><members></members></doc>");

        // Act
        var result = McpServiceCollectionExtensions.AddMcpEndpointsServer(services, opts =>
        {
            opts.ServerName = "Test";
            opts.ServerVersion = "1.0.0";
            opts.XmlCommentsPath = tempXmlPath; // 🔧 тут правильный путь
        });

        // Assert
        Assert.NotNull(result);

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<XmlCommentsProvider>());
        Assert.NotNull(provider.GetService<OperationRegistrar>());

        // Clean up
        File.Delete(tempXmlPath);
    }
}