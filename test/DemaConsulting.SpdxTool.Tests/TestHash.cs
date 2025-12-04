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
///     Tests for the 'hash' command
/// </summary>
[TestClass]
public class TestHash
{
    /// <summary>
    ///     Tests the 'hash' command with missing arguments
    /// </summary>
    [TestMethod]
    public void HashCommand_MissingArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'hash' command missing arguments", output);
    }

    /// <summary>
    ///     Tests the 'hash' command with missing file
    /// </summary>
    [TestMethod]
    public void HashCommand_MissingFile()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash",
            "generate",
            "sha256",
            "missing-file.txt");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("Error: Could not find file 'missing-file.txt'", output);
    }

    /// <summary>
    ///     Tests the 'hash' command to generate a hash file
    /// </summary>
    [TestMethod]
    public void HashCommand_Generate()
    {
        try
        {
            // Arrange: Create a test file
            File.WriteAllText("test.txt", "The quick brown fox jumps over the lazy dog");

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "generate",
                "sha256",
                "test.txt");

            // Assert: Verify success reported
            Assert.AreEqual(0, exitCode);

            // Assert: Verify the hash file was created
            Assert.IsTrue(File.Exists("test.txt.sha256"));
            var digest = File.ReadAllText("test.txt.sha256");
            Assert.AreEqual("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592", digest);
        }
        finally
        {
            File.Delete("test.txt");
            File.Delete("test.txt.sha256");
        }
    }

    /// <summary>
    ///     Tests the 'hash' command to verify a hash file with a missing file
    /// </summary>
    [TestMethod]
    public void HashCommand_Verify_MissingFile()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash",
            "verify",
            "sha256",
            "missing-file.txt");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("Error: Could not find file", output);
    }

    /// <summary>
    ///     Tests the 'hash' command to verify a hash file with a bad hash
    /// </summary>
    [TestMethod]
    public void HashCommand_Verify_Bad()
    {
        try
        {
            // Arrange: Create a test file and a hash file with a bad hash
            File.WriteAllText("test.txt", "Test string");
            File.WriteAllText("test.txt.sha256", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "verify",
                "sha256",
                "test.txt");

            // Assert: Verify error reported
            Assert.AreEqual(1, exitCode);
            Assert.Contains("Sha256 hash mismatch for 'test.txt'", output);
        }
        finally
        {
            File.Delete("test.txt");
            File.Delete("test.txt.sha256");
        }
    }

    /// <summary>
    ///     Tests the 'hash' command to verify a hash file with a good hash
    /// </summary>
    [TestMethod]
    public void HashCommand_Verify_Good()
    {
        try
        {
            // Arrange: Create a test file and a hash file with a good hash
            File.WriteAllText("test.txt", "The quick brown fox jumps over the lazy dog");
            File.WriteAllText("test.txt.sha256", "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "verify",
                "sha256",
                "test.txt");

            // Assert: Verify success reported
            Assert.AreEqual(0, exitCode);
            Assert.Contains("Sha256 Digest OK for 'test.txt'", output);
        }
        finally
        {
            File.Delete("test.txt");
            File.Delete("test.txt.sha256");
        }
    }
}