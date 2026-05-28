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
public class HashTests
{
    /// <summary>
    ///     Test that hash command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void Hash_MissingArguments_ReportsError()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'hash' command missing arguments", output);
    }

    /// <summary>
    ///     Test that hash command with missing file reports an error
    /// </summary>
    [Fact]
    public void Hash_MissingFile_ReportsError()
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
        Assert.Equal(1, exitCode);
        Assert.Contains("Error: Could not find file 'missing-file.txt'", output);
    }

    /// <summary>
    ///     Test that hash command with generate operation creates the sidecar .sha256 file
    /// </summary>
    [Fact]
    public void Hash_GenerateOperation_WritesSidecarFile()
    {
        var testFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var hashFile = testFile + ".sha256";
        try
        {
            // Arrange: Create a test file
            File.WriteAllText(testFile, "The quick brown fox jumps over the lazy dog");

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "generate",
                "sha256",
                testFile);

            // Assert: Verify success reported
            Assert.Equal(0, exitCode);

            // Assert: Verify the hash file was created
            Assert.True(File.Exists(hashFile));
            var digest = File.ReadAllText(hashFile);
            Assert.Equal("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592", digest);
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(hashFile);
        }
    }

    /// <summary>
    ///     Test that hash command verify operation with missing file reports an error
    /// </summary>
    [Fact]
    public void Hash_VerifyMissingFile_ReportsError()
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
        Assert.Equal(1, exitCode);
        Assert.Contains("Error: Could not find file", output);
    }

    /// <summary>
    ///     Test that hash command verify operation fails for invalid hash
    /// </summary>
    [Fact]
    public void Hash_VerifyOperation_FailsForInvalidHash()
    {
        var testFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var hashFile = testFile + ".sha256";
        try
        {
            // Arrange: Create a test file and a hash file with a bad hash
            File.WriteAllText(testFile, "Test string");
            File.WriteAllText(hashFile, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "verify",
                "sha256",
                testFile);

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("Sha256 hash mismatch for '", output);
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(hashFile);
        }
    }

    /// <summary>
    ///     Test that hash command verify operation succeeds for valid hash
    /// </summary>
    [Fact]
    public void Hash_VerifyOperation_SucceedsForValidHash()
    {
        var testFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var hashFile = testFile + ".sha256";
        try
        {
            // Arrange: Create a test file and a hash file with a good hash
            File.WriteAllText(testFile, "The quick brown fox jumps over the lazy dog");
            File.WriteAllText(hashFile, "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "verify",
                "sha256",
                testFile);

            // Assert: Verify success reported
            Assert.Equal(0, exitCode);
            Assert.Contains("Sha256 Digest OK for '", output);
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(hashFile);
        }
    }

    /// <summary>
    ///     Test that hash command with unsupported algorithm reports an error
    /// </summary>
    [Fact]
    public void Hash_UnsupportedAlgorithm_ReportsError()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash",
            "generate",
            "md5",
            "any-file.txt");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'hash' command invalid algorithm 'md5'", output);
    }

    /// <summary>
    ///     Test that hash command with invalid operation reports an error
    /// </summary>
    [Fact]
    public void Hash_InvalidOperation_ReportsError()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash",
            "bad-operation",
            "sha256",
            "any-file.txt");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'hash' command invalid operation 'bad-operation'", output);
    }
}
