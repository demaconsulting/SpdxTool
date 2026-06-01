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
///     Self-test step that exercises the <c>copy-package</c> command end-to-end.
/// </summary>
/// <remarks>
///     Verifies that a package can be copied from one SPDX document into another via a workflow
///     file and that the destination document contains the copied package with the expected
///     CONTAINED_BY relationship. Uses a temporary <c>validate.tmp</c> directory in the current
///     working directory; callers must ensure sequential execution to avoid races on that directory
///     and on the process-wide current directory set by
///     <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateCopyPackage
{
    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/source.spdx.json</c> so that the copy-package command
    ///     fails with a non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the copy-package self-test and records the result.
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
        Validate.RecordResult(context, results, "SpdxTool_CopyPackage", "DemaConsulting.SpdxTool.SelfTest.ValidateCopyPackage", passed);
    }

    /// <summary>
    ///     Performs the actual copy-package validation. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the command succeeded and the destination SPDX document contains both
    ///     packages with the expected CONTAINED_BY relationship; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Writes a destination SPDX document (to.spdx.json) with SPDXRef-Package-1 and a source
    ///     SPDX document (from.spdx.json) with SPDXRef-Package-2, then writes a workflow YAML that
    ///     copies SPDXRef-Package-2 from the source into the destination with a CONTAINED_BY
    ///     relationship to SPDXRef-Package-1. Invokes <see cref="Validate.RunSpdxTool(string, string[])"/>,
    ///     then reads and structurally verifies the destination document.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or the SPDX document cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        const string tempDir = Validate.TempDir;

        // Write destination SPDX file
        Validate.WriteTestSpdxJson1Package(tempDir, "to.spdx.json");

        // Write source SPDX file
        File.WriteAllText($"{tempDir}/from.spdx.json",
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package-2",
                  "name": "Another Package",
                  "versionInfo": "1.2.3",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [{
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-2",
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
              "documentDescribes": [ "SPDXRef-Package-2" ]
            }
            """);

        // Write test workflow file
        File.WriteAllText($"{tempDir}/workflow.yaml",
            """
            steps:
            - command: copy-package
              inputs:
                from: from.spdx.json
                to: to.spdx.json
                package: SPDXRef-Package-2
                relationships:
                  - type: CONTAINED_BY
                    element: SPDXRef-Package-1
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
        var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText($"{tempDir}/to.spdx.json"));

        // Verify expected SPDX content using order-insensitive LINQ checks so that
        // changes to deserializer ordering do not cause false failures
        return doc.Packages.Any(p => p.Id == "SPDXRef-Package-1") &&
               doc.Packages.Any(p => p.Id == "SPDXRef-Package-2") &&
               doc.Relationships.Any(r =>
                   r.Id == "SPDXRef-Package-2" &&
                   r.RelationshipType == SpdxRelationshipType.ContainedBy &&
                   r.RelatedSpdxElement == "SPDXRef-Package-1");
    }
}
