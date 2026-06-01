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

using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'update-package' command.
/// </summary>
[Collection("CommandSequential")]
public class UpdatePackageTests
{
    /// <summary>
    ///     Test that update-package command on command line reports workflow-only error
    /// </summary>
    [Fact]
    public void UpdatePackage_Run_OnCommandLine_ReportsWorkflowOnlyError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "update-package");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'update-package' command is only valid in a workflow", output);
    }

    /// <summary>
    ///     Test that update-package command in workflow updates the package
    /// </summary>
    [Fact]
    public void UpdatePackage_Run_InWorkflow_UpdatesPackage()
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
            - command: update-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-1
                  name: New package name
                  download: https://new.package.download
                  version: 2.0.0
                  filename: new.zip
                  supplier: New Supplier
                  originator: New Originator
                  homepage: https://new.package.org
                  copyright: Copyright New Package Maker
                  summary: New Package
                  description: A new package description
                  license: MIT v2
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

            // Assert: Verify the package was updated correctly
            Assert.Single(doc.Packages);
            Assert.Equal("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.Equal("New package name", doc.Packages[0].Name);
            Assert.Equal("https://new.package.download", doc.Packages[0].DownloadLocation);
            Assert.Equal("2.0.0", doc.Packages[0].Version);
            Assert.Equal("new.zip", doc.Packages[0].FileName);
            Assert.Equal("New Supplier", doc.Packages[0].Supplier);
            Assert.Equal("New Originator", doc.Packages[0].Originator);
            Assert.Equal("https://new.package.org", doc.Packages[0].HomePage);
            Assert.Equal("Copyright New Package Maker", doc.Packages[0].CopyrightText);
            Assert.Equal("New Package", doc.Packages[0].Summary);
            Assert.Equal("A new package description", doc.Packages[0].Description);
            Assert.Equal("MIT v2", doc.Packages[0].ConcludedLicense);
            Assert.Equal("MIT v2", doc.Packages[0].DeclaredLicense);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that update-package command with missing package id reports an error
    /// </summary>
    [Fact]
    public void UpdatePackage_Run_MissingPackageIdInput_ReportsError()
    {
        // Workflow contents with 'package' present but missing 'id' sub-key
        const string workflowContents =
            """
            steps:
            - command: update-package
              inputs:
                spdx: spdx.json
                package:
                  name: Updated Name
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
            Assert.Contains("'update-package' missing 'package.id' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that update-package command with missing spdx input reports an error
    /// </summary>
    [Fact]
    public void UpdatePackage_Run_MissingSpdxInput_ReportsError()
    {
        // Workflow contents with missing 'spdx' input
        const string workflowContents =
            """
            steps:
            - command: update-package
              inputs:
                package:
                  id: SPDXRef-Package-1
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
            Assert.Contains("'update-package' missing 'spdx' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that update-package command with missing package input reports an error
    /// </summary>
    [Fact]
    public void UpdatePackage_Run_MissingPackageInput_ReportsError()
    {
        // Workflow contents with missing 'package' input
        const string workflowContents =
            """
            steps:
            - command: update-package
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
            Assert.Contains("'update-package' missing 'package' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that update-package command with package not found reports an error
    /// </summary>
    [Fact]
    public void UpdatePackage_Run_PackageNotFound_ReportsError()
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

        // Workflow contents referencing a non-existent package
        const string workflowContents =
            """
            steps:
            - command: update-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-NotExist
                  name: Updated Name
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("spdx.json", spdxContents);
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
            Assert.Contains("SPDXRef-NotExist", output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that update-package command with an unrecognized field reports an error
    /// </summary>
    [Fact]
    public void UpdatePackage_Run_UnrecognizedField_ReportsError()
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

        // Workflow contents with an unrecognized package field
        const string workflowContents =
            """
            steps:
            - command: update-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-1
                  unknown-field: some-value
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("spdx.json", spdxContents);
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
            Assert.Contains("Invalid package update key", output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that update-package command with a partial update preserves unspecified fields
    /// </summary>
    [Fact]
    public void UpdatePackage_Run_PartialUpdate_PreservesUnspecifiedFields()
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

        // Workflow contents specifying only the name field
        const string workflowContents =
            """
            steps:
            - command: update-package
              inputs:
                spdx: spdx.json
                package:
                  id: SPDXRef-Package-1
                  name: Updated Name
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

            // Assert: Verify name was updated
            Assert.Equal("Updated Name", doc.Packages[0].Name);

            // Assert: Verify unspecified fields were preserved
            Assert.Equal("1.0.0", doc.Packages[0].Version);
            Assert.Equal("https://github.com/demaconsulting/SpdxTool", doc.Packages[0].DownloadLocation);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}
