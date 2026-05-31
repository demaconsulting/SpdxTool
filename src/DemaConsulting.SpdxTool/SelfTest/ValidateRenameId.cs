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

using DemaConsulting.SpdxModel.IO;
using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.SelfTest;

/// <summary>
///     Self-test step that exercises the <c>rename-id</c> command end-to-end.
/// </summary>
/// <remarks>
///     Verifies that an SPDX element ID and all its references (including relationship entries) are
///     renamed consistently throughout an SPDX document when <c>rename-id</c> is invoked via a
///     workflow file. Uses a temporary <c>validate.tmp</c> directory in the current working directory;
///     callers must ensure sequential execution to avoid races on that directory and on the
///     process-wide current directory set by
///     <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateRenameId
{
    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/test.spdx.json</c> so that the rename-id command fails
    ///     with a non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the rename-id self-test and records the result.
    /// </summary>
    /// <remarks>
    ///     Calls <see cref="DoValidate"/> and records a <see cref="TestResult"/> named
    ///     <c>SpdxTool_RenameId</c> with <see cref="TestOutcome.Passed"/> or
    ///     <see cref="TestOutcome.Failed"/> depending on the return value. If <see cref="DoValidate"/>
    ///     throws an exception, the exception propagates uncaught from this method and no
    ///     <see cref="TestResult"/> is recorded for this step.
    /// </remarks>
    /// <param name="context">The active Program context providing output and error streams. Must not be null.</param>
    /// <param name="results">The TestResults collection to append the step outcome to. Must not be null.</param>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        // Perform the validation
        var passed = DoValidate();

        // Report validation result
        if (passed)
        {
            context.WriteLine("✓ SpdxTool_RenameId - Passed");
        }
        else
        {
            context.WriteError("✗ SpdxTool_RenameId - Failed");
        }

        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_RenameId",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateRenameId",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Creates a temporary SPDX document and workflow, invokes <c>rename-id</c> via RunSpdxTool,
    ///     and verifies that the target ID and all relationship references are updated.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the command succeeded and the SPDX document reflects the renamed ID;
    ///     otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Creates <c>validate.tmp</c>, writes an SPDX JSON document with a package using
    ///         <c>SPDXRef-Package-1</c>, a <c>DESCRIBES</c> relationship referencing that ID, and a
    ///         <c>documentDescribes</c> entry referencing that ID, plus a workflow YAML that renames
    ///         <c>SPDXRef-Package-1</c> to <c>SPDXRef-Package-2</c>. Invokes
    ///         <see cref="Validate.RunSpdxTool(string, string[])"/> with <c>--silent</c> and
    ///         <c>run-workflow</c> arguments, then reads the modified SPDX file and verifies that the
    ///         package ID, relationship reference, and <c>documentDescribes</c> entry have all been
    ///         updated to <c>SPDXRef-Package-2</c>.
    ///     </para>
    ///     <para>
    ///         <strong>Thread safety:</strong> <see cref="Validate.RunSpdxTool(string, string[])"/>
    ///         temporarily mutates the process-wide current working directory; callers must execute
    ///         serially to avoid races.
    ///     </para>
    ///     <para>
    ///         The <c>validate.tmp</c> directory is deleted in a <c>finally</c> block only if it exists,
    ///         guarding against a secondary <see cref="DirectoryNotFoundException"/> masking the original
    ///         exception when <see cref="Directory.CreateDirectory(string)"/> fails.
    ///     </para>
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the temporary directory or files cannot be created or deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        try
        {
            // Create the temporary validation folder
            Directory.CreateDirectory("validate.tmp");

            // Write test SPDX file
            File.WriteAllText("validate.tmp/test.spdx.json",
                """
                {
                  "files": [],
                  "packages": [    {
                      "SPDXID": "SPDXRef-Package-1",
                      "name": "Test Package",
                      "versionInfo": "1.0.0",
                      "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
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
            File.WriteAllText("validate.tmp/workflow.yaml",
                """
                steps:
                - command: rename-id
                  inputs:
                    spdx: test.spdx.json
                    old: SPDXRef-Package-1
                    new: SPDXRef-Package-2
                """);

            // Allow tests to corrupt fixtures immediately before the command runs
            PreRunSpdxToolHookForTest?.Invoke();

            // Run the workflow file
            var exitCode = Validate.RunSpdxTool(
                "validate.tmp",
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

            // Fail if SPDX document is absent
            if (!File.Exists("validate.tmp/test.spdx.json"))
            {
                return false;
            }

            // Read the SPDX document
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("validate.tmp/test.spdx.json"));

            // Verify expected SPDX content using order-insensitive LINQ checks so that
            // changes to deserializer ordering do not cause false failures
            return doc.Packages.Any(p => p.Id == "SPDXRef-Package-2") &&
                   doc.Relationships.Any(r => r.RelatedSpdxElement == "SPDXRef-Package-2") &&
                   doc.Describes.Any(d => d == "SPDXRef-Package-2");
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
}
