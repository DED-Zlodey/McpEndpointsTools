namespace McpEndpointsTools.Options;

/// <summary>
/// Represents configuration options for the server.
/// </summary>
public class ServerOptions
{
    /// <summary>
    /// Gets or sets the endpoint path for the MCP pipeline.
    /// This property defines the base URL path for MCP-related endpoints in the application,
    /// such as for HTTP streaming or Server-Sent Events (SSE).
    /// Default value is "/mcp".
    /// </summary>
    public string PipelineEndpoint { get; set; } = "/mcp";

    /// <summary>
    /// Gets or sets the name of the server.
    /// This property specifies the server's name, which can be utilized in various ways,
    /// such as displaying in API documentation or returning in server information responses.
    /// Default value is "MCP Server".
    /// </summary>
    public string ServerName { get; set; } = "MCP Server";

    /// <summary>
    /// Gets or sets the description of the server.
    /// This property provides a textual representation describing the purpose or functionality
    /// of the server. It is typically used for documentation or identification purposes.
    /// Default value is "MCP Server for ASP.NET Core".
    /// </summary>
    public string ServerDescription { get; set; } = "MCP Server for ASP.NET Core";

    /// <summary>
    /// Gets or sets the version of the server.
    /// Represents the version number of the MCP server that conforms to the semantic versioning format (e.g., "1.0.0").
    /// This value is utilized for identifying the server build and can be used in logging or diagnostics.
    /// Default value is "1.0.0".
    /// </summary>
    public string ServerVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the file path to the XML documentation comments file.
    /// This property specifies the location of the XML file containing comments generated from
    /// code documentation, which can be used to enrich services like Swagger with detailed API descriptions.
    /// The path is configurable and should typically point to the XML output of the application's compiled assembly.
    /// Default value is an empty string.
    /// </summary>
    public string XmlCommentsPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host URL for the server.
    /// This property specifies the base URL that the server will use to host its endpoints.
    /// Default value is an empty string.
    /// </summary>
    public string HostUrl { get; set; } = "";
}