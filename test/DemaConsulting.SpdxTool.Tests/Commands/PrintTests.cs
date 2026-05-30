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
///     Tests for the 'print' command
/// </summary>
public class PrintTests
{
    /// <summary>
    ///     Test that print command on command line prints the text
    /// </summary>
    [Fact]
    public void Print_Run_OnCommandLine_PrintsText()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "print",
            "Hello, World!");

        // Assert: Verify output
        Assert.Equal(0, exitCode);
        Assert.Contains("Hello, World!", output);
    }

    /// <summary>
    ///     Test that print command in workflow prints the text
    /// </summary>
    [Fact]
    public void Print_Run_InWorkflow_PrintsText()
    {
        // Workflow contents
        const string workflowContents =
            """
            parameters:
              p1: Hello
              p2: World

            steps:
            - command: print
              inputs:
                text:
                - The first parameter is ${{ p1 }}.
                - ${{ p2 }} is the second parameter.
            """;

        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify success
            Assert.Equal(0, exitCode);
            Assert.Contains("The first parameter is Hello.", output);
            Assert.Contains("World is the second parameter.", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that print command in workflow without text input reports an error
    /// </summary>
    [Fact]
    public void Print_Run_MissingTextInput_ReportsError()
    {
        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: print
              inputs: {}
            """;

        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify error is reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'print' command missing 'text' input", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that print command in workflow reports an error when a text line references an undefined variable
    /// </summary>
    [Fact]
    public void Print_Run_UndefinedVariable_ReportsError()
    {
        // Workflow contents — references a variable that is not defined
        const string workflowContents =
            """
            steps:
            - command: print
              inputs:
                text:
                - ${{ unknown_var }}
            """;

        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify non-zero exit code and error message
            Assert.Equal(1, exitCode);
            Assert.Contains("Undefined variable unknown_var", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that print command in workflow reports an error when a text line contains an empty variable name
    /// </summary>
    [Fact]
    public void Print_Run_EmptyVariableName_ReportsError()
    {
        // Workflow contents — macro delimiter encloses only whitespace
        const string workflowContents =
            """
            steps:
            - command: print
              inputs:
                text:
                - "${{  }}"
            """;

        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify non-zero exit code and error message
            Assert.Equal(1, exitCode);
            Assert.Contains("Empty variable name in macro expansion", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that print command in workflow reports an error when a text line contains an unmatched macro delimiter
    /// </summary>
    [Fact]
    public void Print_Run_UnmatchedMacroDelimiter_ReportsError()
    {
        // Workflow contents — macro start "${{" has no matching "}}"
        const string workflowContents =
            """
            steps:
            - command: print
              inputs:
                text:
                - "${{ unclosed"
            """;

        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify non-zero exit code and error message
            Assert.Equal(1, exitCode);
            Assert.Contains("Unmatched '${{' in variable expansion", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }
}
