using System.Reflection;

namespace McpEndpointsTools.Options;

/// <summary>
/// Represents configuration options for the server.
/// </summary>
public class ServerOptions
{
    /// <summary>
    /// Represents the underlying private field for storing the MCP endpoint used for server routing configuration.
    /// This field is initialized with the default value "/mcp".
    /// </summary>
    private string _mcpEndpoint = "/mcp";
    /// <summary>
    /// Gets or sets the MCP endpoint used for server routing.
    /// This property defines the base path for MCP-related endpoints in the application.
    /// The default value is "/mcp".
    /// </summary>
    public string McpEndpoint
    {
        get => _mcpEndpoint;
        set
        {
            var trimmed = value?.Trim() ?? "/mcp";
            _mcpEndpoint = trimmed.TrimEnd('/');
            if (string.IsNullOrEmpty(_mcpEndpoint))
                _mcpEndpoint = "/mcp";
        }
    }

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
    /// The default value is "1.0.0".
    /// </summary>
    public string ServerVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Stores the file path to the XML documentation file used for generating API documentation.
    /// This field is resolved based on the application's entry assembly or manually set to a specific path.
    /// </summary>
    private string _xmlCommentsPath = string.Empty;

    /// <summary>
    /// Gets or sets the file path to the XML comments file used for API documentation.
    /// This property specifies the location of the XML file that contains the
    /// documentation comments for the application. If not explicitly set, the
    /// default behavior resolves it to the XML file associated with the application's entry assembly.
    /// </summary>
    public string XmlCommentsPath
    {
        get => _xmlCommentsPath;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _xmlCommentsPath = value.Trim();
            }
            else
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                    throw new InvalidOperationException(
                        "Cannot resolve XML comments path automatically because entry assembly is null.");

                _xmlCommentsPath = Path.ChangeExtension(entryAssembly.Location, ".xml");
            }
        }
    }

    /// <summary>
    /// Represents the underlying private field for storing the base URL of the host server configuration.
    /// This field is utilized to validate and retain a properly formatted URL that starts with "http://" or "https://".
    /// </summary>
    private string _hostUrl = string.Empty;

    /// <summary>
    /// Represents the base URL of the server. This property must be a valid URL starting with either "http://" or "https://".
    /// It trims any trailing slashes, ensuring the URL is consistent, and throws an exception if the input is null, empty,
    /// or does not follow the required format.
    /// </summary>
    public string HostUrl
    {
        get => _hostUrl;
        set
        {
            var input = value?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new InvalidOperationException("HostUrl cannot be null or empty. Please provide a valid base URL (e.g., https://example.com).");
            }

            if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("HostUrl must start with http:// or https://");
            }

            _hostUrl = input.TrimEnd('/');
        }
    }
}