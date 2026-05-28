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
///     Tests for the 'help' command
/// </summary>
public class HelpTests
{
    /// <summary>
    ///     Test that help command with no arguments reports an error
    /// </summary>
    [Fact]
    public void Help_Run_NoArguments_ReportsError()
    {
        // Arrange: no setup required — the tool binary is invoked as a self-contained process

        // Act: Run the help command with no arguments
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help");

        // Assert: Verify an error was detected
        Assert.Equal(1, exitCode);
        Assert.Contains("'help' command missing arguments", output);
    }

    /// <summary>
    ///     Test that help command with unknown command reports an error
    /// </summary>
    [Fact]
    public void Help_Run_UnknownCommand_ReportsError()
    {
        // Arrange: no setup required — the tool binary is invoked as a self-contained process

        // Act: Run the help command with an unknown command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "unknown-command");

        // Assert: Verify an error was detected
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown command: 'unknown-command'", output);
    }

    /// <summary>
    ///     Test that help command with run-workflow displays help information
    /// </summary>
    [Fact]
    public void Help_Run_RunWorkflowCommand_DisplaysHelp()
    {
        // Arrange: no setup required — the tool binary is invoked as a self-contained process

        // Act: Run the help command with the 'run-workflow' command name
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "run-workflow");

        // Assert: Verify success
        Assert.Equal(0, exitCode);
        Assert.Contains("This command runs the steps specified in the workflow file/url.", output);
    }

    /// <summary>
    ///     Test that help command in a YAML workflow step displays help
    /// </summary>
    [Fact]
    public void Help_Run_YamlInvocation_DisplaysHelp()
    {
        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: help
              inputs:
                about: run-workflow
            """;

        try
        {
            // Arrange: no setup required — the tool binary is invoked as a self-contained process
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify help text is displayed
            Assert.Equal(0, exitCode);
            Assert.Contains("This command runs the steps specified in the workflow file/url.", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }
}
