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
///     Self-test step that exercises the <c>validate</c> command with both a well-formed and a
///     malformed SPDX document.
/// </summary>
/// <remarks>
///     Verifies that the tool correctly accepts a conformant SPDX document and rejects a malformed
///     one, confirming that basic validation logic functions correctly after installation. Uses a
///     temporary <c>validate.tmp</c> directory in the current working directory; callers must ensure
///     sequential execution to avoid races on that directory and on the process-wide current
///     directory set by <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateBasic
{
    /// <summary>
    ///     Optional test hook invoked after <c>validate.tmp/test-valid.spdx.json</c> is written
    ///     and immediately before <see cref="Validate.RunSpdxTool(string, string[])"/> is called
    ///     in the valid-document sub-test.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts the fixture so that the validate command fails with a non-zero exit code,
    ///     exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the basic SPDX validation self-test and records the result.
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
    /// <exception cref="System.IO.IOException">
    ///     Propagated from <see cref="DoValidate"/> when the temporary directory or files
    ///     cannot be created or deleted.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Propagated from <see cref="DoValidate"/> when the current user lacks write access
    ///     to the working directory.
    /// </exception>
    internal static void Run(Context context, TestResults.TestResults results)
    {
        var passed = Validate.RunInTempDir("validate.tmp", DoValidate);
        Validate.RecordResult(context, results, "SpdxTool_Basic", "DemaConsulting.SpdxTool.SelfTest.ValidateBasic", passed);
    }

    /// <summary>
    ///     Runs both the valid-document and invalid-document sub-tests. Called by
    ///     <see cref="Validate.RunInTempDir"/>, which creates and cleans up the temporary directory.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if both <see cref="DoValidateValid"/> and <see cref="DoValidateInvalid"/>
    ///     succeed; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Uses short-circuit evaluation: <see cref="DoValidateInvalid"/> is not called if
    ///     <see cref="DoValidateValid"/> returns <c>false</c>.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        // Run validation tests for both valid and invalid documents
        return DoValidateValid() && DoValidateInvalid();
    }

    /// <summary>
    ///     Verifies that a well-formed SPDX document is accepted by the <c>validate</c> command.
    /// </summary>
    /// <returns><c>true</c> if RunSpdxTool returns exit code zero; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     Writes a minimal valid SPDX document to <c>validate.tmp/test-valid.spdx.json</c> and
    ///     invokes <see cref="Validate.RunSpdxTool(string, string[])"/> with <c>--silent</c> and
    ///     <c>validate</c> arguments. Expects a zero exit code as evidence that no issues were found.
    ///     Depends on <c>validate.tmp</c> already existing; must be called after
    ///     <see cref="DoValidate"/> creates the directory.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test file cannot be written.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to <c>validate.tmp</c>.</exception>
    private static bool DoValidateValid()
    {
        // Write test SPDX file that is valid
        Validate.WriteTestSpdxJsonMinimal("validate.tmp", "test-valid.spdx.json");

        // Allow tests to corrupt the fixture immediately before the command runs
        PreRunSpdxToolHookForTest?.Invoke();

        // Run validation without NTIA flag on valid document
        var exitCode = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "validate",
                "test-valid.spdx.json"
            ]);

        // Validation should pass for valid document
        return exitCode == 0;
    }

    /// <summary>
    ///     Verifies that a malformed SPDX document is rejected by the <c>validate</c> command.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if RunSpdxTool returns a non-zero exit code and the log contains the expected
    ///     error text referencing the filename; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Writes an SPDX document with a package missing the required SPDXID field to
    ///     <c>validate.tmp/test-invalid.spdx.json</c> and invokes
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> with <c>--silent</c>, <c>--log</c>,
    ///     and <c>validate</c> arguments. Expects a non-zero exit code and verifies that the log file
    ///     contains error text referencing the validation issue. Depends on <c>validate.tmp</c> already
    ///     existing; must be called after <see cref="DoValidate"/> creates the directory.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test file cannot be written or the log file cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to <c>validate.tmp</c>.</exception>
    private static bool DoValidateInvalid()
    {
        // Write test SPDX file that is invalid (missing required SPDXID)
        File.WriteAllText("validate.tmp/test-invalid.spdx.json",
            """
            {
              "files": [],
              "packages": [    {
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "filesAnalyzed": false,
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [],
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

        // Run validation on invalid document
        var exitCode = Validate.RunSpdxTool(
            "validate.tmp",
            [
                "--silent",
                "--log", "output.log",
                "validate",
                "test-invalid.spdx.json"
            ]);

        // Validation should fail for invalid document
        if (exitCode == 0)
        {
            return false;
        }

        // Read the log file to verify error was reported
        var log = File.ReadAllText("validate.tmp/output.log");

        // Verify log references the invalid file, confirming validation ran and reported issues
        return log.Contains("Issues in test-invalid.spdx.json");
    }
}
