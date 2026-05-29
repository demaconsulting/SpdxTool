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
    ///     Regular expression to check for version
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
    /// <param name="context">The active Program context providing output and error streams.</param>
    /// <param name="results">The TestResults collection to append the step outcome to.</param>
    /// <remarks>
    ///     Calls <see cref="DoValidate"/> and records a <see cref="TestResult"/> named
    ///     <c>SpdxTool_Query</c> with <see cref="TestOutcome.Passed"/> or
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
            context.WriteLine($"✓ SpdxTool_Query - Passed");
        }
        else
        {
            context.WriteError($"✗ SpdxTool_Query - Failed");
        }

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
    ///         The <c>validate.tmp</c> directory is deleted unconditionally in a <c>finally</c> block,
    ///         even if directory creation or file writes only partially succeeded.
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

            // Write test workflow file
            File.WriteAllText("validate.tmp/workflow.yaml",
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

            // Run the workflow file
            var exitCode = Validate.RunSpdxTool(
                "validate.tmp",
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

            // Read the log file
            var log = File.ReadAllText("validate.tmp/output.log");

            // Verify expected output
            return VersionRegex().IsMatch(log);
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
