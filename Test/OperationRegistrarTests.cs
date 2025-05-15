using McpEndpointsTools.Infrastructure;
using McpEndpointsTools.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

/// <summary>
/// Contains unit tests for ensuring that the OperationRegistrar class
/// properly registers tools and resources by scanning assemblies.
/// It verifies correct behavior, including handling of included and excluded components
/// during the registration process.
/// </summary>
public class OperationRegistrarTests
{
    /// <summary>
    /// Creates a mock <see cref="XmlCommentsProvider"/> instance by generating a temporary XML document
    /// containing documentation comments for a specified method based on its name and summary description.
    /// </summary>
    /// <param name="methodName">The unique identifier or name of the method for which the XML documentation is being created.</param>
    /// <param name="summary">The summary description text to include in the XML documentation for the method.</param>
    /// <returns>An instance of <see cref="XmlCommentsProvider"/> initialized with the generated XML documentation.</returns>
    private static XmlCommentsProvider CreateFakeXmlProvider(string methodName, string summary)
    {
        var xml = $"""
                       <doc>
                         <members>
                           <member name="{methodName}">
                             <summary>{summary}</summary>
                           </member>
                         </members>
                       </doc>
                   """;

        var tempPath = Path.GetTempFileName();
        File.WriteAllText(tempPath, xml);
        return new XmlCommentsProvider(tempPath);
    }

    /// <summary>
    /// Validates that the ScanAssembly method correctly registers tools and resources
    /// by scanning the provided assembly and ensuring corresponding protocol definitions
    /// are added for discovered actions and controllers.
    /// </summary>
    [Fact]
    public void ScanAssembly_RegistersToolsAndResources()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<FakeController>();
        var provider = services.BuildServiceProvider();

        var method = typeof(FakeController).GetMethod(nameof(FakeController.TestAction))!;
        var xmlName = XmlCommentsNameHelper.GetMemberNameForMethod(method);
        var xml = CreateFakeXmlProvider(xmlName, "Test summary");

        var registrar = new OperationRegistrar(provider, xml, "/api");

        // Act
        registrar.ScanAssembly(typeof(FakeController).Assembly);

        // Assert
        var tools = registrar.GetTools();
        var resources = registrar.GetResources();

        Assert.Single(tools);
        Assert.Single(resources);

        var tool = tools.First();
        var res = resources.First();

        Assert.Equal("fake-testaction", tool.ProtocolTool.Name);
        Assert.Equal("Test summary", tool.ProtocolTool.Description);
        Assert.Equal("/api/Fake/test", res.ProtocolResource!.Uri);
    }

    /// <summary>
    /// Verifies that the ScanAssembly method correctly excludes controllers or methods
    /// that are marked with the McpIgnore attribute from being registered in tools
    /// or resources during the assembly scanning process.
    /// </summary>
    [Fact]
    public void ScanAssembly_IgnoresMarkedWithMcpIgnore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IgnoredController>();
        var provider = services.BuildServiceProvider();

        var dummyXml = CreateFakeXmlProvider("dummy", "ignored");
        var registrar = new OperationRegistrar(provider, dummyXml, "/base");

        // Act
        registrar.ScanAssembly(typeof(IgnoredController).Assembly);

        // Assert
        var tools = registrar.GetTools();
        var resources = registrar.GetResources();

        Assert.DoesNotContain(tools,
            t => t.ProtocolTool.Name.StartsWith("ignoredcontroller", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resources,
            r => r.ProtocolResource!.Name.StartsWith("ignoredcontroller", StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// A controller class used for handling API requests under the "fake" route.
/// </summary>
[ApiController]
[Route("fake")]
public class FakeController : ControllerBase
{
    /// <summary>
    /// Handles the "test" HTTP GET request for the "fake" route.
    /// Responds with an HTTP 200 OK status code.
    /// </summary>
    /// <returns>An IActionResult representing the response of the operation.</returns>
    [HttpGet("test")]
    public IActionResult TestAction() => Ok();
}

/// <summary>
/// A controller marked for exclusion from specific processing or registration tools.
/// </summary>
/// <remarks>
/// This controller is decorated with the McpIgnore attribute, which prevents it from being scanned
/// or included in automated endpoint registration mechanisms.
/// </remarks>
[ApiController]
[McpIgnore]
[Route("ignored")]
public class IgnoredController : ControllerBase
{
    /// <summary>
    /// Handles the "skip" HTTP GET request for the "ignored" route.
    /// Responds with an HTTP 200 OK status code.
    /// </summary>
    /// <returns>An IActionResult representing the response of the operation.</returns>
    [HttpGet("skip")]
    public IActionResult Skip() => Ok();
}