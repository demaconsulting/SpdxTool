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

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'diagram' command
/// </summary>
public class DiagramTests
{
    /// <summary>
    ///     Test that diagram command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void Diagram_Run_MissingArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "diagram");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'diagram' command invalid arguments", output);
    }

    /// <summary>
    ///     Test that diagram command with insufficient arguments reports an error
    /// </summary>
    [Fact]
    public void Diagram_Run_InsufficientArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "diagram",
            "test.spdx.json");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'diagram' command invalid arguments", output);
    }

    /// <summary>
    ///     Test that diagram command with missing SPDX file reports an error
    /// </summary>
    [Fact]
    public void Diagram_Run_MissingSpdxFile_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "diagram",
            "missing.spdx.json",
            "output.mermaid.txt");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test that diagram command with invalid option reports an error
    /// </summary>
    [Fact]
    public void Diagram_Run_InvalidOption_ReportsError()
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
            File.WriteAllText("diagram-invalidoption.spdx.json", spdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-invalidoption.spdx.json",
                "output.mermaid.txt",
                "invalid-option");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'diagram' command invalid option invalid-option", output);
        }
        finally
        {
            File.Delete("diagram-invalidoption.spdx.json");
        }
    }

    /// <summary>
    ///     Test that diagram command with valid SPDX file generates a diagram
    /// </summary>
    [Fact]
    public void Diagram_Run_ValidSpdxFile_GeneratesDiagram()
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
            File.WriteAllText("diagram-validfile.spdx.json", spdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-validfile.spdx.json",
                "test.mermaid.txt");

            // Assert: Verify success reported
            Assert.Equal(0, exitCode);

            // Assert: Verify the mermaid file was created
            Assert.True(File.Exists("test.mermaid.txt"));
            var mermaid = File.ReadAllText("test.mermaid.txt");
            Assert.Contains("erDiagram", mermaid);
            Assert.Contains("Test Application / 1.2.3", mermaid);
            Assert.Contains("Test Library / 2.3.4", mermaid);
            Assert.Contains("DEPENDS_ON", mermaid);
        }
        finally
        {
            File.Delete("diagram-validfile.spdx.json");
            File.Delete("test.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test that diagram command with tools option generates diagram with tools
    /// </summary>
    [Fact]
    public void Diagram_Run_WithToolsOption_GeneratesDiagramWithTools()
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
            File.WriteAllText("diagram-withtools.spdx.json", spdxContents);

            // Act: Run the command with tools option
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-withtools.spdx.json",
                "test-with-tools.mermaid.txt",
                "tools");

            // Assert: Verify success and tools included
            Assert.Equal(0, exitCode);
            var mermaidWithTools = File.ReadAllText("test-with-tools.mermaid.txt");
            Assert.Contains("Build Tool / 3.4.5", mermaidWithTools);
            Assert.Contains("BUILD_TOOL_OF", mermaidWithTools);
        }
        finally
        {
            File.Delete("diagram-withtools.spdx.json");
            File.Delete("test-with-tools.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test that diagram command without tools option excludes tool relationships
    /// </summary>
    [Fact]
    public void Diagram_Run_WithoutToolsOption_ExcludesToolRelationships()
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
            File.WriteAllText("diagram-notools.spdx.json", spdxContents);

            // Act: Run the command without tools option
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-notools.spdx.json",
                "test-no-tools.mermaid.txt");

            // Assert: Verify success and tools not included
            Assert.Equal(0, exitCode);
            var mermaidNoTools = File.ReadAllText("test-no-tools.mermaid.txt");
            Assert.DoesNotContain("Build Tool", mermaidNoTools);
        }
        finally
        {
            File.Delete("diagram-notools.spdx.json");
            File.Delete("test-no-tools.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test that diagram command in workflow with missing 'spdx' input reports error
    /// </summary>
    [Fact]
    public void Diagram_Run_MissingSpdxInput_ReportsError()
    {
        // Workflow contents - missing 'spdx' input
        const string workflowContents =
            """
            steps:
            - command: diagram
              inputs:
                mermaid: output.mermaid.txt
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("test.workflow.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "test.workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'diagram' command missing 'spdx' input", output);
        }
        finally
        {
            File.Delete("test.workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that diagram command in workflow with missing 'mermaid' input reports error
    /// </summary>
    [Fact]
    public void Diagram_Run_MissingMermaidInput_ReportsError()
    {
        // Workflow contents - missing 'mermaid' input
        const string workflowContents =
            """
            steps:
            - command: diagram
              inputs:
                spdx: test.spdx.json
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("test.workflow.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "test.workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'diagram' command missing 'mermaid' input", output);
        }
        finally
        {
            File.Delete("test.workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that diagram command in workflow with invalid 'tools' value reports error
    /// </summary>
    [Fact]
    public void Diagram_Run_InvalidToolsInput_ReportsError()
    {
        // Workflow contents - non-boolean value for 'tools'
        const string workflowContents =
            """
            steps:
            - command: diagram
              inputs:
                spdx: test.spdx.json
                mermaid: output.mermaid.txt
                tools: not-a-boolean
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("test.workflow.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "test.workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'diagram' invalid 'tools' input", output);
        }
        finally
        {
            File.Delete("test.workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that diagram command in a workflow step generates a diagram from YAML inputs
    /// </summary>
    [Fact]
    public void Diagram_Run_InWorkflow_GeneratesDiagram()
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

        const string workflowContents =
            """
            steps:
            - command: diagram
              inputs:
                spdx: diagram-workflow.spdx.json
                mermaid: test.workflow.mermaid.txt
            """;

        try
        {
            // Arrange: Write the SPDX file and workflow file
            File.WriteAllText("diagram-workflow.spdx.json", spdxContents);
            File.WriteAllText("test.workflow.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "test.workflow.yaml");

            // Assert: Verify success and mermaid file was created with expected content
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists("test.workflow.mermaid.txt"));
            var mermaid = File.ReadAllText("test.workflow.mermaid.txt");
            Assert.Contains("erDiagram", mermaid);
            Assert.Contains("Test Application / 1.2.3", mermaid);
            Assert.Contains("Test Library / 2.3.4", mermaid);
            Assert.Contains("DEPENDS_ON", mermaid);
        }
        finally
        {
            File.Delete("diagram-workflow.spdx.json");
            File.Delete("test.workflow.yaml");
            File.Delete("test.workflow.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test that diagram command without tools option excludes DEV_TOOL_OF relationships
    /// </summary>
    [Fact]
    public void Diagram_Run_WithDevToolOfRelationship_ExcludedFromDefaultDiagram()
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
                  "SPDXID": "SPDXRef-DevTool",
                  "name": "Dev Tool",
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
                  "spdxElementId": "SPDXRef-DevTool",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "DEV_TOOL_OF"
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
            File.WriteAllText("diagram-devtoolof-notools.spdx.json", spdxContents);

            // Act: Run the command without tools option
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-devtoolof-notools.spdx.json",
                "diagram-devtoolof-notools.mermaid.txt");

            // Assert: Verify success and DEV_TOOL_OF relationship excluded
            Assert.Equal(0, exitCode);
            var mermaid = File.ReadAllText("diagram-devtoolof-notools.mermaid.txt");
            Assert.DoesNotContain("Dev Tool", mermaid);
        }
        finally
        {
            File.Delete("diagram-devtoolof-notools.spdx.json");
            File.Delete("diagram-devtoolof-notools.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test that diagram command with tools option includes DEV_TOOL_OF relationships
    /// </summary>
    [Fact]
    public void Diagram_Run_WithDevToolOfRelationship_IncludedInToolsDiagram()
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
                  "SPDXID": "SPDXRef-DevTool",
                  "name": "Dev Tool",
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
                  "spdxElementId": "SPDXRef-DevTool",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "DEV_TOOL_OF"
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
            File.WriteAllText("diagram-devtoolof-tools.spdx.json", spdxContents);

            // Act: Run the command with tools option
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-devtoolof-tools.spdx.json",
                "diagram-devtoolof-tools.mermaid.txt",
                "tools");

            // Assert: Verify success and DEV_TOOL_OF relationship included
            Assert.Equal(0, exitCode);
            var mermaid = File.ReadAllText("diagram-devtoolof-tools.mermaid.txt");
            Assert.Contains("Dev Tool / 3.4.5", mermaid);
            Assert.Contains("DEV_TOOL_OF", mermaid);
        }
        finally
        {
            File.Delete("diagram-devtoolof-tools.spdx.json");
            File.Delete("diagram-devtoolof-tools.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test that diagram command without tools option excludes TEST_TOOL_OF relationships
    /// </summary>
    [Fact]
    public void Diagram_Run_WithTestToolOfRelationship_ExcludedFromDefaultDiagram()
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
                  "SPDXID": "SPDXRef-TestTool",
                  "name": "Test Tool",
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
                  "spdxElementId": "SPDXRef-TestTool",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "TEST_TOOL_OF"
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
            File.WriteAllText("diagram-testtoolof-notools.spdx.json", spdxContents);

            // Act: Run the command without tools option
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-testtoolof-notools.spdx.json",
                "diagram-testtoolof-notools.mermaid.txt");

            // Assert: Verify success and TEST_TOOL_OF relationship excluded
            Assert.Equal(0, exitCode);
            var mermaid = File.ReadAllText("diagram-testtoolof-notools.mermaid.txt");
            Assert.DoesNotContain("Test Tool", mermaid);
        }
        finally
        {
            File.Delete("diagram-testtoolof-notools.spdx.json");
            File.Delete("diagram-testtoolof-notools.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test that diagram command with tools option includes TEST_TOOL_OF relationships
    /// </summary>
    [Fact]
    public void Diagram_Run_WithTestToolOfRelationship_IncludedInToolsDiagram()
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
                  "SPDXID": "SPDXRef-TestTool",
                  "name": "Test Tool",
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
                  "spdxElementId": "SPDXRef-TestTool",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "TEST_TOOL_OF"
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
            File.WriteAllText("diagram-testtoolof-tools.spdx.json", spdxContents);

            // Act: Run the command with tools option
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-testtoolof-tools.spdx.json",
                "diagram-testtoolof-tools.mermaid.txt",
                "tools");

            // Assert: Verify success and TEST_TOOL_OF relationship included
            Assert.Equal(0, exitCode);
            var mermaid = File.ReadAllText("diagram-testtoolof-tools.mermaid.txt");
            Assert.Contains("Test Tool / 3.4.5", mermaid);
            Assert.Contains("TEST_TOOL_OF", mermaid);
        }
        finally
        {
            File.Delete("diagram-testtoolof-tools.spdx.json");
            File.Delete("diagram-testtoolof-tools.mermaid.txt");
        }
    }

    /// <summary>
    ///     Test that diagram command with a package missing versionInfo uses "unspecified" as the fallback
    /// </summary>
    [Fact]
    public void Diagram_Run_PackageWithoutVersion_UsesUnspecifiedFallback()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Application",
                  "name": "Test Application",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Library",
                  "name": "Test Library",
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
            // Arrange: Write the SPDX file with packages that have no versionInfo
            File.WriteAllText("diagram-noversion.spdx.json", spdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "diagram",
                "diagram-noversion.spdx.json",
                "test-no-version.mermaid.txt");

            // Assert: Verify success
            Assert.Equal(0, exitCode);

            // Assert: Verify the fallback "unspecified" text is used in the output
            Assert.True(File.Exists("test-no-version.mermaid.txt"));
            var mermaid = File.ReadAllText("test-no-version.mermaid.txt");
            Assert.Contains("Test Application / unspecified", mermaid);
            Assert.Contains("Test Library / unspecified", mermaid);
        }
        finally
        {
            File.Delete("diagram-noversion.spdx.json");
            File.Delete("test-no-version.mermaid.txt");
        }
    }
}
