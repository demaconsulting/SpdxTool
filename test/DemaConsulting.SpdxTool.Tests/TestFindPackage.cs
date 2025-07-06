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

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Tests for the 'find-package' command
/// </summary>
[TestClass]
public class TestFindPackage
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
    ///     Test the 'find-package' command with missing arguments
    /// </summary>
    [TestMethod]
    public void FindPackage_MissingArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "find-package");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "'find-package' command missing arguments");
    }

    /// <summary>
    ///     Test the 'find-package' command with missing SPDX file
    /// </summary>
    [TestMethod]
    public void FindPackage_MissingFile()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "find-package",
            "missing.spdx.json",
            "name=anything");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "File not found: missing.spdx.json");
    }

    /// <summary>
    ///     Test the 'find-package' command from the command-line
    /// </summary>
    [TestMethod]
    public void FindPackage_CommandLine()
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
            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(output, "SPDXRef-Package-2");
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    /// <summary>
    ///     Test the 'find-package' command to find a package by name
    /// </summary>
    [TestMethod]
    public void FindPackage_ByName()
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
            // Arrange: Write the SPDX files
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
            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(output, "Found package SPDXRef-Package-1");
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test the 'find-package' command to find a package by version
    /// </summary>
    [TestMethod]
    public void FindPackage_ByVersion()
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
            // Arrange: Write the SPDX files
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
            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(output, "Found package SPDXRef-Package-2");
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test the 'find-package' command to find a package by file name
    /// </summary>
    [TestMethod]
    public void FindPackage_ByFileName()
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
            // Arrange: Write the SPDX files
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
            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(output, "Found package SPDXRef-Package-1");
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test the 'find-package' command to find a package by download location
    /// </summary>
    [TestMethod]
    public void FindPackage_ByDownload()
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
            // Arrange: Write the SPDX files
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
            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(output, "Found package SPDXRef-Package-2");
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}