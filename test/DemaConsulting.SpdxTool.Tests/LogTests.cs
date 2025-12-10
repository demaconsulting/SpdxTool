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
[TestClass]
public class LogTests
{
    /// <summary>
    ///     Test that logging functions when '-l' is specified
    /// </summary>
    [TestMethod]
    public void Log_Short()
    {
        try
        {
            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "-l", "output.log",
                "-h");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Assert: Verify log file written
            Assert.IsTrue(File.Exists("output.log"));

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
    ///     Test that logging functions when '--log' is specified
    /// </summary>
    [TestMethod]
    public void Log_Long()
    {
        try
        {
            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "--log", "output.log",
                "--help");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Assert: Verify log file written
            Assert.IsTrue(File.Exists("output.log"));

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
}