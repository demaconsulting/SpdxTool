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

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'query' command
/// </summary>
public partial class QueryTests
{
    /// <summary>
    ///     Regular expression to check for version
    /// </summary>
    /// <returns>Compiled regular expression matching a three-part version number (e.g. 8.0.1).</returns>
    [GeneratedRegex(@"\d+\.\d+\.\d+")]
    private static partial Regex VersionRegex();

    /// <summary>
    ///     Test that query command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void Query_Run_MissingArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'query' command missing arguments", output);
    }

    /// <summary>
    ///     Test that query command with bad regex pattern reports an error
    /// </summary>
    [Fact]
    public void Query_Run_PatternMissingValueGroup_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            "pattern",
            "dotnet",
            "--version");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("Pattern must contain a 'value' capture group", output);
    }

    /// <summary>
    ///     Test that query command with invalid program reports an error
    /// </summary>
    [Fact]
    public void Query_Run_InvalidProgram_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            @"(?<value>\d+\.\d+\.\d+)",
            "does-not-exist");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("Unable to start program 'does-not-exist'", output);
    }

    /// <summary>
    ///     Test that query command for dotnet version on command line returns the version
    /// </summary>
    [Fact]
    public void Query_Run_DotNetVersionOnCommandLine_ReturnsVersion()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            @"(?<value>\d+\.\d+\.\d+)",
            "dotnet",
            "--version");

        // Assert: Verify version returned
        Assert.Equal(0, exitCode);
        Assert.Matches(VersionRegex(), output);
    }

    /// <summary>
    ///     Test that query command for dotnet version in workflow stores the version
    /// </summary>
    [Fact]
    public void Query_Run_DotNetVersionInWorkflow_StoresVersion()
    {
        // Workflow contents
        const string workflowContents =
            """
            parameters:
              version: unknown

            steps:
            - command: query
              inputs:
                output: version
                pattern: (?<value>\d+\.\d+\.\d+)
                program: dotnet
                arguments:
                - '--version'

            - command: print
              inputs:
                text:
                - ${{ version }}
            """;

        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Arrange: Write the workflow file to a temp location
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
            Assert.Matches(VersionRegex(), output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that query command with an invalid regex pattern reports an error
    /// </summary>
    [Fact]
    public void Query_Run_InvalidRegexPattern_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            "[unclosed",
            "dotnet",
            "--version");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("Invalid regular expression pattern", output);
    }

    /// <summary>
    ///     Test that query command with a pattern not found in output reports an error
    /// </summary>
    [Fact]
    public void Query_Run_PatternNotFound_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            @"(?<value>THIS_WILL_NEVER_MATCH)",
            "dotnet",
            "--version");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("not found in program output", output);
    }
}
