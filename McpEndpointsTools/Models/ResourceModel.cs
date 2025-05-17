namespace McpEndpointsTools.Models;

/// <summary>
/// Represents a model containing metadata and details about a specific resource.
/// </summary>
public class ResourceModel
{
    /// Gets or sets the unique identifier or name of the resource.
    /// This property represents the name assigned to a resource, used to distinguish it within the system.
    /// It is typically used in conjunction with other resource properties to define metadata for the resource.
    public string Name { get; set; } = null!;

    /// Gets or sets the description of the resource.
    /// This property provides a textual explanation or details about
    /// the resource, which may include its purpose, behavior, or any
    /// other relevant information.
    public string Description { get; set; } = null!;

    /// Represents the URI associated with the resource. This property contains
    /// the absolute URI that specifies the endpoint location for the resource.
    /// It is used to define the unique address or route for interacting with
    /// the resource in an application, often in the context of HTTP operations.
    public string Uri { get; set; } = null!;

    /// <summary>
    /// Represents the HTTP method (such as GET, POST, PUT, DELETE) associated with a resource or endpoint.
    /// </summary>
    /// <remarks>
    /// This property is used to specify the HTTP method required to interact with a given resource.
    /// It typically corresponds to standard HTTP methods used in RESTful APIs, denoting the type of operation to perform.
    /// </remarks>
    public string HttpMethod { get; set; } = null!;

    /// <summary>
    /// Gets or sets the internet media type (MIME type) associated with the resource.
    /// </summary>
    /// <remarks>
    /// A MIME type is a standard that indicates the nature and format of a file or resource,
    /// such as "application/json" or "text/html". This property is often used to specify the format
    /// in which the content is presented or exchanged over HTTP.
    /// </remarks>
    public string MimeType { get; set; } = null!;

    /// Represents a collection of property resources associated with a particular API resource.
    /// This property is used to define metadata about the included property models,
    /// such as their names, types, and optional descriptions, typically describing the parameters
    /// or attributes of the API resource.
    /// The `PropertiesResource` is commonly used within API documentation or tools generation
    /// processes to provide insight into the parameters or data fields utilized by the resource.
    public List<PropertyResourceModel>? Params { get; set; }
}