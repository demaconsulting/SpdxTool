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
///     Tests for the 'diagram' command
/// </summary>
[TestClass]
public class DiagramTests
{
    /// <summary>
    ///     Test the 'diagram' command with missing arguments
    /// </summary>
    [TestMethod]
    public void DiagramCommand_MissingArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "diagram");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'diagram' command invalid arguments", output);
    }

    /// <summary>
    ///     Test the 'diagram' command with insufficient arguments
    /// </summary>
    [TestMethod]
    public void DiagramCommand_InsufficientArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "diagram",
            "test.spdx.json");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'diagram' command invalid arguments", output);
    }

    /// <summary>
    ///     Test the 'diagram' command with missing SPDX file
    /// </summary>
    [TestMethod]
    public void DiagramCommand_MissingSpdxFile()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "diagram",
            "missing.spdx.json",
            "output.mermaid.txt");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test the 'diagram' command with invalid option
    /// </summary>
    [TestMethod]
    public void DiagramCommand_InvalidOption()
    {
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
              }
            }
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "test.spdx.json",
                "output.mermaid.txt",
                "invalid-option");

            // Assert: Verify error reported
            Assert.AreEqual(1, exitCode);
            Assert.Contains("'diagram' command invalid option invalid-option", output);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test the 'diagram' command to generate a diagram
    /// </summary>
    [TestMethod]
    public void DiagramCommand_GenerateDiagram()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Application",
                  "name": "Test Application",
                  "versionInfo": "1.2.3",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Library",
                  "name": "Test Library",
                  "versionInfo": "2.3.4",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [
                {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "DESCRIBES"
                },
                {
                  "spdxElementId": "SPDXRef-Application",
                  "relatedSpdxElement": "SPDXRef-Library",
                  "relationshipType": "DEPENDS_ON"
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
              }
            }
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "test.spdx.json",
                "test.mermaid.txt");

            // Assert: Verify success reported
            Assert.AreEqual(0, exitCode);

            // Assert: Verify the mermaid file was created
            Assert.IsTrue(File.Exists("test.mermaid.txt"));
            var mermaid = File.ReadAllText("test.mermaid.txt");
            Assert.Contains("erDiagram", mermaid);
            Assert.Contains("Test Application / 1.2.3", mermaid);
            Assert.Contains("Test Library / 2.3.4", mermaid);
            Assert.Contains("DEPENDS_ON", mermaid);
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("test.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test the 'diagram' command with tools option
    /// </summary>
    [TestMethod]
    public void DiagramCommand_WithTools()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Application",
                  "name": "Test Application",
                  "versionInfo": "1.2.3",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Tool",
                  "name": "Build Tool",
                  "versionInfo": "3.4.5",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [
                {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "DESCRIBES"
                },
                {
                  "spdxElementId": "SPDXRef-Tool",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "BUILD_TOOL_OF"
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
              }
            }
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the command without tools option
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "test.spdx.json",
                "test-no-tools.mermaid.txt");

            // Assert: Verify success and tools not included
            Assert.AreEqual(0, exitCode);
            var mermaidNoTools = File.ReadAllText("test-no-tools.mermaid.txt");
            Assert.DoesNotContain("Build Tool", mermaidNoTools);

            // Act: Run the command with tools option
            exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "test.spdx.json",
                "test-with-tools.mermaid.txt",
                "tools");

            // Assert: Verify success and tools included
            Assert.AreEqual(0, exitCode);
            var mermaidWithTools = File.ReadAllText("test-with-tools.mermaid.txt");
            Assert.Contains("Build Tool / 3.4.5", mermaidWithTools);
            Assert.Contains("BUILD_TOOL_OF", mermaidWithTools);
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("test-no-tools.mermaid.txt");
            File.Delete("test-with-tools.mermaid.txt");
        }
    }
}
