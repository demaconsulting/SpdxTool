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
///     Self-tests the update-package command by running it against a temporary SPDX document.
/// </summary>
/// <remarks>
///     Exercises every updatable metadata field to confirm that the update-package command
///     correctly writes all changes to the SPDX document. This class is stateless; callers
///     must not invoke it concurrently because it mutates the working directory.
/// </remarks>
internal static class ValidateUpdatePackage
{
    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/test.spdx.json</c> so that the update-package command fails
    ///     with a non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Runs the update-package self-test and records the outcome in the test results collection.
    /// </summary>
    /// <remarks>
    ///     Delegates to <see cref="DoValidate"/> for the actual command invocation and field
    ///     verification, then writes a pass or fail message to the context and appends a
    ///     TestResult entry named SpdxTool_UpdatePackage to results. If <see cref="DoValidate"/>
    ///     throws an exception, the exception propagates uncaught from this method and no
    ///     <see cref="TestResult"/> is recorded for this step.
    /// </remarks>
    /// <param name="context">Active program context for output and error reporting. Must not be null.</param>
    /// <param name="results">Test results collection to append the step outcome to. Must not be null.</param>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        // Perform the validation
        var passed = DoValidate();

        // Report validation result
        if (passed)
        {
            context.WriteLine("✓ SpdxTool_UpdatePackage - Passed");
        }
        else
        {
            context.WriteError("✗ SpdxTool_UpdatePackage - Failed");
        }

        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_UpdatePackage",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateUpdatePackage",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Performs the update-package validation in a temporary working directory.
    /// </summary>
    /// <remarks>
    ///     Creates a validate.tmp directory, writes a minimal SPDX JSON document containing
    ///     SPDXRef-Package-1 and a workflow YAML that updates all twelve metadata fields, then
    ///     invokes the SpdxTool run-workflow command with --silent. After the tool exits, checks
    ///     that the output SPDX file exists, then uses LINQ to locate SPDXRef-Package-1 by Id and
    ///     verifies each of the twelve updated field values individually so that deserializer
    ///     ordering changes do not cause false failures. Deletes the temporary directory in a
    ///     <c>finally</c> block only if it exists, guarding against a secondary
    ///     <see cref="DirectoryNotFoundException"/> masking the original exception when
    ///     <see cref="Directory.CreateDirectory(string)"/> fails.
    /// </remarks>
    /// <returns>
    ///     True if the tool exited with code zero and every updated field in the deserialized
    ///     SPDX document matches the expected value; false otherwise.
    /// </returns>
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
                - command: update-package
                  inputs:
                    spdx: test.spdx.json
                    package:
                      id: SPDXRef-Package-1
                      name: New package name
                      download: https://new.package.download
                      version: 2.0.0
                      filename: new.zip
                      supplier: New Supplier
                      originator: New Originator
                      homepage: https://new.package.org
                      copyright: Copyright New Package Maker
                      summary: New Package
                      description: A new package description
                      license: MIT v2
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

            // Fail if the output SPDX file was not written
            if (!File.Exists("validate.tmp/test.spdx.json"))
            {
                return false;
            }

            // Read the SPDX document
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("validate.tmp/test.spdx.json"));

            // Find the updated package by SPDX ID to confirm correct package identity;
            // using FirstOrDefault rather than a list pattern makes the check order-insensitive
            // so that changes to deserializer ordering do not cause false failures
            var package = doc.Packages.FirstOrDefault(p => p.Id == "SPDXRef-Package-1");
            if (package == null)
            {
                return false;
            }

            // Verify all twelve updated SPDX field values individually
            return package.Name == "New package name" &&
                   package.DownloadLocation == "https://new.package.download" &&
                   package.Version == "2.0.0" &&
                   package.FileName == "new.zip" &&
                   package.Supplier == "New Supplier" &&
                   package.Originator == "New Originator" &&
                   package.HomePage == "https://new.package.org" &&
                   package.CopyrightText == "Copyright New Package Maker" &&
                   package.Summary == "New Package" &&
                   package.Description == "A new package description" &&
                   package.ConcludedLicense == "MIT v2" &&
                   package.DeclaredLicense == "MIT v2";
        }
        finally
        {
            // Delete the temporary validation folder
            if (Directory.Exists("validate.tmp"))
            {
                Directory.Delete("validate.tmp", true);
            }
        }
    }
}
