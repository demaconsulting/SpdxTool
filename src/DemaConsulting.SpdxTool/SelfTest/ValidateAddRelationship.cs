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

using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;
using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.SelfTest;

/// <summary>
///     Self-test step that exercises the <c>add-relationship</c> command end-to-end.
/// </summary>
/// <remarks>
///     Verifies that a CONTAINS relationship can be added between two existing SPDX packages via a
///     workflow file, and that the resulting document contains the expected relationship with the
///     correct type, element IDs, and comment. Uses a temporary <c>validate.tmp</c> directory in
///     the current working directory; callers must ensure sequential execution to avoid races on
///     that directory and on the process-wide current directory set by
///     <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateAddRelationship
{
    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/test.spdx.json</c> so that the add-relationship command
    ///     fails with a non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the add-relationship self-test and records the result.
    /// </summary>
    /// <param name="context">The active Program context providing output and error streams.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <remarks>
    ///     Runs <see cref="DoValidate"/> inside a temporary directory via
    ///     <see cref="Validate.RunInTempDir"/> and records the outcome via
    ///     <see cref="Validate.RecordResult"/>. If <see cref="DoValidate"/> throws an exception,
    ///     the exception propagates uncaught from this method and no <see cref="TestResult"/> is
    ///     recorded for this step.
    ///     Not thread-safe; see class remarks for the serial-execution requirement.
    /// </remarks>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = Validate.RunInTempDir(Validate.TempDir, DoValidate);
        Validate.RecordResult(context, results, "SpdxTool_AddRelationship", "DemaConsulting.SpdxTool.SelfTest.ValidateAddRelationship", passed);
    }

    /// <summary>
    ///     Performs the actual add-relationship validation. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <returns><c>true</c> if the command succeeded and the SPDX document matches expectations; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     Writes the test SPDX document containing two packages and the workflow file, invokes
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/>, then reads and verifies the modified
    ///     SPDX document using a positional list pattern match — the order of relationships in the
    ///     deserialized document is significant.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or the SPDX document cannot be read.</exception>
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
            - command: add-relationship
              inputs:
                spdx: test.spdx.json
                id: SPDXRef-Package-1
                relationships:
                - type: CONTAINS
                  element: SPDXRef-Package-2
                  comment: Package 1 contains Package 2
            """);

        // Allow tests to corrupt fixtures immediately before the command runs
        PreRunSpdxToolHookForTest?.Invoke();

        // Run the workflow file
        var exitCode = Validate.RunSpdxTool(
            tempDir,
            [
                "--silent",
                "run-workflow",
                "workflow.yaml"
            ]);

        // Fail if SpdxTool reported an error
        if (exitCode != 0)
        {
            return false;
        }

        // Read the SPDX document
        var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText($"{tempDir}/test.spdx.json"));

        // Verify expected SPDX content
        return doc is
        {
            Relationships:
            [
                {
                    Id: "SPDXRef-Package-1",
                    RelationshipType: SpdxRelationshipType.Contains,
                    RelatedSpdxElement: "SPDXRef-Package-2",
                    Comment: "Package 1 contains Package 2"
                }
            ]
        };
    }
}
