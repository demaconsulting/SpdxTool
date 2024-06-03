using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestAddRelationship
{
    /// <summary>
    /// SPDX file for finding packages
    /// </summary>
    private const string SpdxContents = "{\r\n" +
                                        "  \"files\": [],\r\n" +
                                        "  \"packages\": [" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Package-1\",\r\n" +
                                        "      \"name\": \"Test Package\",\r\n" +
                                        "      \"versionInfo\": \"1.0.0\",\r\n" +
                                        "      \"packageFileName\": \"package1.zip\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
                                        "    },\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Package-2\",\r\n" +
                                        "      \"name\": \"Another Test Package\",\r\n" +
                                        "      \"versionInfo\": \"2.0.0\",\r\n" +
                                        "      \"packageFileName\": \"package2.tar\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
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

    [TestMethod]
    public void AddRelationshipMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "add-relationship");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'add-relationship' command missing arguments"));
    }

    [TestMethod]
    public void AddRelationshipMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "add-relationship",
            "missing.spdx.json",
            "from-package",
            "CONTAINS",
            "to-package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("File not found: missing.spdx.json"));
    }

    [TestMethod]
    public void AddRelationshipCommandLine()
    {
        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);

            // Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "add-relationship",
                "spdx.json",
                "SPDXRef-Package-1",
                "CONTAINS",
                "SPDXRef-Package-2",
                "Package 1 contains Package 2");

            // Verify error reported
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            Assert.AreEqual(1, doc.Relationships.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.AreEqual(SpdxRelationshipType.Contains, doc.Relationships[0].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.AreEqual("Package 1 contains Package 2", doc.Relationships[0].Comment);
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    [TestMethod]
    public void AddRelationshipWorkflow()
    {
        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: add-relationship\n" +
                                        "  inputs:\n" +
                                        "    spdx: spdx.json\n" +
                                        "    id: SPDXRef-Package-1\n" +
                                        "    relationships:\n"+
                                        "    - type: CONTAINS\n" +
                                        "      element: SPDXRef-Package-2\n" +
                                        "      comment: Package 1 contains Package 2";
        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify error reported
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            Assert.AreEqual(1, doc.Relationships.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.AreEqual(SpdxRelationshipType.Contains, doc.Relationships[0].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.AreEqual("Package 1 contains Package 2", doc.Relationships[0].Comment);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}