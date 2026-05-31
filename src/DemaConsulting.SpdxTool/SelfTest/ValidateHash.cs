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
    ///     Calls <see cref="DoValidate"/> and records a <see cref="TestResult"/> named
    ///     <c>SpdxTool_Hash</c> with <see cref="TestOutcome.Passed"/> or
    ///     <see cref="TestOutcome.Failed"/> depending on the return value. If <see cref="DoValidate"/>
    ///     throws an exception, the exception propagates uncaught from this method and no
    ///     <see cref="TestResult"/> is recorded for this step.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        // Capture start time before validation begins so the recorded StartTime
        // reflects when Run was entered, not when DoValidate returned
        var startTime = DateTime.Now;

        // Perform the validation
        var passed = DoValidate();

        // Report validation result
        if (passed)
        {
            context.WriteLine("✓ SpdxTool_Hash - Passed");
        }
        else
        {
            context.WriteError("✗ SpdxTool_Hash - Failed");
        }

        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_Hash",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateHash",
                ComputerName = Environment.MachineName,
                StartTime = startTime,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Orchestrates the generate and verify sub-tests in a shared temporary directory, returning
    ///     true only if both succeed.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if both <see cref="DoValidateGenerate"/> and <see cref="DoValidateVerify"/>
    ///     return <c>true</c>; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Creates <c>validate.tmp</c>, delegates to <see cref="DoValidateGenerate"/> and
    ///     <see cref="DoValidateVerify"/> in sequence (using short-circuit evaluation), then
    ///     deletes the temporary directory in a <c>finally</c> block only if it exists, guarding against
    ///     a secondary <see cref="DirectoryNotFoundException"/> when <see cref="Directory.CreateDirectory(string)"/> fails.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the temporary directory or any test file cannot be created, read, or deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        try
        {
            // Create the temporary validation folder
            Directory.CreateDirectory("validate.tmp");

            // Run both generation and verification validation tests
            return DoValidateGenerate() && DoValidateVerify();
        }
        finally
        {
            // Delete the temporary validation folder if it exists (guards against
            // Directory.CreateDirectory failing before the directory was created)
            if (Directory.Exists("validate.tmp"))
            {
                Directory.Delete("validate.tmp", true);
            }
        }
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
        File.WriteAllText("validate.tmp/test-file.txt", "The quick brown fox jumps over the lazy dog");

        // Run hash generate command to create SHA256 hash
        var exitCode = Validate.RunSpdxTool(
            "validate.tmp",
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
        if (!File.Exists("validate.tmp/test-file.txt.sha256"))
        {
            return false;
        }

        // Read the generated hash value
        var hash = File.ReadAllText("validate.tmp/test-file.txt.sha256");

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
            "validate.tmp",
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
        File.WriteAllText("validate.tmp/test-file.txt.sha256", "0000000000000000000000000000000000000000000000000000000000000000");

        // Run hash verify command with incorrect hash
        var exitCode2 = Validate.RunSpdxTool(
            "validate.tmp",
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
