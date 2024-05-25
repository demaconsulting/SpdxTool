using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestCopyPackage
{
    [TestMethod]
    public void CopyPackageCommandLine()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "copy-package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'copy-package' command is only valid in a workflow"));
    }

    [TestMethod]
    public void CopyPackageWorkflow()
    {
        const string toSpdxContents = "{\r\n" +
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

        const string fromSpdxContents = "{\r\n" +
                                        "  \"files\": [],\r\n" +
                                        "  \"packages\": [" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Package-2\",\r\n" +
                                        "      \"name\": \"Test Package\",\r\n" +
                                        "      \"versionInfo\": \"1.0.0\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
                                        "    }\r\n" +
                                        "  ],\r\n" +
                                        "  \"relationships\": [" +
                                        "    {\r\n" +
                                        "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                        "      \"relatedSpdxElement\": \"SPDXRef-Package-2\",\r\n" +
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
                                        "  \"documentDescribes\": [ \"SPDXRef-Package-2\" ]\r\n" +
                                        "}";

        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: copy-package\n" +
                                        "  inputs:\n" +
                                        "    from: from.spdx.json\n" +
                                        "    to: to.spdx.json\n" +
                                        "    package: SPDXRef-Package-2\n" +
                                        "    relationships:\n" +
                                        "      - type: CONTAINED_BY\n" +
                                        "        element: SPDXRef-Package-1\n";

        try
        {
            // Write the SPDX files
            File.WriteAllText("to.spdx.json", toSpdxContents);
            File.WriteAllText("from.spdx.json", fromSpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("to.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("to.spdx.json"));

            // Verify both packages present
            Assert.AreEqual(2, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-2", doc.Packages[1].Id);

            // Verify the relationship
            Assert.AreEqual(2, doc.Relationships.Length);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[1].Id);
            Assert.AreEqual(SpdxRelationshipType.ContainedBy, doc.Relationships[1].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[1].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("to.spdx.json");
            File.Delete("from.spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}