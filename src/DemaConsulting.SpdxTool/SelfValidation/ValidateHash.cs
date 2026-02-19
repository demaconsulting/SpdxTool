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

namespace DemaConsulting.SpdxTool.SelfValidation;

/// <summary>
///     Self-validation of Hash command
/// </summary>
internal static class ValidateHash
{
    /// <summary>
    ///     Run validation test
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="results">Test results</param>
    public static void Run(Context context, TestResults.TestResults results)
    {
        // Perform the validation
        var passed = DoValidate();

        // Report validation result to console
        context.WriteLine($"- SpdxTool_Hash: {(passed ? "Passed" : "Failed")}");
        
        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_Hash",
                ClassName = "DemaConsulting.SpdxTool.SelfValidation.ValidateHash",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Do the validation
    /// </summary>
    /// <returns>True on success</returns>
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
            // Delete the temporary validation folder
            Directory.Delete("validate.tmp", true);
        }
    }

    /// <summary>
    ///     Validate hash generation
    /// </summary>
    /// <returns>True on success</returns>
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
    ///     Validate hash verification
    /// </summary>
    /// <returns>True on success</returns>
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
