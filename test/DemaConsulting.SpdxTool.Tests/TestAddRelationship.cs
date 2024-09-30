// Copyright (c) 2024 DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
/// Tests for the 'add-relationship' command
/// </summary>
[TestClass]
public class TestAddRelationship
{
    /// <summary>
    /// SPDX file for finding packages
    /// </summary>
    private const string SpdxContents = 
        """
        {
          "files": [],
          "packages": [    {
              "SPDXID": "SPDXRef-Package-1",
              "name": "Test Package",
              "versionInfo": "1.0.0",
              "packageFileName": "package1.zip",
              "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
              "licenseConcluded": "MIT"
            },
            {
              "SPDXID": "SPDXRef-Package-2",
              "name": "Another Test Package",
              "versionInfo": "2.0.0",
              "packageFileName": "package2.tar",
              "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
              "licenseConcluded": "MIT"
            }
          ],
          "spdxVersion": "SPDX-2.2",
          "dataLicense": "CC0-1.0",
          "SPDXID": "SPDXRef-DOCUMENT",
          "name": "Test Document",
          "documentNamespace": "https://sbom.spdx.org",
          "creationInfo": {
            "created": "2021-10-01T00:00:00Z",
            "creators": [ "Person: Malcolm Nixon" ]
          },
          "documentDescribes": [ "SPDXRef-Package-1" ]
        }
        """;

    /// <summary>
    /// Test the add-relationship command with missing arguments
    /// </summary>
    [TestMethod]
    public void AddRelationship_MissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "add-relationship");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "'add-relationship' command missing arguments");
    }

    /// <summary>
    /// Test the add-relationship command with missing SPDX file
    /// </summary>
    [TestMethod]
    public void AddRelationship_MissingFile()
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
        StringAssert.Contains(output, "File not found: missing.spdx.json");
    }

    /// <summary>
    /// Test the add-relationship command from the command-line
    /// </summary>
    [TestMethod]
    public void AddRelationship_CommandLine()
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

    /// <summary>
    /// Test the add-relationship command from a workflow
    /// </summary>
    [TestMethod]
    public void AddRelationship_Workflow()
    {
        // Workflow contents
        const string workflowContents = 
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                relationships:
                - type: CONTAINS
                  element: SPDXRef-Package-2
                  comment: Package 1 contains Package 2
            """;

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

    /// <summary>
    /// Test the add-relationship command from a workflow replacing an existing relationship
    /// </summary>
    [TestMethod]
    public void AddRelationship_Replace()
    {
        // Workflow1 contents
        const string workflow1Contents = 
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                relationships:
                - type: CONTAINS
                  element: SPDXRef-Package-2
                  comment: Package 1 contains Package 2
                - type: DESCRIBES
                  element: SPDXRef-Package-2
                  comment: Package 1 describes Package 2
            """;

        // Workflow2 contents
        const string workflow2Contents = 
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                replace: true
                relationships:
                - type: BUILD_TOOL_OF
                  element: SPDXRef-Package-2
                  comment: Package 1 builds Package 2
            """;

        try
        {
            // Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow1.yaml", workflow1Contents);
            File.WriteAllText("workflow2.yaml", workflow2Contents);

            // Run the first workflow
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml");

            // Verify error reported
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            Assert.AreEqual(2, doc.Relationships.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.AreEqual(SpdxRelationshipType.Contains, doc.Relationships[0].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.AreEqual("Package 1 contains Package 2", doc.Relationships[0].Comment);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[1].Id);
            Assert.AreEqual(SpdxRelationshipType.Describes, doc.Relationships[1].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[1].RelatedSpdxElement);
            Assert.AreEqual("Package 1 describes Package 2", doc.Relationships[1].Comment);

            // Run the second workflow
            exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow2.yaml");

            // Verify error reported
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            Assert.AreEqual(1, doc.Relationships.Length);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.AreEqual(SpdxRelationshipType.BuildToolOf, doc.Relationships[0].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.AreEqual("Package 1 builds Package 2", doc.Relationships[0].Comment);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }
}