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
///     Self-test step that exercises the <c>get-version</c> command end-to-end.
/// </summary>
/// <remarks>
///     Verifies that a package version can be retrieved from an SPDX document by package ID and
///     captured into a workflow variable that is subsequently printed to the log output. Uses a
///     temporary <c>validate.tmp</c> directory in the current working directory; callers must
///     ensure sequential execution to avoid races on that directory and on the process-wide
///     current directory set by <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateGetVersion
{
    /// <summary>
    ///     Executes the get-version self-test and records the result.
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
        Validate.RecordResult(context, results, "SpdxTool_GetVersion", "DemaConsulting.SpdxTool.SelfTest.ValidateGetVersion", passed);
    }

    /// <summary>
    ///     Performs the actual get-version validation. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the command succeeded and the log contains the expected version string;
    ///     otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Writes an SPDX JSON document containing two packages where SPDXRef-Package-2 has version
    ///     "2.0.0", and a workflow YAML that executes <c>get-version</c> to retrieve the version of
    ///     SPDXRef-Package-2 into the <c>version</c> variable and prints it via the <c>print</c>
    ///     command. Invokes <see cref="Validate.RunSpdxTool(string, string[])"/> with <c>--silent</c>,
    ///     <c>--log</c>, and <c>run-workflow</c> arguments, then reads the log file and verifies it
    ///     contains "Found version 2.0.0".
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or the log file cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        const string tempDir = Validate.TempDir;

        // Write test SPDX file
        File.WriteAllText($"{tempDir}/test.spdx.json",
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package-1",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "packageFileName": "package1.zip",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Package-2",
                  "name": "Another Test Package",
                  "versionInfo": "2.0.0",
                  "packageFileName": "package2.tar",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-1",
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
              },
              "documentDescribes": [ "SPDXRef-Package-1" ]
            }
            """);

        // Write test workflow file
        File.WriteAllText($"{tempDir}/workflow.yaml",
            """
            steps:
            - command: get-version
              inputs:
                spdx: test.spdx.json
                id: SPDXRef-Package-2
                output: version
            - command: print
              inputs:
                text:
                - Found version ${{ version }}
            """);

        // Run the workflow file
        var exitCode = Validate.RunSpdxTool(
            tempDir,
            [
                "--silent",
                "--log", "output.log",
                "run-workflow",
                "workflow.yaml"
            ]);

        // Fail if SpdxTool reported an error
        if (exitCode != 0)
        {
            return false;
        }

        // Fail if log file is absent
        if (!File.Exists($"{tempDir}/output.log"))
        {
            return false;
        }

        // Read the log file
        var log = File.ReadAllText($"{tempDir}/output.log");

        // Verify expected output
        return log.Contains("Found version 2.0.0");
    }
}
