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

using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.SelfTest;

/// <summary>
///     Self-test step that exercises the <c>hash</c> command end-to-end.
/// </summary>
/// <remarks>
///     Verifies that a SHA-256 hash file can be generated for a known file and that the generated
///     hash file can subsequently be verified, confirming both the <c>generate</c> and
///     <c>verify</c> sub-commands function correctly. Uses a temporary <c>validate.tmp</c>
///     directory in the current working directory; callers must ensure sequential execution to
///     avoid races on that directory and on the process-wide current directory set by
///     <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateHash
{
    /// <summary>
    ///     Executes the hash self-test and records the result.
    /// </summary>
    /// <param name="context">The active Program context providing output and error streams.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <remarks>
    ///     Runs <see cref="DoValidate"/> inside a temporary directory via
    ///     <see cref="Validate.RunInTempDir"/> and records the outcome via
    ///     <see cref="Validate.RecordResult"/>. If <see cref="DoValidate"/> throws an exception,
    ///     the exception propagates uncaught from this method and no <see cref="TestResult"/> is
    ///     recorded for this step.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = Validate.RunInTempDir(Validate.TempDir, DoValidate);
        Validate.RecordResult(context, results, "SpdxTool_Hash", "DemaConsulting.SpdxTool.SelfTest.ValidateHash", passed);
    }

    /// <summary>
    ///     Orchestrates the generate and verify sub-tests. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if both <see cref="DoValidateGenerate"/> and <see cref="DoValidateVerify"/>
    ///     return <c>true</c>; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Uses short-circuit evaluation: <see cref="DoValidateVerify"/> is not called if
    ///     <see cref="DoValidateGenerate"/> returns <c>false</c>.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if any test file cannot be created, read, or deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        // Run both generation and verification validation tests
        return DoValidateGenerate() && DoValidateVerify();
    }

    /// <summary>
    ///     Verifies that the <c>hash generate</c> sub-command produces the correct SHA-256 hash for a
    ///     known input file.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if RunSpdxTool returns exit code zero, the <c>.sha256</c> file is created,
    ///     and the hash value matches the known expected digest; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Writes a test file containing "The quick brown fox jumps over the lazy dog" to
    ///     <c>validate.tmp/test-file.txt</c>, calls
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> with <c>hash generate sha256</c>
    ///     arguments, verifies the generated hash file exists, and checks that the file content
    ///     equals the known SHA-256 digest for that string.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test file or hash file cannot be written or read.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidateGenerate()
    {
        // Write test file with known content
        File.WriteAllText($"{Validate.TempDir}/test-file.txt", "The quick brown fox jumps over the lazy dog");

        // Run hash generate command to create SHA256 hash
        var exitCode = Validate.RunSpdxTool(
            Validate.TempDir,
            [
                "--silent",
                "hash",
                "generate",
                "sha256",
                "test-file.txt"
            ]);

        // Fail if SpdxTool reported an error
        if (exitCode != 0)
        {
            return false;
        }

        // Verify hash file was created with expected naming
        if (!File.Exists($"{Validate.TempDir}/test-file.txt.sha256"))
        {
            return false;
        }

        // Read the generated hash value
        var hash = File.ReadAllText($"{Validate.TempDir}/test-file.txt.sha256");

        // Verify hash matches expected SHA256 value for the test content
        return hash == "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592";
    }

    /// <summary>
    ///     Verifies that the <c>hash verify</c> sub-command accepts a correct hash and rejects a
    ///     corrupted one.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if verification with the correct hash returns exit code zero and
    ///     verification with a corrupted hash returns a non-zero exit code; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Relies on <c>validate.tmp/test-file.txt</c> and <c>validate.tmp/test-file.txt.sha256</c>
    ///     having been created by <see cref="DoValidateGenerate"/>. Calls
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> twice: first with the correct hash
    ///     (expects exit code zero), then after overwriting the hash file with all-zero digits
    ///     (expects a non-zero exit code).
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the hash file cannot be overwritten.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidateVerify()
    {
        // Run hash verify command with correct hash
        var exitCode1 = Validate.RunSpdxTool(
            Validate.TempDir,
            [
                "--silent",
                "hash",
                "verify",
                "sha256",
                "test-file.txt"
            ]);

        // Verification should succeed with correct hash
        if (exitCode1 != 0)
        {
            return false;
        }

        // Corrupt the hash file with invalid hash value
        File.WriteAllText($"{Validate.TempDir}/test-file.txt.sha256", "0000000000000000000000000000000000000000000000000000000000000000");

        // Run hash verify command with incorrect hash
        var exitCode2 = Validate.RunSpdxTool(
            Validate.TempDir,
            [
                "--silent",
                "hash",
                "verify",
                "sha256",
                "test-file.txt"
            ]);

        // Verification should fail with incorrect hash
        return exitCode2 != 0;
    }
}
