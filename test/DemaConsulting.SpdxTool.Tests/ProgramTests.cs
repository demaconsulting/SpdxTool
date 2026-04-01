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
///     Unit tests for the Program class.
/// </summary>
[TestClass]
public partial class ProgramTests
{
    /// <summary>
    ///     Regular expression to match a semantic version string.
    /// </summary>
    [GeneratedRegex(@"\d+\.\d+\.\d+.*")]
    private static partial Regex VersionRegex();

    /// <summary>
    ///     Test that Program.Version is a valid version string.
    /// </summary>
    [TestMethod]
    public void Program_Version_IsValidVersionString()
    {
        // Act
        var version = Program.Version;

        // Assert
        Assert.IsNotNull(version);
        Assert.IsFalse(string.IsNullOrWhiteSpace(version));
        Assert.MatchesRegex(VersionRegex(), version);
    }

    /// <summary>
    ///     Test that Program.Run with version context writes version to output.
    /// </summary>
    [TestMethod]
    public void Program_Run_VersionContext_WritesVersion()
    {
        // Arrange
        using var context = Context.Create(["-v"]);

        // Act: capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            Program.Run(context);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        // Assert
        Assert.AreEqual(0, context.ExitCode);
        Assert.MatchesRegex(VersionRegex(), writer.ToString());
    }

    /// <summary>
    ///     Test that Program.Run with help context writes usage to output.
    /// </summary>
    [TestMethod]
    public void Program_Run_HelpContext_WritesUsage()
    {
        // Arrange
        using var context = Context.Create(["--help"]);

        // Act: capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            Program.Run(context);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        // Assert
        Assert.AreEqual(0, context.ExitCode);
        Assert.Contains("Usage: spdx-tool", writer.ToString());
    }

    /// <summary>
    ///     Test that Program.Run with no arguments writes error and usage.
    /// </summary>
    [TestMethod]
    public void Program_Run_NoArguments_WritesErrorAndUsage()
    {
        // Arrange
        using var context = Context.Create([]);

        // Act: capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            Program.Run(context);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        // Assert
        Assert.AreEqual(1, context.ExitCode);
        var output = writer.ToString();
        Assert.Contains("Error: Missing arguments", output);
        Assert.Contains("Usage: spdx-tool", output);
    }
}
