using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestRenameIdCommand
{
    [TestMethod]
    public void RenameIdMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "rename-id");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'rename-id' command missing arguments"));
    }

    [TestMethod]
    public void RenameIdMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "rename-id",
            "missing.spdx.json",
            "SPDXRef-Package-1",
            "SPDXRef-Package-2");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("File not found: missing.spdx.json"));
    }

    [TestMethod]
    public void RenameId()
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
                                    "    }\r\n" +
                                    "  ],\r\n" +
                                    "  \"relationships\": [" +
                                    "    {\r\n" +
                                    "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                    "      \"relatedSpdxElement\": \"SPDXRef-Package-1\",\r\n" +
                                    "      \"relationshipType\": \"DESCRIBES\"\r\n" +
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
                "rename-id",
                "test.spdx.json",
                "SPDXRef-Package-1",
                "SPDXRef-Package-2");

            // Verify the conversion succeeded
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("test.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("test.spdx.json"));

            // Verify the SPDX ID was updated
            Assert.AreEqual("SPDXRef-Package-2", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.AreEqual("SPDXRef-Package-2", doc.Describes[0]);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }
}