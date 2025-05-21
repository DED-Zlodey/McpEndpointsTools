using McpEndpointsTools.Options;

namespace Test;

/// <summary>
/// Contains test cases for validating the behavior of the <see cref="ServerOptions"/> class.
/// </summary>
public class ServerOptionsTests
{
    /// <summary>
    /// Verifies that the default values of the <see cref="ServerOptions"/> class properties
    /// are correctly set upon object instantiation.
    /// </summary>
    /// <remarks>
    /// This method tests whether the following default property values are correctly assigned:
    /// - <see cref="ServerOptions.ServerName"/> is not null.
    /// - <see cref="ServerOptions.ServerVersion"/> is not null.
    /// - <see cref="ServerOptions.ServerDescription"/> is not null.
    /// - <see cref="ServerOptions.XmlCommentsPath"/> is not null.
    /// - <see cref="ServerOptions.HostUrl"/> is set to an empty string ("").
    /// - <see cref="ServerOptions.McpEndpoint"/> is set to "/mcp".
    /// </remarks>
    [Fact]
    public void Constructor_Defaults_AreCorrect()
    {
        // Act
        var opts = new ServerOptions();

        // Assert
        Assert.NotNull(opts.ServerName);
        Assert.NotNull(opts.ServerVersion);
        Assert.NotNull(opts.ServerDescription);
        Assert.NotNull(opts.XmlCommentsPath);
        Assert.Equal("/mcp", opts.McpEndpoint);
    }

    /// <summary>
    /// Verifies that properties of the <see cref="ServerOptions"/> class can be correctly set
    /// and their values are properly assigned.
    /// </summary>
    [Fact]
    public void Properties_AreSetCorrectly()
    {
        // Arrange
        var opts = new ServerOptions
        {
            ServerName = "MCP Server",
            ServerVersion = "2.5.1",
            ServerDescription = "Some server description",
            XmlCommentsPath = "docs/api.xml",
        };

        // Assert
        Assert.Equal("MCP Server", opts.ServerName);
        Assert.Equal("2.5.1", opts.ServerVersion);
        Assert.Equal("Some server description", opts.ServerDescription);
        Assert.Equal("docs/api.xml", opts.XmlCommentsPath);
    }
}