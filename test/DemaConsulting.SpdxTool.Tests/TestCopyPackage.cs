using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestCopyPackage
{
    [TestMethod]
    public void CopyPackageMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "copy-package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'copy-package' command missing arguments"));
    }

    [TestMethod]
    public void CopyPackageMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "copy-package",
            "missing.spdx.json",
            "missing.spdx.json",
            "some-package");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("File not found: missing.spdx.json"));
    }

    [TestMethod]
    public void CopyPackageCommandLine()
    {
        const string toSpdxContents = "{\r\n" +
                                      "  \"files\": [],\r\n" +
                                      "  \"packages\": [\r\n" +
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
                                        "  \"packages\": [\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Package-2\",\r\n" +
                                        "      \"name\": \"Another Package\",\r\n" +
                                        "      \"versionInfo\": \"1.2.3\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
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

        try
        {
            // Write the SPDX files
            File.WriteAllText("to.spdx.json", toSpdxContents);
            File.WriteAllText("from.spdx.json", fromSpdxContents);

            // Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "copy-package",
                "from.spdx.json",
                "to.spdx.json",
                "SPDXRef-Package-2");

            // Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("to.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("to.spdx.json"));

            // Verify both packages present
            Assert.AreEqual(2, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-2", doc.Packages[1].Id);
        }
        finally
        {
            File.Delete("to.spdx.json");
            File.Delete("from.spdx.json");
        }
    }

    [TestMethod]
    public void CopyPackageWorkflow()
    {
        const string toSpdxContents = "{\r\n" +
                                      "  \"files\": [],\r\n" +
                                      "  \"packages\": [\r\n" +
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
                                        "  \"packages\": [\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Package-2\",\r\n" +
                                        "      \"name\": \"Another Package\",\r\n" +
                                        "      \"versionInfo\": \"1.2.3\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
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

    [TestMethod]
    public void CopyPackageRecursive()
    {
        const string toSpdxContents = "{\r\n" +
                                      "  \"files\": [],\r\n" +
                                      "  \"packages\": [\r\n" +
                                      "    {\r\n" +
                                      "      \"SPDXID\": \"SPDXRef-MainPackage\",\r\n" +
                                      "      \"name\": \"Main Package\",\r\n" +
                                      "      \"versionInfo\": \"1.0.0\",\r\n" +
                                      "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                      "      \"licenseConcluded\": \"MIT\"\r\n" +
                                      "    }\r\n" +
                                      "  ],\r\n" +
                                      "  \"relationships\": [" +
                                      "    {\r\n" +
                                      "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                      "      \"relatedSpdxElement\": \"SPDXRef-MainPackage\",\r\n" +
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
                                      "  \"documentDescribes\": [ \"SPDXRef-MainPackage\" ]\r\n" +
                                      "}";

        const string fromSpdxContents = "{\r\n" +
                                        "  \"files\": [],\r\n" +
                                        "  \"packages\": [\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Application\",\r\n" +
                                        "      \"name\": \"Test Application\",\r\n" +
                                        "      \"versionInfo\": \"1.2.3\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
                                        "    },\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Library\",\r\n" +
                                        "      \"name\": \"Test Library\",\r\n" +
                                        "      \"versionInfo\": \"2.3.4\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
                                        "    },\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Compiler\",\r\n" +
                                        "      \"name\": \"Compiler\",\r\n" +
                                        "      \"versionInfo\": \"3.4.5\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
                                        "    },\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Unrelated-Application\",\r\n" +
                                        "      \"name\": \"Unrelated Application\",\r\n" +
                                        "      \"versionInfo\": \"4.5.6\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\"\r\n" +
                                        "    }\r\n" +
                                        "  ],\r\n" +
                                        "  \"relationships\": [" +
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
                                        "      \"spdxElementId\": \"SPDXRef-Compiler\",\r\n" +
                                        "      \"relatedSpdxElement\": \"SPDXRef-Application\",\r\n" +
                                        "      \"relationshipType\": \"BUILD_TOOL_OF\"\r\n" +
                                        "    },\r\n" +
                                        "    {\r\n" +
                                        "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                        "      \"relatedSpdxElement\": \"SPDXRef-Unrelated-Application\",\r\n" +
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
                                        "  \"documentDescribes\": [ \"SPDXRef-Application\", \"SPDXRef-Unrelated-Application\" ]\r\n" +
                                        "}";

        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: copy-package\n" +
                                        "  inputs:\n" +
                                        "    from: from.spdx.json\n" +
                                        "    to: to.spdx.json\n" +
                                        "    package: SPDXRef-Application\n" +
                                        "    recursive: true\n" +
                                        "    relationships:\n" +
                                        "      - type: CONTAINED_BY\n" +
                                        "        element: SPDXRef-MainPackage\n";
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

            // Verify expected packages
            Assert.AreEqual(4, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-MainPackage", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Application", doc.Packages[1].Id);
            Assert.AreEqual("SPDXRef-Library", doc.Packages[2].Id);
            Assert.AreEqual("SPDXRef-Compiler", doc.Packages[3].Id);

            // Verify expected relationships
            Assert.AreEqual(4, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-DOCUMENT", doc.Relationships[0].Id);
            Assert.AreEqual(SpdxRelationshipType.Describes, doc.Relationships[0].RelationshipType);
            Assert.AreEqual("SPDXRef-MainPackage", doc.Relationships[0].RelatedSpdxElement);
            Assert.AreEqual("SPDXRef-Application", doc.Relationships[1].Id);
            Assert.AreEqual(SpdxRelationshipType.ContainedBy, doc.Relationships[1].RelationshipType);
            Assert.AreEqual("SPDXRef-MainPackage", doc.Relationships[1].RelatedSpdxElement);
            Assert.AreEqual("SPDXRef-Application", doc.Relationships[2].Id);
            Assert.AreEqual(SpdxRelationshipType.Contains, doc.Relationships[2].RelationshipType);
            Assert.AreEqual("SPDXRef-Library", doc.Relationships[2].RelatedSpdxElement);
            Assert.AreEqual("SPDXRef-Compiler", doc.Relationships[3].Id);
            Assert.AreEqual(SpdxRelationshipType.BuildToolOf, doc.Relationships[3].RelationshipType);
            Assert.AreEqual("SPDXRef-Application", doc.Relationships[3].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("to.spdx.json");
            File.Delete("from.spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    [TestMethod]
    public void CopyPackageFiles()
    {
        const string toSpdxContents = "{\r\n" +
                                      "  \"files\": [],\r\n" +
                                      "  \"packages\": [\r\n" +
                                      "    {\r\n" +
                                      "      \"SPDXID\": \"SPDXRef-MainPackage\",\r\n" +
                                      "      \"name\": \"Main Package\",\r\n" +
                                      "      \"versionInfo\": \"1.0.0\",\r\n" +
                                      "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxTool\",\r\n" +
                                      "      \"licenseConcluded\": \"MIT\"\r\n" +
                                      "    }\r\n" +
                                      "  ],\r\n" +
                                      "  \"relationships\": [" +
                                      "    {\r\n" +
                                      "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                      "      \"relatedSpdxElement\": \"SPDXRef-MainPackage\",\r\n" +
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
                                      "  \"documentDescribes\": [ \"SPDXRef-MainPackage\" ]\r\n" +
                                      "}";

        const string fromSpdxContents = "{\r\n" +
                                        "  \"files\": [\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\" : \"SPDXRef-File1\",\r\n" +
                                        "      \"checksums\" : [ {\r\n" +
                                        "        \"algorithm\" : \"SHA1\",\r\n" +
                                        "        \"checksumValue\" : \"d6a770ba38583ed4bb4525bd96e50461655d2758\"\r\n" +
                                        "      } ],\r\n" +
                                        "      \"fileName\" : \"file1\"\r\n" +
                                        "    },\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\" : \"SPDXRef-File2\",\r\n" +
                                        "      \"checksums\" : [ {\r\n" +
                                        "        \"algorithm\" : \"SHA1\",\r\n" +
                                        "        \"checksumValue\" : \"d6a770ba38583ed4bb4525bd96e50461655d2758\"\r\n" +
                                        "      } ],\r\n" +
                                        "      \"fileName\" : \"file2\"\r\n" +
                                        "    }\r\n" +
                                        "  ],\r\n" +
                                        "  \"packages\": [\r\n" +
                                        "    {\r\n" +
                                        "      \"SPDXID\": \"SPDXRef-Application\",\r\n" +
                                        "      \"name\": \"Test Application\",\r\n" +
                                        "      \"versionInfo\": \"1.2.3\",\r\n" +
                                        "      \"downloadLocation\": \"https://github.com/demaconsulting/SpdxModel\",\r\n" +
                                        "      \"licenseConcluded\": \"MIT\",\r\n" +
                                        "      \"filesAnalyzed\" : true,\r\n" +
                                        "      \"hasFiles\" : [ \"SPDXRef-File1\", \"SPDXRef-File2\" ]\r\n" +
                                        "    }\r\n" +
                                        "  ],\r\n" +
                                        "  \"relationships\": [" +
                                        "    {\r\n" +
                                        "      \"spdxElementId\": \"SPDXRef-DOCUMENT\",\r\n" +
                                        "      \"relatedSpdxElement\": \"SPDXRef-Application\",\r\n" +
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
                                        "  \"documentDescribes\": [ \"SPDXRef-Application\", \"SPDXRef-Unrelated-Application\" ]\r\n" +
                                        "}";

        // Workflow contents
        const string workflowContents = "steps:\n" +
                                        "- command: copy-package\n" +
                                        "  inputs:\n" +
                                        "    from: from.spdx.json\n" +
                                        "    to: to.spdx.json\n" +
                                        "    package: SPDXRef-Application\n" +
                                        "    files: true\n" +
                                        "    relationships:\n" +
                                        "      - type: CONTAINED_BY\n" +
                                        "        element: SPDXRef-MainPackage\n";
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

            // Verify expected packages
            Assert.AreEqual(2, doc.Packages.Length);
            Assert.AreEqual("SPDXRef-MainPackage", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Application", doc.Packages[1].Id);

            // Verify expected files
            Assert.AreEqual(2, doc.Files.Length);
            Assert.AreEqual("SPDXRef-File1", doc.Files[0].Id);
            Assert.AreEqual("SPDXRef-File2", doc.Files[1].Id);
        }
        finally
        {
            File.Delete("to.spdx.json");
            File.Delete("from.spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}