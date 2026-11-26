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
///     Tests for the 'help' command
/// </summary>
public class HelpTests
{
    /// <summary>
    ///     Test that help command on the command line reports a workflow-only error
    /// </summary>
    [Fact]
    public void Help_Run_OnCommandLine_ReportsWorkflowOnlyError()
    {
        // Arrange: create a minimal execution context
        using var context = Context.Create([]);

        // Act / Assert: CLI invocation always throws because the command is workflow-only
        var exception = Assert.Throws<CommandUsageException>(() => Help.Instance.Run(context, ["run-workflow"]));
        Assert.Equal("'help' command is only valid in a workflow", exception.Message);
    }

    /// <summary>
    ///     Test that help command in a YAML workflow step with an unknown command reports an error
    /// </summary>
    [Fact]
    public void Help_Run_UnknownCommand_ReportsError()
    {
        // Arrange: workflow step asks about a command that does not exist
        const string workflowContents =
            """
            steps:
            - command: help
              inputs:
                about: unknown-command
            """;

        var workflowFile = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: write the workflow file to disk
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify an error was detected
            Assert.Equal(1, exitCode);
            Assert.Contains("Unknown command: 'unknown-command'", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that help command in a YAML workflow step reports an error when the about input is absent
    /// </summary>
    [Fact]
    public void Help_Run_YamlInvocation_MissingAbout_ReportsError()
    {
        // Arrange: workflow step omits the required 'about' input
        const string workflowContents =
            """
            steps:
            - command: help
              inputs: {}
            """;

        var workflowFile = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: write the workflow file to disk
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: non-zero exit code reported for missing input
            Assert.Equal(1, exitCode);
            Assert.Contains("'help' command missing 'about' input", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that help command in a YAML workflow step displays help
    /// </summary>
    [Fact]
    public void Help_Run_YamlInvocation_DisplaysHelp()
    {
        // Arrange: workflow step specifies 'about: run-workflow'
        const string workflowContents =
            """
            steps:
            - command: help
              inputs:
                about: run-workflow
            """;

        var workflowFile = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: write the workflow file to disk so the tool can load it
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify help text is displayed
            Assert.Equal(0, exitCode);
            Assert.Contains("This command runs the steps specified in the workflow file/url.", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }
}
