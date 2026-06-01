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

using System.Text.RegularExpressions;
using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.SelfTest;

/// <summary>
///     Self-test step that exercises the <c>query</c> command end-to-end.
/// </summary>
/// <remarks>
///     Verifies that an external program can be queried and a version string extracted from its
///     output using a regular expression pattern, with the result captured into a workflow variable
///     and subsequently printed to the log output. Uses a temporary <c>validate.tmp</c> directory
///     in the current working directory; callers must ensure sequential execution to avoid races on
///     that directory and on the process-wide current directory set by
///     <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static partial class ValidateQuery
{
    /// <summary>
    ///     Temporary working directory name used throughout this self-test class.
    /// </summary>
    private const string TempDir = "validate.tmp";

    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/workflow.yaml</c> so that the query command fails with a
    ///     non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Returns a compiled regular expression that matches the query output containing a dotnet
    ///     version string in the form "Dotnet version N.N.N".
    /// </summary>
    /// <returns>
    ///     A compiled <see cref="System.Text.RegularExpressions.Regex"/> that matches the string
    ///     "Dotnet version N.N.N" where N is one or more digits.
    /// </returns>
    [GeneratedRegex(@"Dotnet version \d+\.\d+\.\d+")]
    private static partial Regex VersionRegex();

    /// <summary>
    ///     Executes the query self-test and records the result.
    /// </summary>
    /// <remarks>
    ///     Calls <see cref="DoValidate"/> and records a <see cref="TestResult"/> named
    ///     <c>SpdxTool_Query</c> with <see cref="TestOutcome.Passed"/> or
    ///     <see cref="TestOutcome.Failed"/> depending on the return value. If <see cref="DoValidate"/>
    ///     throws an exception, the exception propagates uncaught from this method and no
    ///     <see cref="TestResult"/> is recorded for this step.
    /// </remarks>
    /// <param name="context">The active Program context providing output and error streams. Must not be null.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        // Perform the validation
        var passed = DoValidate();

        // Report validation result
        if (passed)
        {
            context.WriteLine("✓ SpdxTool_Query - Passed");
        }
        else
        {
            context.WriteError("✗ SpdxTool_Query - Failed");
        }

        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_Query",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateQuery",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Performs the actual query validation in a temporary directory.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the command succeeded and the log matches the version pattern;
    ///     otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Creates <c>validate.tmp</c>, writes a workflow YAML that executes <c>query</c>
    ///         against <c>dotnet --version</c>, extracts the version using a regex pattern into
    ///         the <c>version</c> variable, and prints it via the <c>print</c> command. Invokes
    ///         <see cref="Validate.RunSpdxTool(string, string[])"/> with <c>--silent</c>,
    ///         <c>--log</c>, and <c>run-workflow</c> arguments, then reads the log file and
    ///         verifies it matches the <see cref="VersionRegex"/> pattern.
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
    /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        try
        {
            // Create the temporary validation folder
            Directory.CreateDirectory(TempDir);

            // Write test workflow file
            File.WriteAllText($"{TempDir}/workflow.yaml",
                """
                steps:
                - command: query
                  inputs:
                    output: version
                    pattern: (?<value>\d+\.\d+\.\d+)
                    program: dotnet
                    arguments:
                    - '--version'

                - command: print
                  inputs:
                    text:
                    - Dotnet version ${{ version }}
                """);

            // Allow tests to corrupt fixtures immediately before the command runs
            PreRunSpdxToolHookForTest?.Invoke();

            // Run the workflow file
            var exitCode = Validate.RunSpdxTool(
                TempDir,
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
            if (!File.Exists($"{TempDir}/output.log"))
            {
                return false;
            }

            // Read the log file
            var log = File.ReadAllText($"{TempDir}/output.log");

            // Verify expected output
            return VersionRegex().IsMatch(log);
        }
        finally
        {
            // Delete the temporary validation folder if it exists (guards against
            // Directory.CreateDirectory failing before the directory was created)
            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, true);
            }
        }
    }
}
