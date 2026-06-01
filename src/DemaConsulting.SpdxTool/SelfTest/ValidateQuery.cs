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
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     writes an invalid workflow YAML so that the query command fails with a non-zero exit
    ///     code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the query self-test and records the result.
    /// </summary>
    /// <remarks>
    ///     Runs <see cref="DoValidate"/> inside a temporary directory via
    ///     <see cref="Validate.RunInTempDir"/> and records the outcome via
    ///     <see cref="Validate.RecordResult"/>. If <see cref="DoValidate"/> throws an exception,
    ///     the exception propagates uncaught from this method and no <see cref="TestResult"/> is
    ///     recorded for this step.
    /// </remarks>
    /// <param name="context">The active Program context providing output and error streams. Must not be null.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <exception cref="System.IO.IOException">Propagates uncaught from DoValidate when file system operations fail.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Propagates uncaught from DoValidate when file system access is denied.</exception>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = Validate.RunInTempDir(Validate.TempDir, DoValidate);
        Validate.RecordResult(context, results, "SpdxTool_Query", "DemaConsulting.SpdxTool.SelfTest.ValidateQuery", passed);
    }

    /// <summary>
    ///     Performs the actual query validation. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the command succeeded and the log matches the version pattern;
    ///     otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Writes a workflow YAML that executes <c>query</c> against <c>dotnet --version</c>,
    ///     extracts the version using a regex pattern into the <c>version</c> variable, and prints
    ///     it via the <c>print</c> command. Invokes <see cref="Validate.RunSpdxTool(string, string[])"/>
    ///     with <c>--silent</c>, <c>--log</c>, and <c>run-workflow</c> arguments, then reads the
    ///     log file and verifies it matches the <see cref="VersionRegex"/> pattern.
    ///     <strong>Thread safety:</strong> <see cref="Validate.RunSpdxTool(string, string[])"/>
    ///     temporarily mutates the process-wide current working directory; callers must execute serially.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or the log file cannot be read.</exception>
    /// <exception cref="System.UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        const string tempDir = Validate.TempDir;

        // Write test workflow file
        File.WriteAllText($"{tempDir}/workflow.yaml",
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
        return VersionRegex().IsMatch(log);
    }

    /// <summary>
    ///     Returns a compiled regular expression that matches a dotnet version output line in the
    ///     <c>output.log</c> file produced by the query self-test workflow.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.Text.RegularExpressions.Regex"/> that matches the line
    ///     <c>Dotnet version &lt;major&gt;.&lt;minor&gt;.&lt;patch&gt;</c> as emitted by the
    ///     <c>print</c> step in the query workflow after extracting the dotnet version.
    /// </returns>
    [GeneratedRegex(@"Dotnet version \d+\.\d+\.\d+")]
    private static partial Regex VersionRegex();
}
