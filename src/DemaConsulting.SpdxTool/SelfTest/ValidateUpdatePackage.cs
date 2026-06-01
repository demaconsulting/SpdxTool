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
    ///     Runs <see cref="DoValidate"/> inside a temporary directory via
    ///     <see cref="Validate.RunInTempDir"/> and records the outcome via
    ///     <see cref="Validate.RecordResult"/>. If <see cref="DoValidate"/> throws an exception,
    ///     the exception propagates uncaught from this method and no <see cref="TestResult"/> is
    ///     recorded for this step.
    /// </remarks>
    /// <param name="context">Active program context for output and error reporting. Must not be null.</param>
    /// <param name="results">Test results collection to append the step outcome to. Must not be null.</param>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = Validate.RunInTempDir("validate.tmp", DoValidate);
        Validate.RecordResult(context, results, "SpdxTool_UpdatePackage", "DemaConsulting.SpdxTool.SelfTest.ValidateUpdatePackage", passed);
    }

    /// <summary>
    ///     Performs the update-package validation. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <remarks>
    ///     Writes a minimal SPDX JSON document containing SPDXRef-Package-1 and a workflow YAML
    ///     that updates eleven workflow inputs (mapping to twelve SPDX document fields, since
    ///     <c>license</c> sets both ConcludedLicense and DeclaredLicense), then invokes the
    ///     SpdxTool run-workflow command with --silent. After the tool exits, checks that the
    ///     output SPDX file exists, then uses LINQ to locate SPDXRef-Package-1 by Id and verifies
    ///     each of the twelve updated field values individually so that deserializer ordering
    ///     changes do not cause false failures.
    /// </remarks>
    /// <returns>
    ///     True if the tool exited with code zero and every updated field in the deserialized
    ///     SPDX document matches the expected value; false otherwise.
    /// </returns>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or the SPDX document cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        const string tempDir = "validate.tmp";

        // Write test SPDX file
        Validate.WriteTestSpdxJson1Package(tempDir);

        // Write test workflow file
        File.WriteAllText($"{tempDir}/workflow.yaml",
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

        // Fail if the output SPDX file was not written
        if (!File.Exists($"{tempDir}/test.spdx.json"))
        {
            return false;
        }

        // Read the SPDX document
        var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText($"{tempDir}/test.spdx.json"));

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
}
