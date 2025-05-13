using System.Xml.Linq;

namespace McpEndpointsTools.Infrastructure;

/// <summary>
/// Provides functionality to extract XML documentation comments for members from a specified XML documentation file.
/// </summary>
public class XmlCommentsProvider
{
    /// <summary>
    /// Represents the XML document containing comments for members, such as methods,
    /// parameters, and types, extracted from an XML documentation file.
    /// </summary>
    /// <remarks>
    /// This field holds the parsed XML content used to retrieve documentation
    /// summaries, parameter descriptions, and other metadata for code elements.
    /// </remarks>
    private readonly XDocument _xml;

    /// Provides functionality to parse and retrieve XML documentation comments for members.
    /// This class reads an XML documentation file and enables access to summaries and parameter descriptions.
    public XmlCommentsProvider(string xmlPath)
    {
        if (!File.Exists(xmlPath))
            throw new FileNotFoundException("XML comments file not found", xmlPath);
        _xml = XDocument.Load(xmlPath);
    }

    /// Retrieves the summary documentation for a specified member from an XML documentation file.
    /// <param name="memberName">
    /// The name of the member for which the summary documentation is to be retrieved.
    /// The member name should be in the format used by XML documentation (e.g., "T:Namespace.ClassName").
    /// </param>
    /// <returns>
    /// The summary text from the XML documentation file for the specified member, or null if the member is not found or has no summary documentation.
    /// </returns>
    public string? GetSummary(string memberName)
    {
        var member = _xml.Root?
            .Element("members")?
            .Elements("member")
            .FirstOrDefault(x => x.Attribute("name")?.Value == memberName);
        var summary = member?.Element("summary")?.Value;
        return summary?.Trim();
    }

    /// Retrieves the parameter descriptions for a specified member from the XML comments.
    /// <param name="memberName">
    /// The fully qualified name of the member to retrieve parameter descriptions for.
    /// </param>
    /// <returns>
    /// A dictionary where the key is the parameter name and the value is the parameter description.
    /// If no descriptions are found, an empty dictionary is returned.
    /// </returns>
    public Dictionary<string, string> GetParamDescriptions(string memberName)
    {
        var member = _xml.Root?
            .Element("members")?
            .Elements("member")
            .FirstOrDefault(x => x.Attribute("name")?.Value == memberName);
        if (member == null) return new();

        return member.Elements("param")
            .Where(x => x.Attribute("name") != null)
            .ToDictionary(
                x => x.Attribute("name")!.Value,
                x => x.Value.Trim()
            );
    }
}