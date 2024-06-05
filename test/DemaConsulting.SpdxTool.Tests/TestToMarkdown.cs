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
                                    "      \"SPDXID\": \"SPDXRef-Application\",\r\n" +
                                    "      \"name\": \"Test Application\",\r\n" +
                                    "      \"versionInfo\": \"1.2.3\",\r\n" +
                                    "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                    "      \"licenseConcluded\": \"MIT\"\r\n" +
                                    "    },\r\n" +
                                    "    {\r\n" +
                                    "      \"SPDXID\": \"SPDXRef-Library\",\r\n" +
                                    "      \"name\": \"Test Library\",\r\n" +
                                    "      \"versionInfo\": \"2.3.4\",\r\n" +
                                    "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                    "      \"licenseConcluded\": \"MIT\"\r\n" +
                                    "    },\r\n" +
                                    "    {\r\n" +
                                    "      \"SPDXID\": \"SPDXRef-Tool\",\r\n" +
                                    "      \"name\": \"Test Tool\",\r\n" +
                                    "      \"versionInfo\": \"3.4.5\",\r\n" +
                                    "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                    "      \"licenseConcluded\": \"MIT\"\r\n" +
                                    "    }\r\n" +
                                    "  ],\r\n" +
                                    "  \"relationships\": [\r\n" +
                                    "    {\r\n" +
                                    "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                    "      \"relatedSpdxElement\": \"SPDXRef-Application\",\r\n" +
                                    "      \"relationshipType\": \"DESCRIBES\"\r\n" +
                                    "    },\r\n" +
                                    "    {\r\n" +
                                    "      \"spdxElementId\": \"SPDXRef-Application\",\r\n" +
                                    "      \"relatedSpdxElement\": \"SPDXRef-Library\",\r\n" +
                                    "      \"relationshipType\": \"CONTAINS\"\r\n" +
                                    "    },\r\n" +
                                    "    {\r\n" +
                                    "      \"spdxElementId\": \"SPDXRef-Tool\",\r\n" +
                                    "      \"relatedSpdxElement\": \"SPDXRef-Application\",\r\n" +
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

            // Find the root packages section
            var rootPackagesIndex = markdown.IndexOf("# Root Packages", StringComparison.Ordinal);
            Assert.IsTrue(rootPackagesIndex >= 0);

            // Find the packages section
            var packagesIndex = markdown.IndexOf("# Packages", StringComparison.Ordinal);
            Assert.IsTrue(packagesIndex >= 0);

            // Find the tools section
            var toolsIndex = markdown.IndexOf("# Tools", StringComparison.Ordinal);
            Assert.IsTrue(toolsIndex >= 0);

            // Verify "Test Application" is a root package
            var testPackageIndex = markdown.IndexOf("| Test Application | 1.2.3 | MIT |", StringComparison.Ordinal);
            Assert.IsTrue(testPackageIndex > rootPackagesIndex && testPackageIndex < packagesIndex);

            // Verify "Test Library" is a package
            var testLibraryIndex = markdown.IndexOf("| Test Library | 2.3.4 | MIT |", StringComparison.Ordinal);
            Assert.IsTrue(testLibraryIndex > packagesIndex && testLibraryIndex < toolsIndex);

            // Verify "Test Tool" is a tool
            var testToolPosition = markdown.IndexOf("| Test Tool | 3.4.5 | MIT |", StringComparison.Ordinal);
            Assert.IsTrue(testToolPosition > toolsIndex);
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("test.md");
        }
    }
}