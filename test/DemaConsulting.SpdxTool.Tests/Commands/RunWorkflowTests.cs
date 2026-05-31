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

using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'run-workflow' command.
/// </summary>
[Collection("CommandSequential")]
public partial class RunWorkflowTests
{
    /// <summary>
    ///     Regular expression to check for dotnet version
    /// </summary>
    /// <returns>A compiled <see cref="System.Text.RegularExpressions.Regex"/> that matches "DotNet version is x.y.z".</returns>
    [GeneratedRegex(@"DotNet version is \d+\.\d+\.\d+")]
    private static partial Regex DotnetVersionRegex();

    /// <summary>
    ///     Test that run-workflow command with an undeclared parameter reports an error
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_UndeclaredParameter_ReportsError()
    {
        const string fileContents =
            """
            parameters:
              about: help

            steps:
            - command: help
              inputs:
                about: ${{ about }}
            """;

        try
        {
            // Arrange: Write the file
            File.WriteAllText("help.yaml", fileContents);

            // Act: Run the workflow with an undeclared parameter
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "help.yaml",
                "undeclared=value");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("parameter undeclared not defined", output);
        }
        finally
        {
            File.Delete("help.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with a malformed CLI argument reports an error
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_MalformedCliArgument_ReportsError()
    {
        const string fileContents =
            """
            steps:
            - command: help
              inputs:
                about: help
            """;

        try
        {
            // Arrange: Write the file
            File.WriteAllText("help.yaml", fileContents);

            // Act: Run the workflow with a malformed argument (no '=' separator)
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "help.yaml",
                "malformed-no-equals");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("Invalid argument: malformed-no-equals", output);
        }
        finally
        {
            File.Delete("help.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_MissingArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "run-workflow");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'run-workflow' command missing arguments", output);
    }

    /// <summary>
    ///     Test that run-workflow command with missing file reports an error
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_MissingFile_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "run-workflow",
            "does-not-exist.yaml");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("File not found: does-not-exist.yaml", output);
    }

    /// <summary>
    ///     Test that run-workflow command with invalid workflow file reports an error
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_InvalidWorkflowFile_ReportsError()
    {
        const string fileContents =
            "missing-steps: 123";

        try
        {
            // Arrange: Write the file
            File.WriteAllText("invalid.yaml", fileContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "invalid.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("Error: Workflow invalid.yaml missing steps", output);
        }
        finally
        {
            // Delete the file
            File.Delete("invalid.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with missing parameter reports an error
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_MissingParameter_ReportsError()
    {
        const string fileContents =
            """
            steps:
            - command: help
            """;

        try
        {
            // Arrange: Write the file
            File.WriteAllText("invalid.yaml", fileContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "invalid.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'help' command missing 'about' input", output);
        }
        finally
        {
            // Delete the file
            File.Delete("invalid.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with valid workflow file executes the workflow
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_ValidWorkflowFile_ExecutesWorkflow()
    {
        const string fileContents =
            """
            steps:
            - command: help
              inputs:
                about: help
            """;

        try
        {
            // Arrange: Write the file
            File.WriteAllText("help.yaml", fileContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "help.yaml");

            // Assert: Verify success
            Assert.Equal(0, exitCode);
            Assert.Contains("This command displays extended help information about the specified command",
output);
        }
        finally
        {
            // Delete the file
            File.Delete("help.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with default parameters uses the defaults
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithDefaultParameters_UsesDefaults()
    {
        const string fileContents =
            """
            parameters:
              about: help

            steps:
            - command: help
              inputs:
                about: ${{ about }}
            """;

        try
        {
            // Arrange: Write the file
            File.WriteAllText("help.yaml", fileContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "help.yaml");

            // Assert: Verify success
            Assert.Equal(0, exitCode);
            Assert.Contains("This command displays extended help information about the specified command",
output);
        }
        finally
        {
            // Delete the file
            File.Delete("help.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with specified parameters uses the specified values
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithSpecifiedParameters_UsesSpecified()
    {
        const string fileContents =
            """
            parameters:
              about: help

            steps:
            - command: help
              inputs:
                about: ${{ about }}
            """;

        try
        {
            // Arrange: Write the file
            File.WriteAllText("help.yaml", fileContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "help.yaml",
                "about=to-markdown");

            // Assert: Verify success
            Assert.Equal(0, exitCode);
            Assert.Contains("This command produces a Markdown summary of an SPDX document", output);
        }
        finally
        {
            // Delete the file
            File.Delete("help.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with outputs populates the output variables
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithOutputs_PopulatesOutputs()
    {
        // Workflow2 file with exact string representation
        const string workflow2 =
            "parameters:\n" +
            "  in: unknown\n" +
            "\n" +
            "steps:\n" +
            "- command: set-variable\n" +
            "  inputs:\n" +
            "    value: Got ${{ in }} Param\n" +
            "    output: out\n";

        try
        {
            // Arrange: Write workflow2 and compute its SHA-256 hash
            File.WriteAllText("workflow2.yaml", workflow2);
            var hashBytes = SHA256.HashData(File.ReadAllBytes("workflow2.yaml"));
            var integrity = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Arrange: Build workflow1 with the computed integrity hash
            var workflow1 =
                "parameters:\n" +
                "  arg: unknown\n" +
                "\n" +
                "steps:\n" +
                "- command: run-workflow\n" +
                "  inputs:\n" +
                "    file: workflow2.yaml\n" +
                $"    integrity: {integrity}\n" +
                "    parameters:\n" +
                "      in: ${{ arg }}\n" +
                "    outputs:\n" +
                "      out: out-var\n" +
                "\n" +
                "- command: print\n" +
                "  inputs:\n" +
                "    text:\n" +
                "    - Output is ${{ out-var }}\n";
            File.WriteAllText("workflow1.yaml", workflow1);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml",
                "arg=Fred");

            // Assert: Verify success
            Assert.Equal(0, exitCode);
            Assert.Contains("Output is Got Fred Param", output);
        }
        finally
        {
            // Delete the files
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with bad integrity reports an error
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithBadIntegrity_ReportsError()
    {
        const string workflow1 =
            """
            parameters:
              arg: unknown

            steps:
            - command: run-workflow
              inputs:
                file: workflow2.yaml
                integrity: 0000000000000000000000000000000000000000000000000000000000000000
                parameters:
                  in: ${{ arg }}
                outputs:
                  out: out-var

            - command: print
              inputs:
                text:
                - Output is ${{ out-var }}
            """;

        const string workflow2 =
            """
            parameters:
              in: unknown

            steps:
            - command: set-variable
              inputs:
                value: Got ${{ in }} Param
                output: out
            """;

        try
        {
            // Arrange: Write the files
            File.WriteAllText("workflow1.yaml", workflow1);
            File.WriteAllText("workflow2.yaml", workflow2);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml",
                "arg=Fred");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("Error: Integrity check of workflow2.yaml failed", output);
        }
        finally
        {
            // Delete the files
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with NuGet workflow executes the workflow
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_NuGetWorkflow_ExecutesWorkflow()
    {
        const string workflow =
            """
            steps:
            - command: run-workflow
              inputs:
                nuget: "DemaConsulting.SpdxWorkflows:1.0.0"
                file: "contentFiles/any/any/workflows/GetDotNetVersion.yaml"
                outputs:
                  version: dotnet-version

            - command: print
              inputs:
                text:
                - DotNet version is ${{ dotnet-version }}
            """;

        try
        {
            // Arrange: Write the file
            File.WriteAllText("workflow.yaml", workflow);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.Equal(0, exitCode);
            Assert.Matches(DotnetVersionRegex(), output);
        }
        finally
        {
            // Delete the files
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with URL workflow executes the workflow
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_UrlWorkflow_ExecutesWorkflow()
    {
        const string workflow =
            """
            steps:
            - command: run-workflow
              inputs:
                url: 'https://raw.githubusercontent.com/demaconsulting/SpdxWorkflows/main/GetDotNetVersion.yaml'
                outputs:
                  version: dotnet-version

            - command: print
              inputs:
                text:
                - DotNet version is ${{ dotnet-version }}
            """;

        try
        {
            // Arrange: Write the file
            File.WriteAllText("workflow.yaml", workflow);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.Equal(0, exitCode);
            Assert.Matches(DotnetVersionRegex(), output);
        }
        finally
        {
            // Delete the files
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with valid integrity executes the workflow
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithValidIntegrity_ExecutesWorkflow()
    {
        // Exact byte content for workflow2 so the SHA-256 hash is deterministic
        const string workflow2 =
            "steps:\n" +
            "- command: help\n" +
            "  inputs:\n" +
            "    about: help\n";

        try
        {
            // Arrange: Write the sub-workflow file and compute its exact SHA-256 hash
            File.WriteAllText("workflow2.yaml", workflow2);
            var bytes = File.ReadAllBytes("workflow2.yaml");
            var hashBytes = SHA256.HashData(bytes);
            var integrity = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Arrange: Write the outer workflow referencing the sub-workflow with its correct hash
            var workflow1 = $"""
                steps:
                - command: run-workflow
                  inputs:
                    file: workflow2.yaml
                    integrity: {integrity}
                """;
            File.WriteAllText("workflow1.yaml", workflow1);

            // Act: Run the outer workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml");

            // Assert: Verify success — the inner help step ran
            Assert.Equal(0, exitCode);
            Assert.Contains(
                "This command displays extended help information about the specified command",
                output);
        }
        finally
        {
            // Delete the files
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with a valid uppercase integrity hash executes the workflow
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithUppercaseIntegrity_ExecutesWorkflow()
    {
        // Exact byte content for workflow2 so the SHA-256 hash is deterministic
        const string workflow2 =
            "steps:\n" +
            "- command: help\n" +
            "  inputs:\n" +
            "    about: help\n";

        try
        {
            // Arrange: Write the sub-workflow file and compute its SHA-256 hash in uppercase
            File.WriteAllText("workflow2.yaml", workflow2);
            var bytes = File.ReadAllBytes("workflow2.yaml");
            var hashBytes = SHA256.HashData(bytes);
            var integrity = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

            // Arrange: Write the outer workflow referencing the sub-workflow with its uppercase hash
            var workflow1 = $"""
                steps:
                - command: run-workflow
                  inputs:
                    file: workflow2.yaml
                    integrity: {integrity}
                """;
            File.WriteAllText("workflow1.yaml", workflow1);

            // Act: Run the outer workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml");

            // Assert: Verify success — the inner help step ran
            Assert.Equal(0, exitCode);
            Assert.Contains(
                "This command displays extended help information about the specified command",
                output);
        }
        finally
        {
            // Delete the files
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with --verbose prints workflow output variables
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithVerboseFlag_PrintsOutputs()
    {
        const string fileContents =
            """
            parameters:
              result: hello-world

            steps:
            - command: help
              inputs:
                about: help
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("verbose.yaml", fileContents);

            // Act: Run the workflow with the --verbose flag
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "verbose.yaml",
                "--verbose");

            // Assert: Verify success and that output variables are printed
            Assert.Equal(0, exitCode);
            Assert.Contains("Outputs:", output);
            Assert.Contains("result = hello-world", output);
        }
        finally
        {
            File.Delete("verbose.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command with an outputs mapping referencing a missing
    ///     sub-workflow variable reports an error
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithMissingOutput_ReportsError()
    {
        const string workflow2 =
            "steps:\n" +
            "- command: set-variable\n" +
            "  inputs:\n" +
            "    value: some-value\n" +
            "    output: out\n";

        const string workflow1 =
            "steps:\n" +
            "- command: run-workflow\n" +
            "  inputs:\n" +
            "    file: workflow2.yaml\n" +
            "    outputs:\n" +
            "      missing-output: out-var\n";

        try
        {
            // Arrange: Write the workflow files
            File.WriteAllText("workflow1.yaml", workflow1);
            File.WriteAllText("workflow2.yaml", workflow2);

            // Act: Run the outer workflow requesting an output the sub-workflow never produces
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml");

            // Assert: Verify error reported for missing output
            Assert.Equal(1, exitCode);
            Assert.Contains("Workflow did not produce missing-output output", output);
        }
        finally
        {
            // Delete the files
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }

    /// <summary>
    ///     Test that run-workflow command prints the displayName label before a step executes
    /// </summary>
    [Fact]
    public void RunWorkflow_Run_WithDisplayName_PrintsLabel()
    {
        const string fileContents =
            """
            steps:
            - command: help
              displayName: Testing Help Command
              inputs:
                about: help
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("displayname.yaml", fileContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "displayname.yaml");

            // Assert: Verify success and that the displayName was printed
            Assert.Equal(0, exitCode);
            Assert.Contains("Testing Help Command", output);
        }
        finally
        {
            File.Delete("displayname.yaml");
        }
    }
}
