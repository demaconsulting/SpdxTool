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
///     Self-test step that exercises the <c>validate</c> command with NTIA minimum-elements checking.
/// </summary>
/// <remarks>
///     Verifies that the tool correctly rejects a non-compliant SPDX document (missing the supplier
///     field) and accepts a fully compliant one when the <c>ntia</c> flag is supplied, confirming
///     that NTIA minimum-elements enforcement functions correctly after installation. Uses a temporary
///     <c>validate.tmp</c> directory in the current working directory; callers must ensure sequential
///     execution to avoid races on that directory and on the process-wide current directory set by
///     <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateNtia
{
    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called for the first time.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/test-ntia.spdx.json</c> so that the validate command fails
    ///     with a non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the NTIA validation self-test and records the result.
    /// </summary>
    /// <remarks>
    ///     Runs <see cref="DoValidate"/> inside a temporary directory via
    ///     <see cref="Validate.RunInTempDir"/> and records the outcome via
    ///     <see cref="Validate.RecordResult"/>. If <see cref="DoValidate"/> throws an exception,
    ///     the exception propagates uncaught from this method and no <see cref="TestResult"/> is
    ///     recorded for this step.
    /// </remarks>
    /// <param name="context">The active Program context providing output and error streams. Must not be null.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = Validate.RunInTempDir("validate.tmp", DoValidate);
        Validate.RecordResult(context, results, "SpdxTool_Ntia", "DemaConsulting.SpdxTool.SelfTest.ValidateNtia", passed);
    }

    /// <summary>
    ///     Runs both NTIA sub-tests. Called by <see cref="Validate.RunInTempDir"/>, which creates
    ///     and cleans up the temporary directory.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if both <see cref="DoValidateMissingSupplier"/> and
    ///     <see cref="DoValidateCompliant"/> succeed; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Uses short-circuit evaluation: <see cref="DoValidateCompliant"/> is not called if
    ///     <see cref="DoValidateMissingSupplier"/> returns <c>false</c>.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        // Run individual validation tests
        return DoValidateMissingSupplier() && DoValidateCompliant();
    }

    /// <summary>
    ///     Verifies that NTIA validation correctly rejects a document missing the supplier field.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if basic validation passes (exit code zero) and NTIA validation fails
    ///     (non-zero exit code) with the expected error in the log; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Runs <see cref="Validate.RunSpdxTool(string, string[])"/> twice on the same document:
    ///     once without the <c>ntia</c> flag (expects exit code zero, confirming the document is
    ///     otherwise valid) and once with the <c>ntia</c> flag (expects non-zero exit code).
    ///     Also confirms the log contains the specific "Missing Supplier" error text so that callers
    ///     know the correct code path was exercised. Depends on <c>validate.tmp</c> already existing;
    ///     must be called after <see cref="DoValidate"/> creates the directory.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the temporary files cannot be created or read.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidateMissingSupplier()
    {
        // Write a valid but non-NTIA-compliant SPDX file (missing supplier field)
        Validate.WriteTestSpdxJsonMinimal("validate.tmp", "test-ntia.spdx.json");

        // Allow tests to corrupt fixtures immediately before the command runs
        PreRunSpdxToolHookForTest?.Invoke();

        // Run validation without NTIA flag - should succeed
        var exitCode1 = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "validate",
                "test-ntia.spdx.json"
            ]);

        // Fail if SpdxTool reported an error
        if (exitCode1 != 0)
        {
            return false;
        }

        // Run validation with NTIA flag - should fail due to missing supplier
        // The log file will be written to validate.tmp/output.log since the working directory is changed
        var exitCode2 = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "--log", "output.log",
                "validate",
                "test-ntia.spdx.json",
                "ntia"
            ]);

        // Should fail validation
        if (exitCode2 == 0)
        {
            return false;
        }

        // Fail if log file is absent
        if (!File.Exists("validate.tmp/output.log"))
        {
            return false;
        }

        // Read the log file and verify it contains the expected error
        var log = File.ReadAllText("validate.tmp/output.log");
        if (!log.Contains("NTIA: Package 'Test Package' Missing Supplier"))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Verifies that NTIA validation accepts a document that includes all required minimum elements.
    /// </summary>
    /// <returns><c>true</c> if <see cref="Validate.RunSpdxTool(string, string[])"/> returns exit code zero; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     Writes an SPDX document with the supplier field set and invokes
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> with the <c>ntia</c> flag. A zero
    ///     exit code confirms the validate command does not produce false positives on a compliant
    ///     document. Depends on <c>validate.tmp</c> already existing; must be called after
    ///     <see cref="DoValidate"/> creates the directory.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the temporary files cannot be created or read.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidateCompliant()
    {
        // Write test SPDX file that is NTIA compliant
        File.WriteAllText("validate.tmp/test-ntia-valid.spdx.json",
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "supplier": "Organization: Test",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "filesAnalyzed": false,
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [
                {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package",
                  "relationshipType": "DESCRIBES"
                }
              ],
              "spdxVersion": "SPDX-2.2",
              "dataLicense": "CC0-1.0",
              "SPDXID": "SPDXRef-DOCUMENT",
              "name": "Test Document",
              "documentNamespace": "https://sbom.spdx.org",
              "creationInfo": {
                "created": "2021-10-01T00:00:00Z",
                "creators": [ "Person: Malcolm Nixon" ]
              }
            }
            """);

        // Run validation with NTIA flag on valid document - should succeed
        var exitCode = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "validate",
                "test-ntia-valid.spdx.json",
                "ntia"
            ]);

        // Should pass validation
        return exitCode == 0;
    }
}
