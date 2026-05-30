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
///     Self-test step that exercises the <c>add-package</c> command end-to-end.
/// </summary>
/// <remarks>
///     Verifies that a package and its BUILD_TOOL_OF relationship can be added to an SPDX document
///     via a workflow file, and that the resulting document contains the expected entries. Uses a
///     temporary <c>validate.tmp</c> directory in the current working directory; callers must ensure
///     sequential execution to avoid races on that directory and on the process-wide current directory
///     set by <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateAddPackage
{
    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/test.spdx.json</c> so that the add-package command fails
    ///     with a non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the add-package self-test and records the result.
    /// </summary>
    /// <param name="context">The active Program context providing output and error streams.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <remarks>
    ///     Calls <see cref="DoValidate"/> and records a <see cref="TestResult"/> named
    ///     <c>SpdxTool_AddPackage</c> with <see cref="TestOutcome.Passed"/> or
    ///     <see cref="TestOutcome.Failed"/> depending on the return value. If <see cref="DoValidate"/>
    ///     throws an exception, the exception propagates uncaught from this method and no
    ///     <see cref="TestResult"/> is recorded for this step.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Propagated from <see cref="DoValidate"/> when the temporary directory or files
    ///     cannot be created or deleted.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Propagated from <see cref="DoValidate"/> when the current user lacks write access
    ///     to the working directory.
    /// </exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = DoValidate();

        // Report validation result
        if (passed)
        {
            context.WriteLine("✓ SpdxTool_AddPackage - Passed");
        }
        else
        {
            context.WriteError("✗ SpdxTool_AddPackage - Failed");
        }

        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_AddPackage",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateAddPackage",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Performs the actual add-package validation in a temporary directory.
    /// </summary>
    /// <returns><c>true</c> if the command succeeded and the SPDX document matches expectations; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     <para>
    ///         Creates <c>validate.tmp</c>, writes the test SPDX document and workflow file, invokes
    ///         <see cref="Validate.RunSpdxTool(string, string[])"/>, then reads and verifies the
    ///         modified SPDX document using a positional list pattern match — the order of packages
    ///         and relationships in the deserialized document is significant.
    ///     </para>
    ///     <para>
    ///         The <c>validate.tmp</c> directory is deleted in a <c>finally</c> block only if it
    ///         exists, guarding against a secondary <see cref="DirectoryNotFoundException"/> masking the
    ///         original exception when <see cref="Directory.CreateDirectory(string)"/> fails.
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
                - command: add-package
                  inputs:
                    spdx: test.spdx.json
                    package:
                      id: SPDXRef-Package-2
                      name: Test Package 2
                      version: 2.0.0
                      download: https://dotnet.microsoft.com/download
                      purl: pkg:nuget/BogusPackage@2.0.0
                    relationships:
                      - type: BUILD_TOOL_OF
                        element: SPDXRef-Package-1
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

            // Read the SPDX document
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("validate.tmp/test.spdx.json"));

            // Verify expected SPDX content
            return doc is
            {
                Packages:
                [
                { Id: "SPDXRef-Package-1" },
                { Id: "SPDXRef-Package-2" }
                ],
                Relationships:
                [
                    _,
                {
                    Id: "SPDXRef-Package-2",
                    RelationshipType: SpdxRelationshipType.BuildToolOf,
                    RelatedSpdxElement: "SPDXRef-Package-1"
                }
                ]
            };
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
