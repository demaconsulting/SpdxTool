namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestToMarkdown
{
    [TestMethod]
    public void ToMarkdownMissingArguments()
    {
        // Run the tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "to-markdown");

        // Verify the conversion failed
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'to-markdown' command missing arguments"));
    }

    [TestMethod]
    public void ToMarkdownMissingSpdx()
    {
        // Run the tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "to-markdown",
            "missing.spdx.json",
            "output.md");

        // Verify the conversion failed
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("File not found: missing.spdx.json"));
    }

    [TestMethod]
    public void ToMarkdown()
    {
        const string spdxContents = "{\r\n" +
                                    "  \"files\": [],\r\n" +
                                    "  \"packages\": [" +
                                    "    {\r\n" +
                                    "      \"SPDXID\": \"SPDXRef-Package-1\",\r\n" +
                                    "      \"name\": \"Test Package\",\r\n" +
                                    "      \"versionInfo\": \"1.0.0\",\r\n" +
                                    "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                    "      \"licenseConcluded\": \"MIT\"\r\n" +
                                    "    },\r\n" +
                                    "    {\r\n" +
                                    "      \"SPDXID\": \"SPDXRef-Package-2\",\r\n" +
                                    "      \"name\": \"Test Tool\",\r\n" +
                                    "      \"versionInfo\": \"1.0.0\",\r\n" +
                                    "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                    "      \"licenseConcluded\": \"MIT\"\r\n" +
                                    "    }\r\n" +
                                    "  ],\r\n" +
                                    "  \"relationships\": [\r\n" +
                                    "    {\r\n" +
                                    "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                    "      \"relatedSpdxElement\": \"SPDXRef-Package-1\",\r\n" +
                                    "      \"relationshipType\": \"DESCRIBES\"\r\n" +
                                    "    },\r\n" +
                                    "    {\r\n" +
                                    "      \"spdxElementId\": \"SPDXRef-Package-2\",\r\n" +
                                    "      \"relatedSpdxElement\": \"SPDXRef-Package-1\",\r\n" +
                                    "      \"relationshipType\": \"BUILD_TOOL_OF\"\r\n" +
                                    "    }\r\n" +
                                    "  ],\r\n" +
                                    "  \"spdxVersion\": \"SPDX-2.2\",\r\n" +
                                    "  \"dataLicense\": \"CC0-1.0\",\r\n" +
                                    "  \"SPDXID\": \"SPDXRef-DOCUMENT\",\r\n" +
                                    "  \"name\": \"Test Document\",\r\n" +
                                    "  \"documentNamespace\": \"https://sbom.spdx.org\",\r\n" +
                                    "  \"creationInfo\": {\r\n" +
                                    "    \"created\": \"2021-10-01T00:00:00Z\",\r\n" +
                                    "    \"creators\": [ \"Person: Malcolm Nixon\" ]\r\n" +
                                    "  },\r\n" +
                                    "  \"documentDescribes\": [ \"SPDXRef-Package-1\" ]\r\n" +
                                    "}";

        try
        {
            // Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Run the tool
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "to-markdown",
                "test.spdx.json",
                "test.md");

            // Verify the conversion succeeded
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(File.Exists("test.md"));

            // Read the Markdown text
            var markdown = File.ReadAllText("test.md");

            // Verify the contents
            Assert.IsTrue(markdown.Contains("## SPDX Document"));
            Assert.IsTrue(markdown.Contains("| File Name | test.spdx.json |"));
            Assert.IsTrue(markdown.Contains("| Name | Test Document |"));
            Assert.IsTrue(markdown.Contains("| File Name | test.spdx.json |"));

            // Find the packages section
            var packagesPosition = markdown.IndexOf("# Package Summary", StringComparison.Ordinal);
            Assert.IsTrue(packagesPosition >= 0);

            // Find the tools section
            var toolsPosition = markdown.IndexOf("# Tool Summary", StringComparison.Ordinal);
            Assert.IsTrue(toolsPosition >= 0);

            // Verify "Test Package" is a package
            var testPackagePosition = markdown.IndexOf("| Test Package | 1.0.0 | MIT |", StringComparison.Ordinal);
            Assert.IsTrue(testPackagePosition > packagesPosition && testPackagePosition < toolsPosition);

            // Verify "Test Tool" is a tool
            var testToolPosition = markdown.IndexOf("| Test Tool | 1.0.0 | MIT |", StringComparison.Ordinal);
            Assert.IsTrue(testToolPosition > toolsPosition);
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("test.md");
        }
    }
}