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

using DemaConsulting.SpdxTool.Commands;

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'find-package' command
/// </summary>
[Collection("CommandSequential")]
public class FindPackageTests
{
    /// <summary>
    ///     SPDX file for finding packages
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

    /// <summary>
    ///     Test that find-package command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void FindPackage_Run_MissingArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "find-package");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'find-package' command missing arguments", output);
    }

    /// <summary>
    ///     Test that find-package command with missing file reports an error
    /// </summary>
    [Fact]
    public void FindPackage_Run_MissingFile_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "find-package",
            "missing.spdx.json",
            "name=anything");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test that find-package command by name on command line finds a package
    /// </summary>
    [Fact]
    public void FindPackage_Run_ByNameOnCommandLine_FindsPackage()
    {
        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "find-package",
                "spdx.json",
                "name=Another Test Package");

            // Assert: Verify package ID
            Assert.Equal(0, exitCode);
            Assert.Contains("SPDXRef-Package-2", output);
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    /// <summary>
    ///     Test that find-package command by name finds the package
    /// </summary>
    [Fact]
    public void FindPackage_Run_ByName_FindsPackage()
    {
        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                output: packageId
                spdx: spdx.json
                name: Test Package
            - command: print
              inputs:
                text:
                - Found package ${{ packageId }}
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify package ID
            Assert.Equal(0, exitCode);
            Assert.Contains("Found package SPDXRef-Package-1", output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that find-package command by version finds the package
    /// </summary>
    [Fact]
    public void FindPackage_Run_ByVersion_FindsPackage()
    {
        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                output: packageId
                spdx: spdx.json
                version: 2.0.0
            - command: print
              inputs:
                text:
                - Found package ${{ packageId }}
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify package ID
            Assert.Equal(0, exitCode);
            Assert.Contains("Found package SPDXRef-Package-2", output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that find-package command by file name finds the package
    /// </summary>
    [Fact]
    public void FindPackage_Run_ByFileName_FindsPackage()
    {
        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                output: packageId
                spdx: spdx.json
                filename: package1.zip
            - command: print
              inputs:
                text:
                - Found package ${{ packageId }}
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify package ID
            Assert.Equal(0, exitCode);
            Assert.Contains("Found package SPDXRef-Package-1", output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that find-package command by download URL finds the package
    /// </summary>
    [Fact]
    public void FindPackage_Run_ByDownloadUrl_FindsPackage()
    {
        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                output: packageId
                spdx: spdx.json
                download: https://github.com/demaconsulting/SpdxModel
            - command: print
              inputs:
                text:
                - Found package ${{ packageId }}
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify package ID
            Assert.Equal(0, exitCode);
            Assert.Contains("Found package SPDXRef-Package-2", output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that find-package command by ID finds the package
    /// </summary>
    [Fact]
    public void FindPackage_Run_ById_FindsPackage()
    {
        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("spdx.json", SpdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "find-package",
                "spdx.json",
                "id=SPDXRef-Package-2");

            // Assert: Verify package ID
            Assert.Equal(0, exitCode);
            Assert.Contains("SPDXRef-Package-2", output);
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    /// <summary>
    ///     Test that find-package command with invalid criteria format reports an error
    /// </summary>
    [Fact]
    public void FindPackage_Run_InvalidCriteria_ReportsError()
    {
        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("spdx.json", SpdxContents);

            // Act: Run the command with an invalid criterion that has no '=' separator
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "find-package",
                "spdx.json",
                "invalid-criterion");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("Invalid criteria", output);
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    /// <summary>
    ///     Test that find-package workflow step with missing 'output' input reports error
    /// </summary>
    [Fact]
    public void FindPackage_Run_MissingOutputInput_ReportsError()
    {
        // Workflow contents - missing 'output' input
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                spdx: spdx.json
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
            Assert.Contains("'find-package' command missing 'output' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that find-package workflow step with missing 'spdx' input reports error
    /// </summary>
    [Fact]
    public void FindPackage_Run_MissingSpdxInput_ReportsError()
    {
        // Workflow contents - missing 'spdx' input
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                output: packageId
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
            Assert.Contains("'find-package' command missing 'spdx' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that find-package command reports an error when no package matches the criteria
    /// </summary>
    [Fact]
    public void FindPackage_Run_NoPackageFound_ReportsError()
    {
        // Workflow contents - criteria that match no package
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                output: packageId
                spdx: spdx.json
                name: Nonexistent Package
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported, including the searched criteria
            Assert.Equal(1, exitCode);
            Assert.Contains("Package not found", output);
            Assert.Contains("name=Nonexistent Package", output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that find-package command reports an error when multiple packages match the criteria
    /// </summary>
    [Fact]
    public void FindPackage_Run_MultiplePackagesFound_ReportsError()
    {
        // Workflow contents - criteria that match more than one package (download URL wildcard matches both)
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                output: packageId
                spdx: spdx.json
                download: https://github.com/demaconsulting/*
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported, including the searched criteria and full details
            // (id, name, version, filename, download) for both candidates
            Assert.Equal(1, exitCode);
            Assert.Contains("Multiple packages found", output);
            Assert.Contains("download=https://github.com/demaconsulting/*", output);
            Assert.Contains("id=SPDXRef-Package-1", output);
            Assert.Contains("id=SPDXRef-Package-2", output);
            Assert.Contains("name=Test Package", output);
            Assert.Contains("name=Another Test Package", output);
            Assert.Contains("version=1.0.0", output);
            Assert.Contains("version=2.0.0", output);
            Assert.Contains("filename=package1.zip", output);
            Assert.Contains("filename=package2.tar", output);
            Assert.Contains("download=https://github.com/demaconsulting/SpdxTool]", output);
            Assert.Contains("download=https://github.com/demaconsulting/SpdxModel]", output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that find-package command reports "none" for missing optional fields of both
    ///     candidates when multiple packages match the criteria
    /// </summary>
    [Fact]
    public void FindPackage_Run_MultiplePackagesFoundWithMissingOptionalFields_ReportsNone()
    {
        // SPDX file where both candidate packages omit versionInfo, packageFileName, and
        // downloadLocation (the latter of which defaults to an empty string, not null)
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package-Minimal-1",
                  "name": "Minimal Package",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Package-Minimal-2",
                  "name": "Minimal Package",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-Minimal-1",
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
              "documentDescribes": [ "SPDXRef-Package-Minimal-1" ]
            }
            """;

        // Workflow contents - criteria that match both minimal packages by name
        const string workflowContents =
            """
            steps:
            - command: find-package
              inputs:
                output: packageId
                spdx: spdx.json
                name: Minimal Package
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

            // Assert: Verify error reported, with "none" for both candidates' missing
            // version, filename, and download location (including the empty-string default)
            Assert.Equal(1, exitCode);
            Assert.Contains("Multiple packages found", output);
            Assert.Contains("id=SPDXRef-Package-Minimal-1", output);
            Assert.Contains("id=SPDXRef-Package-Minimal-2", output);
            Assert.Contains(
                "[id=SPDXRef-Package-Minimal-1, name=Minimal Package, version=none, filename=none, download=none]",
                output);
            Assert.Contains(
                "[id=SPDXRef-Package-Minimal-2, name=Minimal Package, version=none, filename=none, download=none]",
                output);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that ParseCriteria throws CommandUsageException when a criterion has an empty key
    /// </summary>
    [Fact]
    public void FindPackage_ParseCriteria_EmptyKey_ThrowsCommandUsageException()
    {
        // Arrange: criteria argument where the key part is empty (e.g. "=value")
        var criteria = new Dictionary<string, string>();

        // Act / Assert: empty key throws CommandUsageException
        Assert.Throws<CommandUsageException>(
            () => FindPackage.ParseCriteria(["=value"], criteria));
    }

    /// <summary>
    ///     Test that find-package command by wildcard name finds the package
    /// </summary>
    [Fact]
    public void FindPackage_Run_ByNameWildcard_FindsPackage()
    {
        try
        {
            // Arrange: Write the SPDX file (SpdxContents has "Test Package" with id SPDXRef-Package-1)
            File.WriteAllText("spdx.json", SpdxContents);

            // Act: Run the command with a wildcard pattern that matches "Test Package"
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "find-package",
                "spdx.json",
                "name=Test*");

            // Assert: Verify the correct package ID is returned
            Assert.Equal(0, exitCode);
            Assert.Contains("SPDXRef-Package-1", output);
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }
}
