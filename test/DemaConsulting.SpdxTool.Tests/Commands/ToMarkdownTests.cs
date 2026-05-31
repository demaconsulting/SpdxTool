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

using DemaConsulting.SpdxTool;
using DemaConsulting.SpdxTool.Commands;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'to-markdown' command.
/// </summary>
[Collection("CommandSequential")]
public class ToMarkdownTests
{
    /// <summary>
    ///     Test that to-markdown command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_MissingArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "to-markdown");

        // Assert: Verify the conversion failed
        Assert.Equal(1, exitCode);
        Assert.Contains("'to-markdown' command missing arguments", output);
    }

    /// <summary>
    ///     Test that to-markdown command with missing SPDX file reports an error
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_MissingSpdxFile_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "to-markdown",
            "missing.spdx.json",
            "output.md");

        // Assert: Verify the conversion failed
        Assert.Equal(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test that to-markdown command with valid SPDX file generates markdown
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_ValidSpdxFile_GeneratesMarkdown()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [    {
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
                },
                {
                  "SPDXID": "SPDXRef-Tool",
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
                  "spdxElementId": "SPDXRef-Application",
                  "relatedSpdxElement": "SPDXRef-Library",
                  "relationshipType": "CONTAINS"
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
              },
              "documentDescribes": [ "SPDXRef-Application" ]
            }
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the tool
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "to-markdown",
                "test.spdx.json",
                "test.md");

            // Assert: Verify the conversion succeeded
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists("test.md"));

            // Read the Markdown text
            var markdown = File.ReadAllText("test.md");

            // Assert: Verify the contents
            Assert.Contains("## SPDX Document", markdown);
            Assert.Contains("| File Name | test.spdx.json |", markdown);
            Assert.Contains("| Name | Test Document |", markdown);

            // Assert: Verify the root packages section
            var rootPackagesIndex = markdown.IndexOf("# Root Packages", StringComparison.Ordinal);
            Assert.True(rootPackagesIndex >= 0);

            // Assert: Verify the packages section
            var packagesIndex = markdown.IndexOf("# Packages", StringComparison.Ordinal);
            Assert.True(packagesIndex >= 0);

            // Assert: Verify the tools section
            var toolsIndex = markdown.IndexOf("# Tools", StringComparison.Ordinal);
            Assert.True(toolsIndex >= 0);

            // Assert: Verify "Test Application" is a root package
            var testPackageIndex = markdown.IndexOf("| Test Application | 1.2.3 | MIT |", StringComparison.Ordinal);
            Assert.True(testPackageIndex > rootPackagesIndex && testPackageIndex < packagesIndex);

            // Assert: Verify "Test Library" is a package
            var testLibraryIndex = markdown.IndexOf("| Test Library | 2.3.4 | MIT |", StringComparison.Ordinal);
            Assert.True(testLibraryIndex > packagesIndex && testLibraryIndex < toolsIndex);

            // Assert: Verify "Test Tool" is a tool
            var testToolPosition = markdown.IndexOf("| Test Tool | 3.4.5 | MIT |", StringComparison.Ordinal);
            Assert.True(testToolPosition > toolsIndex);
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("test.md");
        }
    }

    /// <summary>
    ///     Test that to-markdown command run in a workflow generates markdown
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_InWorkflow_GeneratesMarkdown()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Application",
                  "name": "Test Application",
                  "versionInfo": "1.2.3",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [
                {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Application",
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
              "documentDescribes": [ "SPDXRef-Application" ]
            }
            """;

        const string workflowContents =
            """
            steps:
            - command: to-markdown
              inputs:
                spdx: test.spdx.json
                markdown: test.md
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("test.spdx.json", spdxContents);
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
            Assert.True(File.Exists("test.md"));

            // Assert: Verify the markdown contains the expected heading
            var markdown = File.ReadAllText("test.md");
            Assert.Contains("## SPDX Document", markdown);
        }
        finally
        {
            File.Delete("test.spdx.json");
            File.Delete("test.md");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that to-markdown command with a whitespace title reports an error
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_InvalidTitle_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "to-markdown",
            "test.spdx.json",
            "test.md",
            "   ");

        // Assert: Verify the error was reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'to-markdown' command invalid 'title' argument", output);
    }

    /// <summary>
    ///     Test that to-markdown command with a non-positive depth reports an error
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_InvalidDepth_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "to-markdown",
            "test.spdx.json",
            "test.md",
            "My Title",
            "0");

        // Assert: Verify the error was reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'to-markdown' command invalid 'depth' argument", output);
    }

    /// <summary>
    ///     Test that to-markdown YAML run throws when the spdx input is missing
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_YamlMissingSpdxInput_ThrowsException()
    {
        // Arrange: step node with markdown but no spdx input
        using var context = Context.Create([]);
        var variables = new Dictionary<string, string>();
        var inputs = new YamlMappingNode();
        inputs.Add("markdown", "out.md");
        var step = new YamlMappingNode();
        step.Add("inputs", inputs);

        // Act / Assert: absent spdx input must produce a YamlException
        Assert.Throws<YamlException>(() => ToMarkdown.Instance.Run(context, step, variables));
    }

    /// <summary>
    ///     Test that to-markdown YAML run throws when the markdown input is missing
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_YamlMissingMarkdownInput_ThrowsException()
    {
        // Arrange: step node with spdx but no markdown input
        using var context = Context.Create([]);
        var variables = new Dictionary<string, string>();
        var inputs = new YamlMappingNode();
        inputs.Add("spdx", "test.spdx.json");
        var step = new YamlMappingNode();
        step.Add("inputs", inputs);

        // Act / Assert: absent markdown input must produce a YamlException
        Assert.Throws<YamlException>(() => ToMarkdown.Instance.Run(context, step, variables));
    }

    /// <summary>
    ///     Test that to-markdown YAML run throws when the title is whitespace
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_YamlWhitespaceTitle_ThrowsException()
    {
        // Arrange: step node with required inputs plus a whitespace-only title
        using var context = Context.Create([]);
        var variables = new Dictionary<string, string>();
        var inputs = new YamlMappingNode();
        inputs.Add("spdx", "test.spdx.json");
        inputs.Add("markdown", "out.md");
        inputs.Add("title", "   ");
        var step = new YamlMappingNode();
        step.Add("inputs", inputs);

        // Act / Assert: whitespace title is invalid and must produce a YamlException
        Assert.Throws<YamlException>(() => ToMarkdown.Instance.Run(context, step, variables));
    }

    /// <summary>
    ///     Test that to-markdown YAML run throws when the depth is non-positive
    /// </summary>
    [Fact]
    public void ToMarkdown_Run_YamlNonPositiveDepth_ThrowsException()
    {
        // Arrange: step node with required inputs plus a zero depth value
        using var context = Context.Create([]);
        var variables = new Dictionary<string, string>();
        var inputs = new YamlMappingNode();
        inputs.Add("spdx", "test.spdx.json");
        inputs.Add("markdown", "out.md");
        inputs.Add("depth", "0");
        var step = new YamlMappingNode();
        step.Add("inputs", inputs);

        // Act / Assert: depth of zero is not a positive integer and must produce a YamlException
        Assert.Throws<YamlException>(() => ToMarkdown.Instance.Run(context, step, variables));
    }

    /// <summary>
    ///     Test that a package with NOASSERTION concluded license falls back to the declared license
    /// </summary>
    [Fact]
    public void ToMarkdown_License_ConcludedIsNoAssertion_UsesDeclaredLicense()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://example.com",
                  "licenseConcluded": "NOASSERTION",
                  "licenseDeclared": "Apache-2.0"
                }
              ],
              "relationships": [
                {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package",
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
              "documentDescribes": [ "SPDXRef-Package" ]
            }
            """;

        try
        {
            // Arrange: Write the SPDX file with NOASSERTION concluded and valid declared license
            File.WriteAllText("license-fallback-test.spdx.json", spdxContents);

            // Act: Generate the markdown
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "to-markdown",
                "license-fallback-test.spdx.json",
                "license-fallback-test.md");

            // Assert: The declared license appears in the output (fallback from NOASSERTION concluded)
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists("license-fallback-test.md"));
            var markdown = File.ReadAllText("license-fallback-test.md");
            Assert.Contains("Apache-2.0", markdown);
        }
        finally
        {
            File.Delete("license-fallback-test.spdx.json");
            File.Delete("license-fallback-test.md");
        }
    }

    /// <summary>
    ///     Test that a package with both concluded and declared licenses set to NOASSERTION
    ///     shows NOASSERTION in the markdown output
    /// </summary>
    [Fact]
    public void ToMarkdown_License_BothLicensesNoAssertion_UsesNoAssertion()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://example.com",
                  "licenseConcluded": "NOASSERTION",
                  "licenseDeclared": "NOASSERTION"
                }
              ],
              "relationships": [
                {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package",
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
              "documentDescribes": [ "SPDXRef-Package" ]
            }
            """;

        try
        {
            // Arrange: Write the SPDX file where both license fields are NOASSERTION
            File.WriteAllText("license-noassertion-test.spdx.json", spdxContents);

            // Act: Generate the markdown
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "to-markdown",
                "license-noassertion-test.spdx.json",
                "license-noassertion-test.md");

            // Assert: NOASSERTION appears in the output (both fallback paths exhausted)
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists("license-noassertion-test.md"));
            var markdown = File.ReadAllText("license-noassertion-test.md");
            Assert.Contains("NOASSERTION", markdown);
        }
        finally
        {
            File.Delete("license-noassertion-test.spdx.json");
            File.Delete("license-noassertion-test.md");
        }
    }

    /// <summary>
    ///     Test that a minimal document with only root packages (no dependencies, no tools) does
    ///     not emit the Packages or Tools sections in the generated markdown
    /// </summary>
    [Fact]
    public void ToMarkdown_GenerateSummaryMarkdown_OnlyRootPackages_SuppressesPackagesAndToolsSections()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Root Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://example.com",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [
                {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package",
                  "relationshipType": "DESCRIBES"
                }
              ],
              "spdxVersion": "SPDX-2.2",
              "dataLicense": "CC0-1.0",
              "SPDXID": "SPDXRef-DOCUMENT",
              "name": "Minimal Document",
              "documentNamespace": "https://sbom.spdx.org",
              "creationInfo": {
                "created": "2021-10-01T00:00:00Z",
                "creators": [ "Person: Malcolm Nixon" ]
              },
              "documentDescribes": [ "SPDXRef-Package" ]
            }
            """;

        try
        {
            // Arrange: Write a minimal SPDX file with only a single root package
            File.WriteAllText("minimal-only-root-test.spdx.json", spdxContents);

            // Act: Generate the markdown
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "to-markdown",
                "minimal-only-root-test.spdx.json",
                "minimal-only-root-test.md");

            // Assert: Verify the markdown was generated successfully
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists("minimal-only-root-test.md"));
            var markdown = File.ReadAllText("minimal-only-root-test.md");

            // Assert: Root packages section is present
            Assert.Contains("Root Packages", markdown);

            // Assert: Packages section is absent (no non-root, non-tool packages)
            Assert.DoesNotContain("# Packages", markdown);

            // Assert: Tools section is absent (no tool packages)
            Assert.DoesNotContain("# Tools", markdown);
        }
        finally
        {
            File.Delete("minimal-only-root-test.spdx.json");
            File.Delete("minimal-only-root-test.md");
        }
    }
}
