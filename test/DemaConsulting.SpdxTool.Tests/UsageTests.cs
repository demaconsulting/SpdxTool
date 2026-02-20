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
///     Tests for usage information.
/// </summary>
[TestClass]
public class UsageTests
{
    /// <summary>
    ///     Test that running with no arguments displays an error message
    /// </summary>
    [TestMethod]
    public void Usage_NoArguments_DisplaysError()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll");

        // Assert: Verify an error was reported
        Assert.AreEqual(1, exitCode);

        // Assert: Verify the output contains the usage information
        Assert.Contains("Error: Missing arguments", output);
        Assert.Contains("Usage: spdx-tool", output);
    }

    /// <summary>
    ///     Test that the short help flag displays usage information
    /// </summary>
    [TestMethod]
    public void Usage_ShortHelpFlag_DisplaysUsage()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "-h");

        // Assert: Verify success
        Assert.AreEqual(0, exitCode);

        // Assert: Verify the output contains the usage information
        Assert.Contains("Usage: spdx-tool", output);
    }

    /// <summary>
    ///     Test that the long help flag displays usage information
    /// </summary>
    [TestMethod]
    public void Usage_LongHelpFlag_DisplaysUsage()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--help");

        // Assert: Verify success
        Assert.AreEqual(0, exitCode);

        // Assert: Verify the output contains the usage information
        Assert.Contains("Usage: spdx-tool", output);
    }
}
