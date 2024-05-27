using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestUpdatePackage
{
    [TestMethod]
    public void UpdatePackageCommandLine()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "update-package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'update-package' command is only valid in a workflow"));
    }

    [TestMethod]
    public void UpdatePackageWorkflow()
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
                                        "- command: update-package\n" +
                                        "  inputs:\n" +
                                        "    spdx: spdx.json\n" +
                                        "    package:\n" +
                                        "      id: SPDXRef-Package-1\n" +
                                        "      name: New package name\n" +
                                        "      download: https://new.package.download\n" +
                                        "      version: 2.0.0\n" +
                                        "      filename: new.zip\n" +
                                        "      supplier: New Supplier\n" +
                                        "      originator: New Originator\n" +
                                        "      homepage: https://new.package.org\n" +
                                        "      copyright: Copyright New Package Maker\n" +
                                        "      summary: New Package\n" +
                                        "      description: A new package description\n" +
                                        "      license: MIT v2\n";

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
            Assert.AreEqual(1, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("New package name", doc.Packages[0].Name);
            Assert.AreEqual("https://new.package.download", doc.Packages[0].DownloadLocation);
            Assert.AreEqual("2.0.0", doc.Packages[0].Version);
            Assert.AreEqual("new.zip", doc.Packages[0].FileName);
            Assert.AreEqual("New Supplier", doc.Packages[0].Supplier);
            Assert.AreEqual("New Originator", doc.Packages[0].Originator);
            Assert.AreEqual("https://new.package.org", doc.Packages[0].HomePage);
            Assert.AreEqual("Copyright New Package Maker", doc.Packages[0].CopyrightText);
            Assert.AreEqual("New Package", doc.Packages[0].Summary);
            Assert.AreEqual("A new package description", doc.Packages[0].Description);
            Assert.AreEqual("MIT v2", doc.Packages[0].ConcludedLicense);
            Assert.AreEqual("MIT v2", doc.Packages[0].DeclaredLicense);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}