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

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'add-package' command.
/// </summary>
[Collection("CommandSequential")]
public class AddPackageTests
{
    /// <summary>
    ///     Test that add-package command on command line reports workflow-only error
    /// </summary>
    [Fact]
    public void AddPackage_Run_OnCommandLine_ReportsWorkflowOnlyError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "add-package");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'add-package' command is only valid in a workflow", output);
    }

    /// <summary>
    ///     Test that add-package command in workflow with relationship adds package and relationship
    /// </summary>
    [Fact]
    public void AddPackage_Run_InWorkflowWithRelationship_AddsPackageAndRelationship()
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
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify both packages present
            Assert.Equal(2, doc.Packages.Length);
            Assert.Equal("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.Equal("SPDXRef-Package-2", doc.Packages[1].Id);

            // Assert: Verify the relationship
            Assert.Equal(2, doc.Relationships.Length);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[1].Id);
            Assert.Equal(SpdxRelationshipType.BuildToolOf, doc.Relationships[1].RelationshipType);
            Assert.Equal("SPDXRef-Package-1", doc.Relationships[1].RelatedSpdxElement);

            // Assert: Verify the purl external reference was stored on the new package
            Assert.Single(doc.Packages[1].ExternalReferences);
            Assert.Equal(SpdxReferenceCategory.PackageManager, doc.Packages[1].ExternalReferences[0].Category);
            Assert.Equal("purl", doc.Packages[1].ExternalReferences[0].Type);
            Assert.Equal("pkg:nuget/BogusPackage@2.0.0", doc.Packages[1].ExternalReferences[0].Locator);
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
    [Fact]
    public void AddPackage_Run_InWorkflowNoRelationship_AddsPackageOnly()
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
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify package present
            Assert.Single(doc.Packages);
            Assert.Equal("SPDXRef-Package-1", doc.Packages[0].Id);

            // Assert: Verify no relationships
            Assert.Empty(doc.Relationships);
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
    [Fact]
    public void AddPackage_Run_InWorkflowWithQueryVersion_AddsPackage()
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
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify both packages present
            Assert.Equal(2, doc.Packages.Length);
            Assert.Equal("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.Equal("SPDXRef-Package-DotNet", doc.Packages[1].Id);

            // Assert: Verify version was expanded
            Assert.False(string.IsNullOrEmpty(doc.Packages[1].Version));

            // Assert: Verify the relationship
            Assert.Equal(2, doc.Relationships.Length);
            Assert.Equal("SPDXRef-Package-DotNet", doc.Relationships[1].Id);
            Assert.Equal(SpdxRelationshipType.BuildToolOf, doc.Relationships[1].RelationshipType);
            Assert.Equal("SPDXRef-Package-1", doc.Relationships[1].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with existing same-identity package enhances rather than duplicates
    /// </summary>
    [Fact]
    public void AddPackage_Run_InWorkflowWithExistingPackage_EnhancesPackage()
    {
        // SPDX contents - existing package with name "Test Package" version "1.0.0"
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package-Old",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
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

        // Workflow contents - add package with same name/version but different ID and with a supplier
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-New
                  name: Test Package
                  version: 1.0.0
                  download: https://dotnet.microsoft.com/download
                  supplier: "Organization: TestOrg"
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
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
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify only one package (enhanced, not duplicated)
            Assert.Single(doc.Packages);
            Assert.Equal("SPDXRef-Package-New", doc.Packages[0].Id);

            // Assert: Verify a field from the incoming package was merged into the existing package
            Assert.Equal("Organization: TestOrg", doc.Packages[0].Supplier);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with missing spdx input reports error
    /// </summary>
    [Fact]
    public void AddPackage_Run_InWorkflowMissingSpdxInput_ReportsError()
    {
        // Workflow contents - missing 'spdx' input
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                package:
                  id: SPDXRef-Package-1
                  name: Test Package
                  version: 1.0.0
                  download: https://dotnet.microsoft.com/download
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-package' missing 'spdx' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with missing package input reports error
    /// </summary>
    [Fact]
    public void AddPackage_Run_InWorkflowMissingPackageInput_ReportsError()
    {
        // Workflow contents - missing 'package' input
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-package' missing 'package' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with empty package ID reports error
    /// </summary>
    [Fact]
    public void AddPackage_Run_InWorkflowWithEmptyPackageId_ReportsError()
    {
        // Workflow contents - empty package ID
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: ''
                  name: Test Package
                  version: 1.0.0
                  download: https://dotnet.microsoft.com/download
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("Invalid package ID", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with SPDXRef-DOCUMENT package ID reports error
    /// </summary>
    [Fact]
    public void AddPackage_Run_InWorkflowWithDocumentPackageId_ReportsError()
    {
        // Workflow contents - reserved document package ID
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-DOCUMENT
                  name: Test Package
                  version: 1.0.0
                  download: https://dotnet.microsoft.com/download
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("Invalid package ID", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with missing package ID reports error
    /// </summary>
    [Fact]
    public void AddPackage_ParsePackage_MissingPackageId_ReportsError()
    {
        // Workflow contents - package entry missing the 'id' field entirely
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  name: Test Package
                  download: https://dotnet.microsoft.com/download
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-package' missing package 'id' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with missing package name reports error
    /// </summary>
    [Fact]
    public void AddPackage_ParsePackage_MissingPackageName_ReportsError()
    {
        // Workflow contents - package entry missing the 'name' field
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-1
                  download: https://dotnet.microsoft.com/download
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-package' missing package 'name' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package command in workflow with missing package download reports error
    /// </summary>
    [Fact]
    public void AddPackage_ParsePackage_MissingPackageDownload_ReportsError()
    {
        // Workflow contents - package entry missing the 'download' field
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-1
                  name: Test Package
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-package' missing package 'download' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package workflow step with a non-existent SPDX file reports an error
    /// </summary>
    [Fact]
    public void AddPackage_Run_InWorkflowWithMissingSpdxFile_ReportsError()
    {
        // Workflow contents - references a non-existent SPDX file
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: nonexistent.json
                package:
                  id: SPDXRef-Package-1
                  name: Test Package
                  download: https://example.com
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify non-zero exit code and error message
            Assert.Equal(1, exitCode);
            Assert.Contains("nonexistent.json", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that ParsePackage stores a cpe23 field as a Security/cpe23Type external reference
    /// </summary>
    [Fact]
    public void AddPackage_ParsePackage_WithCpe23_StoresCpe23ExternalReference()
    {
        // SPDX contents - empty document to receive the new package
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

        // Workflow contents - package with a cpe23 field
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-1
                  name: Test Package
                  download: https://example.com
                  cpe23: cpe:2.3:a:test:package:1.0:*:*:*:*:*:*:*
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
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
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify the cpe23 external reference was stored on the package
            Assert.Single(doc.Packages);
            Assert.Single(doc.Packages[0].ExternalReferences);
            Assert.Equal(SpdxReferenceCategory.Security, doc.Packages[0].ExternalReferences[0].Category);
            Assert.Equal("cpe23Type", doc.Packages[0].ExternalReferences[0].Type);
            Assert.Equal(
                "cpe:2.3:a:test:package:1.0:*:*:*:*:*:*:*",
                doc.Packages[0].ExternalReferences[0].Locator);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-package renames document references when enhancing an existing package
    ///     with a different ID, validating the F-01 fix (old ID captured before Enhance is called)
    /// </summary>
    [Fact]
    public void AddPackage_Run_InWorkflowWithExistingPackageAndRelationship_RenamesReferences()
    {
        // SPDX contents — package SPDXRef-OldId described by the document relationship
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-OldId",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://example.com",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-OldId",
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
              "documentDescribes": [ "SPDXRef-OldId" ]
            }
            """;

        // Workflow contents — same-identity package (same name + version) but different ID
        const string workflowContents =
            """
            steps:
            - command: add-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-NewId
                  name: Test Package
                  version: 1.0.0
                  download: https://example.com
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
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
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify the package was enhanced (only one package) with the new ID
            Assert.Single(doc.Packages);
            Assert.Equal("SPDXRef-NewId", doc.Packages[0].Id);

            // Assert: Verify the relationship was renamed to reference the new ID (not the stale old ID)
            Assert.Single(doc.Relationships);
            Assert.Equal("SPDXRef-NewId", doc.Relationships[0].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}
