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

using System.Text.RegularExpressions;

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Tests for the 'run-workflow' command.
/// </summary>
[TestClass]
public partial class TestRunWorkflow
{
    /// <summary>
    ///     Regular expression to check for dotnet version
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"DotNet version is \d+\.\d+\.\d+")]
    private static partial Regex DotnetVersionRegex();

    /// <summary>
    ///     Test the 'run-workflow' command with missing arguments.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_MissingArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "run-workflow");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'run-workflow' command missing arguments", output);
    }

    /// <summary>
    ///     Test the 'run-workflow' command with missing file.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_MissingFile()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "run-workflow",
            "does-not-exist.yaml");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("File not found: does-not-exist.yaml", output);
    }

    /// <summary>
    ///     Test the 'run-workflow' command with invalid file.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_FileInvalid()
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
            Assert.AreEqual(1, exitCode);
            Assert.Contains("Error: Workflow invalid.yaml missing steps", output);
        }
        finally
        {
            // Delete the file
            File.Delete("invalid.yaml");
        }
    }

    /// <summary>
    ///     Test the 'run-workflow' command with missing parameter in file.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_MissingParameterInFile()
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
            Assert.AreEqual(1, exitCode);
            Assert.Contains("'help' command missing 'about' input", output);
        }
        finally
        {
            // Delete the file
            File.Delete("invalid.yaml");
        }
    }

    /// <summary>
    ///     Test the 'run-workflow' command with a basic workflow file.
    /// </summary>
    [TestMethod]
    public void RunWorkflow()
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
            Assert.AreEqual(0, exitCode);
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
    ///     Test the 'run-workflow' command with default parameters.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_WithDefaultParameters()
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
            Assert.AreEqual(0, exitCode);
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
    ///     Test the 'run-workflow' command with specified parameters.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_WithSpecifiedParameters()
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
            Assert.AreEqual(0, exitCode);
            Assert.Contains("This command produces a Markdown summary of an SPDX document", output);
        }
        finally
        {
            // Delete the file
            File.Delete("help.yaml");
        }
    }

    /// <summary>
    ///     Test the 'run-workflow' command with outputs.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_WithOutputs()
    {
        const string workflow1 =
            """
            parameters:
              arg: unknown

            steps:
            - command: run-workflow
              inputs:
                file: workflow2.yaml
                integrity: 7c8cbebe55ab1094e513bd50e05823820c4b0229c19d4e8edfbfa3a3765b2be2
                parameters:
                  in: ${{ arg }}
                outputs:
                  out: out-var

            - command: print
              inputs:
                text:
                - Output is ${{ out-var }}
            """;

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

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);
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
    ///     Test the 'run-workflow' command with bad file integrity.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_WithBadIntegrity()
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

            // Assert: Verify success
            Assert.AreEqual(1, exitCode);
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
    ///     Test the 'run-workflow' command with workflow URL.
    /// </summary>
    [TestMethod]
    public void RunWorkflow_Url()
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
            Assert.AreEqual(0, exitCode);
            Assert.MatchesRegex(DotnetVersionRegex(), output);
        }
        finally
        {
            // Delete the files
            File.Delete("workflow.yaml");
        }
    }
}