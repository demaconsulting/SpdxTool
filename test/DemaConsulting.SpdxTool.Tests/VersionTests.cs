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
///     Tests for version information.
/// </summary>
[TestClass]
public partial class VersionTests
{
    /// <summary>
    ///     Regular expression to check for version
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\d+\.\d+\.\d+.*")]
    private static partial Regex VersionRegex();

    /// <summary>
    ///     Test that version information is printed when '-v' is specified
    /// </summary>
    [TestMethod]
    public void Version_Short()
    {
        // Act: Run the SPDX tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "-v");

        // Assert: Check the output
        Assert.AreEqual(0, exitCode);

        // Assert: Verify version response
        Assert.MatchesRegex(VersionRegex(), output);
    }

    /// <summary>
    ///     Test that version information is printed when '--version' is specified
    /// </summary>
    [TestMethod]
    public void Version_Long()
    {
        // Act: Run the SPDX tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--version");

        // Assert: Check the output
        Assert.AreEqual(0, exitCode);

        // Assert: Verify version response
        Assert.MatchesRegex(VersionRegex(), output);
    }
}