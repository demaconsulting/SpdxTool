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
[TestClass]
public class HelpTests
{
    /// <summary>
    ///     Tests the 'help' command with missing arguments
    /// </summary>
    [TestMethod]
    public void Help_MissingArguments()
    {
        // Act: Run the help command with no arguments
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help");

        // Assert: Verify an error was detected
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'help' command missing arguments", output);
    }

    /// <summary>
    ///     Tests the 'help' command with an unknown command
    /// </summary>
    [TestMethod]
    public void Help_UnknownCommand()
    {
        // Act: Run the help command with an unknown command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "unknown-command");

        // Assert: Verify an error was detected
        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unknown command: 'unknown-command'", output);
    }

    /// <summary>
    ///     Tests the 'help' command with the 'run-workflow' command
    /// </summary>
    [TestMethod]
    public void Help_RunWorkflow()
    {
        // Act: Run the help command with an unknown command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "run-workflow");

        // Assert: Verify success
        Assert.AreEqual(0, exitCode);
        Assert.Contains("This command runs the steps specified in the workflow file/url.", output);
    }
}