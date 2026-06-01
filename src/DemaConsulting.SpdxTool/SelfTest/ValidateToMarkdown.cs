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
///     Self-test step that exercises the <c>to-markdown</c> command end-to-end.
/// </summary>
/// <remarks>
///     Exercises the to-markdown command end-to-end to confirm it is correctly installed and
///     operational. Called by the SelfTest orchestrator as part of the deployment validation
///     sequence. Uses a temporary <c>validate.tmp</c> directory in the current working directory;
///     callers must ensure sequential execution to avoid races on that directory and on the
///     process-wide current directory set by <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateToMarkdown
{
    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/test.spdx.json</c> so that the to-markdown command fails
    ///     with a non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Runs the to-markdown self-test and records the outcome in the test results collection.
    /// </summary>
    /// <remarks>
    ///     Runs <see cref="DoValidate"/> inside a temporary directory via
    ///     <see cref="Validate.RunInTempDir"/> and records the outcome via
    ///     <see cref="Validate.RecordResult"/>. The test name <c>SpdxTool_ToMarkdown</c> is fixed
    ///     so that ReqStream can trace it to the SpdxTool-SelfTest-ToMarkdown requirement.
    ///     If <see cref="DoValidate"/> throws an exception, the exception propagates uncaught from
    ///     this method and no <see cref="TestResult"/> is recorded for this step.
    /// </remarks>
    /// <param name="context">Active program context used for console output. Must not be null.</param>
    /// <param name="results">Test results collection to append the outcome to. Must not be null.</param>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = Validate.RunInTempDir("validate.tmp", DoValidate);
        Validate.RecordResult(context, results, "SpdxTool_ToMarkdown", "DemaConsulting.SpdxTool.SelfTest.ValidateToMarkdown", passed);
    }

    /// <summary>
    ///     Performs the to-markdown validation. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <remarks>
    ///     Creates a two-package SPDX JSON document (Test Application 1.0.0/MIT and Test Library
    ///     2.0.0/Apache-2.0) with DESCRIBES and CONTAINS relationships, invokes the to-markdown
    ///     command via Validate.RunSpdxTool, then verifies that the output Markdown file contains
    ///     the expected title, section headings, package names, and version strings.
    /// </remarks>
    /// <returns>
    ///     True if the to-markdown command exits with code zero and the output Markdown file
    ///     contains all expected strings; false if the exit code is non-zero or any expected
    ///     string is absent.
    /// </returns>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or the Markdown file cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        const string tempDir = "validate.tmp";

        // Write test SPDX file with packages and relationships
        File.WriteAllText($"{tempDir}/test-markdown.spdx.json",
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Application",
                  "name": "Test Application",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Library",
                  "name": "Test Library",
                  "versionInfo": "2.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "Apache-2.0"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "DESCRIBES"
                },
                {
                  "spdxElementId": "SPDXRef-Application",
                  "relatedSpdxElement": "SPDXRef-Library",
                  "relationshipType": "CONTAINS"
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

        // Allow tests to corrupt fixtures immediately before the command runs
        PreRunSpdxToolHookForTest?.Invoke();

        // Run the to-markdown command to generate Markdown summary
        var exitCode = Validate.RunSpdxTool(
            tempDir,
            [
                "--silent",
                "to-markdown",
                "test-markdown.spdx.json",
                "test-markdown.md",
                "Test SBOM Summary"
            ]);

        // Fail if SpdxTool reported an error
        if (exitCode != 0)
        {
            return false;
        }

        // Verify the Markdown file was created
        if (!File.Exists($"{tempDir}/test-markdown.md"))
        {
            return false;
        }

        // Read the generated Markdown content
        var markdown = File.ReadAllText($"{tempDir}/test-markdown.md");

        // Verify Markdown contains expected structure and package information
        return markdown.Contains("Test SBOM Summary") &&
               markdown.Contains("Root Packages") &&
               markdown.Contains("### Packages") &&
               markdown.Contains("Test Application") &&
               markdown.Contains("1.0.0") &&
               markdown.Contains("Test Library") &&
               markdown.Contains("2.0.0");
    }
}
