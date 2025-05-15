using McpEndpointsTools.Infrastructure;

namespace Test;

/// <summary>
/// Provides unit tests for the <see cref="XmlCommentsProvider"/> class to validate
/// its behavior when handling XML comments.
/// </summary>
public class XmlCommentsProviderTests
{
    /// <summary>
    /// Validates that the XmlCommentsProvider constructor throws a FileNotFoundException
    /// if the specified XML file does not exist.
    /// </summary>
    /// <remarks>
    /// This test ensures that the constructor for the XmlCommentsProvider class correctly
    /// handles cases where the XML documentation file is missing by throwing a meaningful
    /// exception. It also verifies the exception message to confirm it includes
    /// "XML comments file not found".
    /// </remarks>
    [Fact]
    public void Constructor_ThrowsIfFileDoesNotExist()
    {
        // Arrange
        var path = Path.GetTempFileName();
        File.Delete(path);

        // Act & Assert
        var ex = Assert.Throws<FileNotFoundException>(() => new XmlCommentsProvider(path));
        Assert.Contains("XML comments file not found", ex.Message);
    }

    /// Tests that the GetSummary method returns the expected summary for a given member.
    /// Validates the retrieval of summary documentation when a valid XML documentation file is provided.
    /// Ensures correct parsing and matching of a specified member's summary field.
    /// This test performs the following:
    /// - Creates a temporary XML documentation file with a specified structure.
    /// - Writes XML content to simulate a documentation file, including a summary for a test member.
    /// - Uses the XmlCommentsProvider to retrieve the summary for the specified member.
    /// - Asserts that the retrieved summary matches the expected value.
    /// - Cleans up by deleting the temporary file.
    [Fact]
    public void GetSummary_ReturnsExpectedSummary()
    {
        // Arrange
        var xml = """
            <doc>
              <members>
                <member name="M:TestNamespace.TestClass.TestMethod">
                  <summary>
                    Test summary.
                  </summary>
                </member>
              </members>
            </doc>
            """;

        var path = Path.GetTempFileName();
        File.WriteAllText(path, xml);

        var provider = new XmlCommentsProvider(path);

        // Act
        var summary = provider.GetSummary("M:TestNamespace.TestClass.TestMethod");

        // Assert
        Assert.Equal("Test summary.", summary);

        // Cleanup
        File.Delete(path);
    }

    /// Verifies that the GetSummary method returns null if the specified member is not found in the XML comments file.
    [Fact]
    public void GetSummary_ReturnsNull_IfMemberNotFound()
    {
        var xml = """
            <doc>
              <members></members>
            </doc>
            """;

        var path = Path.GetTempFileName();
        File.WriteAllText(path, xml);

        var provider = new XmlCommentsProvider(path);

        var summary = provider.GetSummary("M:Missing");

        Assert.Null(summary);
        File.Delete(path);
    }

    /// Validates that the XML comments provider correctly retrieves parameter descriptions for a given member.
    /// This method ensures that the `GetParamDescriptions` function in the `XmlCommentsProvider` class
    /// parses and returns the expected parameter descriptions based on the provided XML data.
    /// The test sets up a temporary XML documentation file with parameter elements, initializes
    /// an instance of `XmlCommentsProvider` using the file, and verifies that all parameter descriptions
    /// are correctly retrieved.
    /// Expected behavior:
    /// - Returns a dictionary containing the parameter names and their respective descriptions.
    /// - The dictionary keys should match the parameter names.
    /// - The dictionary values should match the descriptions provided in the XML.
    /// Verifies that:
    /// - The dictionary has an entry for each parameter defined in the XML.
    /// - The descriptions in the dictionary match those defined in the XML.
    /// This test covers the functionality of extracting parameter descriptions from XML documentation.
    [Fact]
    public void GetParamDescriptions_ReturnsDescriptions()
    {
        var xml = """
            <doc>
              <members>
                <member name="M:TestNamespace.TestClass.TestMethod">
                  <param name="a">First param</param>
                  <param name="b">Second param</param>
                </member>
              </members>
            </doc>
            """;

        var path = Path.GetTempFileName();
        File.WriteAllText(path, xml);

        var provider = new XmlCommentsProvider(path);

        var result = provider.GetParamDescriptions("M:TestNamespace.TestClass.TestMethod");

        Assert.Equal(2, result.Count);
        Assert.Equal("First param", result["a"]);
        Assert.Equal("Second param", result["b"]);

        File.Delete(path);
    }

    /// Validates that the GetParamDescriptions method returns an empty dictionary
    /// when the specified member has no parameter descriptions in the XML documentation.
    /// This test creates a temporary XML file containing documentation for a member
    /// with no parameter descriptions, invokes the GetParamDescriptions method with
    /// the member name, and asserts that the returned dictionary is empty.
    /// Ensures the behavior of the XmlCommentsProvider class when handling members
    /// without parameter documentation.
    /// Preconditions:
    /// - The XML documentation file exists but does not include parameter descriptions
    /// for the given member.
    /// Postconditions:
    /// - The returned dictionary is empty, indicating no parameter descriptions present.
    /// Exceptions:
    /// - Deletes the temporary file created for the test post-validation.
    [Fact]
    public void GetParamDescriptions_ReturnsEmpty_IfNoParams()
    {
        var xml = """
            <doc>
              <members>
                <member name="M:TestNamespace.TestClass.TestMethod">
                </member>
              </members>
            </doc>
            """;

        var path = Path.GetTempFileName();
        File.WriteAllText(path, xml);

        var provider = new XmlCommentsProvider(path);

        var result = provider.GetParamDescriptions("M:TestNamespace.TestClass.TestMethod");

        Assert.Empty(result);
        File.Delete(path);
    }
}
