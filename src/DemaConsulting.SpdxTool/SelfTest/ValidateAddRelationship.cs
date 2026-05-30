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
    ///     Executes the add-relationship self-test and records the result.
    /// </summary>
    /// <param name="context">The active Program context providing output and error streams.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <remarks>
    ///     Calls <see cref="DoValidate"/> and records a <see cref="TestResult"/> named
    ///     <c>SpdxTool_AddRelationship</c> with <see cref="TestOutcome.Passed"/> or
    ///     <see cref="TestOutcome.Failed"/> depending on the return value. If <see cref="DoValidate"/>
    ///     throws an exception, the exception propagates uncaught from this method and no
    ///     <see cref="TestResult"/> is recorded for this step.
    /// </remarks>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = DoValidate();

        // Report validation result
        if (passed)
        {
            context.WriteLine("✓ SpdxTool_AddRelationship - Passed");
        }
        else
        {
            context.WriteError("✗ SpdxTool_AddRelationship - Failed");
        }

        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_AddRelationship",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateAddRelationship",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Performs the actual add-relationship validation in a temporary directory.
    /// </summary>
    /// <returns><c>true</c> if the command succeeded and the SPDX document matches expectations; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     <para>
    ///         Creates <c>validate.tmp</c>, writes the test SPDX document containing two packages and
    ///         the workflow file, invokes <see cref="Validate.RunSpdxTool(string, string[])"/>, then
    ///         reads and verifies the modified SPDX document using a positional list pattern match —
    ///         the order of relationships in the deserialized document is significant.
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
            File.WriteAllText("validate.tmp/workflow.yaml",
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
