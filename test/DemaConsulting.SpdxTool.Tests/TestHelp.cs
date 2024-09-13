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

[TestClass]
public class TestHelp
{
    [TestMethod]
    public void HelpMissingArguments()
    {
        // Run the help command with no arguments
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help");

        // Verify an error was detected
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "'help' command missing arguments");
    }

    [TestMethod]
    public void HelpUnknownCommand()
    {
        // Run the help command with an unknown command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "unknown-command");

        // Verify an error was detected
        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(output, "Unknown command: 'unknown-command'");
    }

    [TestMethod]
    public void HelpRunWorkflow()
    {
        // Run the help command with an unknown command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "run-workflow");

        // Verify success
        Assert.AreEqual(0, exitCode);
        StringAssert.Contains(output, "This command runs the steps specified in the workflow file/url.");
    }
}