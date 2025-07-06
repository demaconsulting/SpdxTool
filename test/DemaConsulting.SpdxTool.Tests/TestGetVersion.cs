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
///     Tests for the 'get-version' command
/// </summary>
[TestClass]
public class TestGetVersion
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
    ///     Test the 'get-version' command with missing arguments
    /// </summary>
    [TestMethod]
    public void GetVersion_MissingArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "get-version");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "'get-version' command missing arguments");
    }

    /// <summary>
    ///     Test the 'get-version' command with missing SPDX file
    /// </summary>
    [TestMethod]
    public void GetVersion_MissingFile()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "get-version",
            "missing.spdx.json",
            "id=SPDXRef-Package");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "File not found: missing.spdx.json");
    }

    /// <summary>
    ///     Test the 'get-version' command from the command line
    /// </summary>
    [TestMethod]
    public void GetVersion_CommandLine()
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
                "get-version",
                "spdx.json",
                "id=SPDXRef-Package-2");

            // Assert: Verify package ID
            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(output, "2.0.0");
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    /// <summary>
    ///     Test the 'get-version' command from a workflow
    /// </summary>
    [TestMethod]
    public void GetVersion_Workflow()
    {
        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: get-version
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-2
                output: version
            - command: print
              inputs:
                text:
                - Found version ${{ version }}
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
            StringAssert.Contains(output, "Found version 2.0.0");
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}