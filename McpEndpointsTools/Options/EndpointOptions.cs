namespace McpEndpointsTools.Options;

/// <summary>
/// Represents configuration options for an endpoint in the application.
/// </summary>
public class EndpointOptions
{
    /// <summary>
    /// Represents the relative URI template for an endpoint within the application.
    /// This property is used to define and retrieve the standardized path that serves
    /// as an access point for specific server-side functionality.
    /// The value should always follow the expected format of a URI path.
    /// </summary>
    public string Endpoint { get; set; } = "/resources";

    /// <summary>
    /// Represents the internally maintained name identifier for the endpoint configuration.
    /// This value is used as a unique descriptor for the endpoint within the application and
    /// is typically set to a meaningful and valid string that aligns with naming conventions.
    /// </summary>
    private string _name = "Tools-Endpoint";

    /// <summary>
    /// Represents the name associated with the endpoint configuration.
    /// This property is used to define and retrieve a valid identifier for the endpoint,
    /// ensuring it only contains letters, digits, and hyphens. Assigning an invalid
    /// value will result in an exception being thrown.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (!TrySetName(value))
                throw new ArgumentException("Name must contain only letters, digits, and hyphens.");
        }
    }

    /// Gets or sets the title of the endpoint.
    /// This property specifies a descriptive title for the endpoint,
    /// which can be used to identify it in a user interface or documentation.
    /// The title provides a clear, human-readable description
    /// of the purpose or function of the endpoint.
    public string Title { get; set; } = "Internal endpoints";

    /// Gets or sets the description of the endpoint.
    /// Provides detailed information about the purpose and functionality
    /// of the endpoint, including metadata such as parameter details,
    /// request and response expectations, and guidance on its usage in
    /// generating UI components or interacting with backend resources.
    public string Description { get; set; } = "\"You are a coding assistant whose goal is to generate UI code " +
                                          "components based on the backend API structure. You have access to a " +
                                          "list of tools representing HTTP endpoints of the application. " +
                                          "Each tool corresponds to a method on the server, and provides " +
                                          "information such as: the HTTP method (GET, POST, etc.), the URI " +
                                          "template of the endpoint, a description of what the endpoint does, " +
                                          "a list of parameters (with names, types, and detailed descriptions), " +
                                          "and the expected MIME type (e.g., application/json). " +
                                          "Each parameter may include nested fields if the parameter is a complex object. " +
                                          "These nested fields are represented using a 'children' property in the metadata. " +
                                          "You must recursively inspect all nested parameters. For every parameter and nested field, " +
                                          "you must take into account both its 'type' and its 'description'. " +
                                          "This is essential to generate meaningful and usable UI elements. " +
                                          "For example, if a parameter has type 'int' and description 'number of attempts', " +
                                          "the generated input should be numeric and clearly indicate its purpose. " +
                                          "If a parameter is an object with children, all children must be rendered " +
                                          "with appropriate input components that reflect their types and descriptions. " +
                                          "Your first task is to analyze all available tools and pick the most suitable " +
                                          "resource based on the user request. Use the endpoint’s description and " +
                                          "parameter metadata to understand what kind of data or functionality " +
                                          "the tool provides. Each parameter should be considered carefully. " +
                                          "Its name, type, and description should guide how to present it in the " +
                                          "UI (e.g., text input, dropdown, date picker, etc.). " +
                                          "If the list of tools is paginated and does not contain a suitable one, you should " +
                                          "make additional requests to the next pages until the relevant " +
                                          "resource is found. Your generated code should use the correct HTTP " +
                                          "method and endpoint URI, and submit or retrieve data in the correct " +
                                          "format based on the endpoint definition. The UI code you produce must " +
                                          "be complete, usable, and aligned with the backend functionality " +
                                          "described in the tool metadata.\"";

    /// <summary>
    /// Attempts to set the name of the endpoint. The name must only contain letters, digits, and hyphens.
    /// </summary>
    /// <param name="value">The proposed new name for the endpoint. It must not be null, empty, or contain invalid characters.</param>
    /// <returns>
    /// Returns true if the name is successfully set with valid characters; otherwise, false if the value is null,
    /// empty, or contains invalid characters.
    /// </returns>
    private bool TrySetName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        foreach (var c in value)
        {
            if (!(char.IsLetterOrDigit(c) || c == '-'))
                return false;
        }

        _name = value;
        return true;
    }
}