using DemaConsulting.SpdxModel.IO;
using DemaConsulting.SpdxModel;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestAddPackageCommand
{
    [TestMethod]
    public void AddPackageCommandLine()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "add-package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'add-package' command is only valid in a workflow"));
    }

    [TestMethod]
    public void AddPackageSimple()
    {
        // SPDX contents
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

        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: add-package\n" +
                                        "  inputs:\n" +
                                        "    package:\n" +
                                        "      id: SPDXRef-Package-2\n" +
                                        "      name: Test Package 2\n" +
                                        "      version: 2.0.0\n" +
                                        "      download: https://dotnet.microsoft.com/download\n" +
                                        "      purl: pkg:nuget/BogusPackage@2.0.0\n" +
                                        "    spdx: spdx.json\n" +
                                        "    relationship: BUILD_TOOL_OF\n" +
                                        "    element: SPDXRef-Package-1\n";

        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", spdxContents);
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
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Verify both packages present
            Assert.AreEqual(2, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-2", doc.Packages[1].Id);

            // Verify the relationship
            Assert.AreEqual(2, doc.Relationships.Length);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[1].Id);
            Assert.AreEqual(SpdxRelationshipType.BuildToolOf, doc.Relationships[1].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[1].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    [TestMethod]
    public void AddPackageFromQuery()
    {
        // SPDX contents
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

        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: query\n" +
                                        "  inputs:\n" +
                                        "    output: dotnet_version\n" +
                                        "    pattern: '(?<value>\\d+\\.\\d+\\.\\d+)'\n" +
                                        "    program: dotnet\n" +
                                        "    arguments:\n" +
                                        "    - --version\n" +
                                        "- command: add-package\n" +
                                        "  inputs:\n" +
                                        "    package:\n" +
                                        "      id: SPDXRef-Package-DotNet\n" +
                                        "      name: DotNet SDK\n" +
                                        "      version: ${{ dotnet_version }}\n" +
                                        "      download: https://dotnet.microsoft.com/download\n" +
                                        "      license: MIT\n" +
                                        "    spdx: spdx.json\n" +
                                        "    relationship: BUILD_TOOL_OF\n" +
                                        "    element: SPDXRef-Package-1\n";

        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", spdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Verify both packages present
            Assert.AreEqual(2, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-DotNet", doc.Packages[1].Id);

            // Verify the relationship
            Assert.AreEqual(2, doc.Relationships.Length);
            Assert.AreEqual("SPDXRef-Package-DotNet", doc.Relationships[1].Id);
            Assert.AreEqual(SpdxRelationshipType.BuildToolOf, doc.Relationships[1].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[1].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}