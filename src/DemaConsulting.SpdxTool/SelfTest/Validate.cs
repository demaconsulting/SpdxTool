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

using System.Runtime.InteropServices;
using DemaConsulting.TestResults;
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.SpdxTool.SelfTest;

/// <summary>
///     Orchestrates the complete Self-Test suite for DemaConsulting.SpdxTool.
/// </summary>
/// <remarks>
///     This class is the entry point invoked by <see cref="Program"/> when the <c>--validate</c>
///     flag is detected. It runs all individual validation step classes in sequence, collects
///     pass/fail <see cref="DemaConsulting.TestResults.TestResult"/> entries, prints a summary,
///     and optionally serializes the results to a TRX or JUnit XML file.
/// </remarks>
public static class Validate
{
    /// <summary>
    ///     Temporary working directory used by the self-test steps.
    /// </summary>
    /// <remarks>
    ///     Centralized here so all self-test helpers use the same process-relative folder name
    ///     when creating and cleaning up temporary fixtures.
    /// </remarks>
    internal const string TempDir = "validate.tmp";

    /// <summary>
    ///     Executes the complete self-test suite using the supplied Program context.
    /// </summary>
    /// <param name="context">The active Program context providing output and error streams.</param>
    /// <remarks>
    ///     Writes a system-information header (tool version, machine name, OS description, .NET runtime
    ///     version, UTC timestamp) before invoking any steps. All step classes are invoked in sequence;
    ///     individual step failures are captured as <see cref="DemaConsulting.TestResults.TestResult"/>
    ///     entries and do not abort the suite. Computes total, passed, and failed counts after all steps
    ///     complete, writing "Validation Passed" if <see cref="Context.Errors"/> is zero. If
    ///     <see cref="Context.ValidationFile"/> is set, the results file is written via
    ///     <see cref="WriteResultsFile"/>.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Propagated from individual step classes or <see cref="WriteResultsFile"/> when file I/O
    ///     operations fail.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Propagated from individual step classes or <see cref="WriteResultsFile"/> when the process
    ///     lacks write permission for the working or results directory.
    /// </exception>
    public static void Run(Context context)
    {
        // Write validation header
        context.WriteLine(
            $"""
             {new string('#', context.Depth)} DemaConsulting.SpdxTool

             | Information         | Value                                              |
             | :------------------ | :------------------------------------------------- |
             | SpdxTool Version    | {Program.Version,-50} |
             | Machine Name        | {Environment.MachineName,-50} |
             | OS Version          | {RuntimeInformation.OSDescription,-50} |
             | DotNet Runtime      | {Environment.Version,-50} |
             | Time Stamp          | {DateTime.UtcNow,-50:u} |

             """);

        var results = new TestResults.TestResults
        {
            Name = $"DemaConsulting.SpdxTool Validation Results - {Program.Version}"
        };

        // Run validation tests
        ValidateAddPackage.Run(context, results);
        ValidateAddRelationship.Run(context, results);
        ValidateBasic.Run(context, results);
        ValidateCopyPackage.Run(context, results);
        ValidateDiagram.Run(context, results);
        ValidateFindPackage.Run(context, results);
        ValidateGetVersion.Run(context, results);
        ValidateHash.Run(context, results);
        ValidateNtia.Run(context, results);
        ValidateQuery.Run(context, results);
        ValidateRenameId.Run(context, results);
        ValidateRunNuGetWorkflow.Run(context, results);
        ValidateToMarkdown.Run(context, results);
        ValidateUpdatePackage.Run(context, results);

        // Calculate and print summary counts
        var totalTests = results.Results.Count;
        var passedTests = results.Results.Count(t => t.Outcome == TestOutcome.Passed);
        var failedTests = results.Results.Count(t => t.Outcome == TestOutcome.Failed);

        context.WriteLine($"\nTotal Tests: {totalTests}");
        context.WriteLine($"Passed: {passedTests}");
        if (failedTests > 0)
        {
            context.WriteError($"Failed: {failedTests}");
        }
        else
        {
            context.WriteLine($"Failed: {failedTests}");
        }

        // Save test results
        if (!string.IsNullOrEmpty(context.ValidationFile))
        {
            WriteResultsFile(context, results);
        }

        // If all validations succeeded (no errors) then report validation passed
        if (context.Errors == 0)
        {
            context.WriteLine("\nValidation Passed");
        }
    }

    /// <summary>
    ///     Serializes the collected test results to the file path in <see cref="Context.ValidationFile"/>.
    /// </summary>
    /// <param name="context">The active Program context; provides the output file path and error stream.</param>
    /// <param name="results">The collected test results to serialize.</param>
    /// <remarks>
    ///     Supports <c>.trx</c> (Visual Studio TRX) and <c>.xml</c> (JUnit XML) output formats.
    ///     For an unsupported extension, an error message is written to the context and no file is produced.
    ///     IO exceptions from the file-write operation propagate unhandled to the caller as fatal errors.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Thrown when the result file cannot be written (disk full, invalid path, etc.).
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Thrown when the process does not have write permission for the result file path.
    /// </exception>
    private static void WriteResultsFile(Context context, TestResults.TestResults results)
    {
        var extension = Path.GetExtension(context.ValidationFile).ToLowerInvariant();
        string content;

        if (extension == ".trx")
        {
            content = TrxSerializer.Serialize(results);
        }
        else if (extension == ".xml")
        {
            // Assume JUnit format for .xml extension
            content = JUnitSerializer.Serialize(results);
        }
        else
        {
            context.WriteError($"Unsupported results file format '{extension}'. Use .trx or .xml extension.");
            return;
        }

        File.WriteAllText(context.ValidationFile, content);
    }

    /// <summary>
    ///     Runs SpdxTool in-process with the supplied argument array.
    /// </summary>
    /// <param name="args">The command-line arguments to pass to SpdxTool.</param>
    /// <returns>The exit code returned by <see cref="Program.Run"/>.</returns>
    /// <remarks>
    ///     Creates a new <see cref="Context"/>, invokes <see cref="Program.Run"/>, then disposes
    ///     the context and returns its exit code. This overload does not change the current directory.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Propagated from any command executed by <see cref="Program.Run"/> that performs file I/O.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Propagated from any command that requires write access to the file system.
    /// </exception>
    internal static int RunSpdxTool(string[] args)
    {
        // Create the context
        using var context = Context.Create(args);

        // Run SpdxTool
        Program.Run(context);

        // Return the exit code
        return context.ExitCode;
    }

    /// <summary>
    ///     Runs SpdxTool in the specified folder with the supplied argument array.
    /// </summary>
    /// <param name="workingFolder">The directory to set as the current working directory before running. Must exist on disk.</param>
    /// <param name="args">The command-line arguments to pass to SpdxTool.</param>
    /// <returns>The exit code returned by <see cref="Program.Run"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         Changes the process-wide current working directory to <paramref name="workingFolder"/> before
    ///         running, and restores the original directory in a <c>finally</c> block regardless of outcome.
    ///     </para>
    ///     <para>
    ///         <strong>Thread safety:</strong> <see cref="Directory.SetCurrentDirectory"/> mutates global
    ///         process state. Concurrent calls to this overload (or any code that depends on the current
    ///         directory) will race. All callers within the Self-Test subsystem must execute serially.
    ///     </para>
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Propagated from any command executed by <see cref="Program.Run"/> that performs file I/O.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Propagated from any command that requires write access to the file system.
    /// </exception>
    internal static int RunSpdxTool(string workingFolder, string[] args)
    {
        var cwd = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingFolder);
            return RunSpdxTool(args);
        }
        finally
        {
            Directory.SetCurrentDirectory(cwd);
        }
    }

    /// <summary>
    ///     Records a self-test result with pass/fail reporting.
    /// </summary>
    /// <param name="context">The active Program context providing output and error streams.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <param name="testName">The short test name used in console output and the TestResult record.</param>
    /// <param name="className">The fully-qualified class name written to the TestResult record.</param>
    /// <param name="passed">Whether the test passed.</param>
    /// <remarks>
    ///     Writes a Unicode check or cross mark followed by <paramref name="testName"/> and the outcome
    ///     to <paramref name="context"/>, then appends a <see cref="TestResult"/> to
    ///     <paramref name="results"/>. <see cref="TestResult.StartTime"/> is set to
    ///     <see cref="DateTime.Now"/> at the moment this method is called.
    /// </remarks>
    internal static void RecordResult(
        Context context,
        TestResults.TestResults results,
        string testName,
        string className,
        bool passed)
    {
        if (passed)
        {
            context.WriteLine($"✓ {testName} - Passed");
        }
        else
        {
            context.WriteError($"✗ {testName} - Failed");
        }

        results.Results.Add(
            new TestResult
            {
                Name = testName,
                ClassName = className,
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Writes a single-package SPDX 2.2 document used as the starting fixture for add-package,
    ///     rename-id, update-package, and copy-package self-tests.
    /// </summary>
    /// <param name="tempDir">Path to the directory to write the file into. Must exist on disk.</param>
    /// <param name="fileName">
    ///     Filename to write within <paramref name="tempDir"/>. Defaults to <c>test.spdx.json</c>.
    /// </param>
    /// <remarks>
    ///     Produces a minimal SPDX 2.2 document containing one package (<c>SPDXRef-Package-1</c>,
    ///     "Test Package", version 1.0.0, MIT license) with a single DESCRIBES relationship and a
    ///     <c>documentDescribes</c> entry. The fixture is shared across multiple self-test steps
    ///     that each write their own workflow YAML to modify the document.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Thrown when the file cannot be written (disk full, invalid path, etc.).
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Thrown when the process does not have write permission for the file path.
    /// </exception>
    internal static void WriteTestSpdxJson1Package(string tempDir, string fileName = "test.spdx.json")
    {
        File.WriteAllText($"{tempDir}/{fileName}",
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
    }

    /// <summary>
    ///     Writes a minimal single-package SPDX 2.2 document used as the starting fixture for basic
    ///     validation and NTIA compliance self-tests.
    /// </summary>
    /// <param name="tempDir">Path to the directory to write the file into. Must exist on disk.</param>
    /// <param name="fileName">Filename to write within <paramref name="tempDir"/>.</param>
    /// <remarks>
    ///     Produces a minimal SPDX 2.2 document containing one package (<c>SPDXRef-Package</c>,
    ///     "Test Package", version 1.0.0, MIT license, <c>filesAnalyzed: false</c>) with a single
    ///     DESCRIBES relationship but no <c>documentDescribes</c> entry. The document is valid for
    ///     the <c>validate</c> command but intentionally omits the supplier field so it is not
    ///     NTIA-compliant, making it suitable as a baseline fixture for both tests.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Thrown when the file cannot be written (disk full, invalid path, etc.).
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Thrown when the process does not have write permission for the file path.
    /// </exception>
    internal static void WriteTestSpdxJsonMinimal(string tempDir, string fileName)
    {
        File.WriteAllText($"{tempDir}/{fileName}",
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "filesAnalyzed": false,
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package",
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
              }
            }
            """);
    }

    /// <summary>
    ///     Creates a temporary directory, executes an action within it, and deletes it on completion.
    /// </summary>
    /// <param name="tempDir">The temporary directory path to create. Must be a valid writable path.</param>
    /// <param name="action">The action to execute after the directory is created. Must not be null.</param>
    /// <returns>The boolean result returned by <paramref name="action"/>.</returns>
    /// <remarks>
    ///     Creates <paramref name="tempDir"/>, invokes <paramref name="action"/>, and deletes the
    ///     directory in a <c>finally</c> block only if it exists — guarding against a secondary
    ///     <see cref="DirectoryNotFoundException"/> masking the original exception when
    ///     <see cref="Directory.CreateDirectory(string)"/> fails.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Propagated from <see cref="Directory.CreateDirectory(string)"/>,
    ///     <paramref name="action"/>, or <see cref="Directory.Delete(string, bool)"/> when file
    ///     I/O operations fail.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Propagated when the process lacks write permission for <paramref name="tempDir"/>.
    /// </exception>
    internal static bool RunInTempDir(string tempDir, Func<bool> action)
    {
        try
        {
            Directory.CreateDirectory(tempDir);
            return action();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
