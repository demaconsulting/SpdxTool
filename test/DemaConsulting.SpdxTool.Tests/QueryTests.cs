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
///     Tests for the 'query' command
/// </summary>
[TestClass]
public partial class QueryTests
{
    /// <summary>
    ///     Regular expression to check for version
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\d+\.\d+\.\d+")]
    private static partial Regex VersionRegex();

    /// <summary>
    ///     Tests the 'query' command with missing arguments
    /// </summary>
    [TestMethod]
    public void Query_MissingArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'query' command missing arguments", output);
    }

    /// <summary>
    ///     Tests the 'query' command with bad pattern
    /// </summary>
    [TestMethod]
    public void Query_BadPattern()
    {
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
        Assert.AreEqual(1, exitCode);
        Assert.Contains("Pattern must contain a 'value' capture group", output);
    }

    /// <summary>
    ///     Tests the 'query' command with invalid program
    /// </summary>
    [TestMethod]
    public void Query_InvalidProgram()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            @"(?<value>\d+\.\d+\.\d+)",
            "does-not-exist");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unable to start program 'does-not-exist'", output);
    }

    /// <summary>
    ///     Tests the 'query' command for dotnet version from the command line
    /// </summary>
    [TestMethod]
    public void Query_DotNet_CommandLine()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            @"(?<value>\d+\.\d+\.\d+)",
            "dotnet",
            "--version");

        // Assert: Verify error reported
        Assert.AreEqual(0, exitCode);
        Assert.MatchesRegex(VersionRegex(), output);
    }

    /// <summary>
    ///     Tests the 'query' command for dotnet version from a workflow
    /// </summary>
    [TestMethod]
    public void Query_DotNet_Workflow()
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

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);
            Assert.MatchesRegex(VersionRegex(), output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }
}
