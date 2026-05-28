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
///     Tests for logging output.
/// </summary>
public class LogTests
{
    /// <summary>
    ///     Test that the short log flag writes output to a file
    /// </summary>
    [Fact]
    public void SpdxTool_Log_ShortFlag_WritesOutputToFile()
    {
        try
        {
            // Arrange: no setup required

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "-l", "output.log",
                "-h");

            // Assert: Verify success
            Assert.Equal(0, exitCode);

            // Assert: Verify log file written
            Assert.True(File.Exists("output.log"));

            // Assert: Verify the log contains the usage information
            var log = File.ReadAllText("output.log");
            Assert.Contains("Usage: spdx-tool", log);
        }
        finally
        {
            // Delete output file
            File.Delete("output.log");
        }
    }

    /// <summary>
    ///     Test that the long log flag writes output to a file
    /// </summary>
    [Fact]
    public void SpdxTool_Log_LongFlag_WritesOutputToFile()
    {
        try
        {
            // Arrange: no setup required

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "--log", "output.log",
                "--help");

            // Assert: Verify success
            Assert.Equal(0, exitCode);

            // Assert: Verify log file written
            Assert.True(File.Exists("output.log"));

            // Assert: Verify the log contains the usage information
            var log = File.ReadAllText("output.log");
            Assert.Contains("Usage: spdx-tool", log);
        }
        finally
        {
            // Delete output file
            File.Delete("output.log");
        }
    }

    /// <summary>
    ///     Test that --silent --log combination writes to log but suppresses console output
    /// </summary>
    [Fact]
    public void SpdxTool_Log_SilentFlag_WritesToLogButNotConsole()
    {
        try
        {
            // Arrange: no setup required

            // Act: Run the command with --silent and --log
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "--silent",
                "-l", "output-silent.log",
                "-h");

            // Assert: Verify success
            Assert.Equal(0, exitCode);

            // Assert: Verify console output was suppressed
            Assert.Empty(output.Trim());

            // Assert: Verify log file was written
            Assert.True(File.Exists("output-silent.log"));

            // Assert: Verify the log contains the usage information
            var log = File.ReadAllText("output-silent.log");
            Assert.Contains("Usage: spdx-tool", log);
        }
        finally
        {
            // Delete output file
            File.Delete("output-silent.log");
        }
    }
}
