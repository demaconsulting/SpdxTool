namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestToMarkdown
{
    [TestMethod]
    public void ToMarkdown()
    {
        var spdxContents =
            "{\r\n" +
            "  \"files\": [],\r\n" +
            "  \"packages\": [" +
            "    {\r\n" +
            "      \"SPDXID\": \"SPDXRef-Package\",\r\n" +
            "      \"name\": \"Test Package\",\r\n" +
            "      \"versionInfo\": \"1.0.0\",\r\n" +
            "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
            "      \"licenseConcluded\": \"MIT\"\r\n" +
            "    }\r\n" +
            "  ],\r\n" +
            "  \"relationships\": [],\r\n" +
            "  \"spdxVersion\": \"SPDX-2.2\",\r\n" +
            "  \"dataLicense\": \"CC0-1.0\",\r\n" +
            "  \"SPDXID\": \"SPDXRef-DOCUMENT\",\r\n" +
            "  \"name\": \"Test Document\",\r\n" +
            "  \"documentNamespace\": \"https://sbom.spdx.org\",\r\n" +
            "  \"creationInfo\": {\r\n" +
            "    \"created\": \"2021-10-01T00:00:00Z\",\r\n" +
            "    \"creators\": [ \"Person: Malcolm Nixon\" ]\r\n" +
            "  },\r\n" +
            "  \"documentDescribes\": [ \"SPDXRef-Package\" ]\r\n" +
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
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("test.md");
        }
    }
}