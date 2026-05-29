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
public partial class VersionTests
{
    /// <summary>
    ///     Regular expression to check for version
    /// </summary>
    /// <returns>A compiled Regex that matches a semantic version string (e.g., 1.2.3 or 1.2.3-preview).</returns>
    [GeneratedRegex(@"\d+\.\d+\.\d+.*")]
    private static partial Regex VersionRegex();

    /// <summary>
    ///     Test that the short version flag displays the version information
    /// </summary>
    [Fact]
    public void SpdxTool_Version_ShortFlag_DisplaysVersion()
    {
        // Arrange: no setup required

        // Act: Run the SPDX tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "-v");

        // Assert: Check the output
        Assert.Equal(0, exitCode);

        // Assert: Verify version response
        Assert.Matches(VersionRegex(), output);
    }

    /// <summary>
    ///     Test that the long version flag displays the version information
    /// </summary>
    [Fact]
    public void SpdxTool_Version_LongFlag_DisplaysVersion()
    {
        // Arrange: no setup required

        // Act: Run the SPDX tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--version");

        // Assert: Check the output
        Assert.Equal(0, exitCode);

        // Assert: Verify version response
        Assert.Matches(VersionRegex(), output);
    }
}
