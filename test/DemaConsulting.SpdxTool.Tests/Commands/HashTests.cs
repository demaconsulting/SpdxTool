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

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'hash' command
/// </summary>
public class HashTests
{
    /// <summary>
    ///     Test that hash command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void Hash_Run_MissingArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'hash' command requires exactly 3 arguments", output);
    }

    /// <summary>
    ///     Test that hash command with excess arguments reports an error
    /// </summary>
    [Fact]
    public void Hash_Run_ExcessArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command with four arguments instead of the required three
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "hash",
            "generate",
            "sha256",
            "some-file.txt",
            "extra-argument");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'hash' command requires exactly 3 arguments", output);
    }

    /// <summary>
    ///     Test that hash command with missing file reports an error
    /// </summary>
    [Fact]
    public void Hash_Run_MissingFile_ReportsError()
    {
        // Arrange: no setup required

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
    public void Hash_Run_GenerateOperation_WritesSidecarFile()
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
    ///     Test that hash command verify operation with missing sidecar file reports an error
    /// </summary>
    [Fact]
    public void Hash_Run_VerifyMissingSidecarFile_ReportsError()
    {
        // Arrange: no setup required

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
    ///     Test that hash command verify operation with a missing target file reports an error
    /// </summary>
    [Fact]
    public void Hash_Run_VerifyTargetMissing_ReportsError()
    {
        var targetFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var hashFile = targetFile + ".sha256";
        try
        {
            // Arrange: Create the sidecar file but NOT the target file
            File.WriteAllText(hashFile, "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "verify",
                "sha256",
                targetFile);

            // Assert: Verify error reported for missing target
            Assert.Equal(1, exitCode);
            Assert.Contains($"Error: Could not find file '{targetFile}'", output);
        }
        finally
        {
            File.Delete(hashFile);
        }
    }

    /// <summary>
    ///     Test that hash command verify operation fails for invalid hash
    /// </summary>
    [Fact]
    public void Hash_Run_VerifyOperation_FailsForInvalidHash()
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
    public void Hash_Run_VerifyOperation_SucceedsForValidHash()
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
    ///     Test that hash command verify operation succeeds when the sidecar file contains an uppercase digest
    /// </summary>
    [Fact]
    public void Hash_Run_VerifySha256_UppercaseDigest_Succeeds()
    {
        var testFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var hashFile = testFile + ".sha256";
        try
        {
            // Arrange: Create a test file and a sidecar with an uppercase digest (as produced by some external tools)
            File.WriteAllText(testFile, "The quick brown fox jumps over the lazy dog");
            File.WriteAllText(hashFile, "D7A8FBB307D7809469CA9ABCB0082E4F8D5651E46D3CDB762D02D0BF37C9E592");

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "hash",
                "verify",
                "sha256",
                testFile);

            // Assert: Verify success reported despite uppercase digest in sidecar file
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
    public void Hash_Run_UnsupportedAlgorithm_ReportsError()
    {
        // Arrange: no setup required

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
    public void Hash_Run_InvalidOperation_ReportsError()
    {
        // Arrange: no setup required

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

    /// <summary>
    ///     Test that hash command in workflow generates a hash sidecar file
    /// </summary>
    [Fact]
    public void Hash_Run_InWorkflow_GeneratesHash()
    {
        var testFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var hashFile = testFile + ".sha256";
        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var workflowContents =
            $"""
            steps:
            - command: hash
              inputs:
                operation: generate
                algorithm: sha256
                file: {testFile}
            """;

        try
        {
            // Arrange: Create the test file and workflow
            File.WriteAllText(testFile, "The quick brown fox jumps over the lazy dog");
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify success and sidecar file was written
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(hashFile));
            var digest = File.ReadAllText(hashFile);
            Assert.Equal("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592", digest);
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(hashFile);
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that hash command in workflow with missing 'operation' input reports an error
    /// </summary>
    [Fact]
    public void Hash_Run_InWorkflow_MissingOperation_ReportsError()
    {
        var testFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var workflowContents =
            $"""
            steps:
            - command: hash
              inputs:
                algorithm: sha256
                file: {testFile}
            """;

        try
        {
            // Arrange: Create the test file and workflow (operation input is omitted)
            File.WriteAllText(testFile, "test content");
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify error reported for missing operation
            Assert.Equal(1, exitCode);
            Assert.Contains("'hash' command missing 'operation' input", output);
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that hash command in workflow with missing 'algorithm' input reports an error
    /// </summary>
    [Fact]
    public void Hash_Run_InWorkflow_MissingAlgorithm_ReportsError()
    {
        var testFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var workflowContents =
            $"""
            steps:
            - command: hash
              inputs:
                operation: generate
                file: {testFile}
            """;

        try
        {
            // Arrange: Create the test file and workflow (algorithm input is omitted)
            File.WriteAllText(testFile, "test content");
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify error reported for missing algorithm
            Assert.Equal(1, exitCode);
            Assert.Contains("'hash' command missing 'algorithm' input", output);
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(workflowFile);
        }
    }

    /// <summary>
    ///     Test that hash command in workflow with missing 'file' input reports an error
    /// </summary>
    [Fact]
    public void Hash_Run_InWorkflow_MissingFile_ReportsError()
    {
        var workflowFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        const string workflowContents =
            """
            steps:
            - command: hash
              inputs:
                operation: generate
                algorithm: sha256
            """;

        try
        {
            // Arrange: Write the workflow (file input is omitted)
            File.WriteAllText(workflowFile, workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                workflowFile);

            // Assert: Verify error reported for missing file
            Assert.Equal(1, exitCode);
            Assert.Contains("'hash' command missing 'file' input", output);
        }
        finally
        {
            File.Delete(workflowFile);
        }
    }
}
