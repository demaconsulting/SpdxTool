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
///     Tests for the 'add-package' command.
/// </summary>
[TestClass]
public class AddPackageTests
{
    /// <summary>
    ///     Test that add-package command on command line reports workflow-only error
    /// </summary>
    [TestMethod]
    public void AddPackage_OnCommandLine_ReportsWorkflowOnlyError()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "add-package");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'add-package' command is only valid in a workflow", output);
    }

    /// <summary>
    ///     Test that add-package command in workflow with relationship adds package and relationship
    /// </summary>
    [TestMethod]
    public void AddPackage_InWorkflowWithRelationship_AddsPackageAndRelationship()
    {
        // SPDX contents
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package-1",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-1",
                  "relationshipType": "DESCRIBES"
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

        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-2
                  name: Test Package 2
                  version: 2.0.0
                  download: https://dotnet.microsoft.com/download
                  purl: pkg:nuget/BogusPackage@2.0.0
                relationships:
                  - type: BUILD_TOOL_OF
                    element: SPDXRef-Package-1
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("spdx.json", spdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify both packages present
            Assert.HasCount(2, doc.Packages);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-2", doc.Packages[1].Id);

            // Assert: Verify the relationship
            Assert.HasCount(2, doc.Relationships);
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

    /// <summary>
    ///     Test that add-package command in workflow with no relationship adds package only
    /// </summary>
    [TestMethod]
    public void AddPackage_InWorkflowNoRelationship_AddsPackageOnly()
    {
        // SPDX contents
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [],
              "relationships": [],
              "spdxVersion": "SPDX-2.2",
              "dataLicense": "CC0-1.0",
              "SPDXID": "SPDXRef-DOCUMENT",
              "name": "Test Document",
              "documentNamespace": "https://sbom.spdx.org",
              "creationInfo": {
                "created": "2021-10-01T00:00:00Z",
                "creators": [ "Person: Malcolm Nixon" ]
              },
              "documentDescribes": []
            }
            """;

        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-1
                  name: Test Package 1
                  version: 1.0.0
                  download: https://dotnet.microsoft.com/download
                  purl: pkg:nuget/BogusPackage@1.0.0
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("spdx.json", spdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify package present
            Assert.HasCount(1, doc.Packages);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);

            // Assert: Verify no relationships
            Assert.IsEmpty(doc.Relationships);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with query version adds package
    /// </summary>
    [TestMethod]
    public void AddPackage_InWorkflowWithQueryVersion_AddsPackage()
    {
        // SPDX contents
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package-1",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-1",
                  "relationshipType": "DESCRIBES"
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

        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: query
              inputs:
                output: dotnet_version
                pattern: '(?<value>\d+\.\d+\.\d+)'
                program: dotnet
                arguments:
                - '--version'

            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-DotNet
                  name: DotNet SDK
                  version: ${{ dotnet_version }}
                  download: https://dotnet.microsoft.com/download
                  license: MIT
                relationships:
                  - type: BUILD_TOOL_OF
                    element: SPDXRef-Package-1
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("spdx.json", spdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify both packages present
            Assert.HasCount(2, doc.Packages);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-DotNet", doc.Packages[1].Id);

            // Assert: Verify the relationship
            Assert.HasCount(2, doc.Relationships);
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
