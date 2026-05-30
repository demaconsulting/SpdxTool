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

using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.SelfTest;

/// <summary>
///     Self-test step that exercises the <c>run-workflow</c> command with a NuGet package source.
/// </summary>
/// <remarks>
///     Verifies that a workflow file can be resolved from a NuGet package in the local cache and
///     executed successfully, with outputs captured into workflow variables. Uses a temporary
///     <c>validate.tmp</c> directory in the current working directory; callers must ensure
///     sequential execution to avoid races on that directory and on the process-wide current directory
///     set by <see cref="Validate.RunSpdxTool(string, string[])"/>.
/// </remarks>
internal static class ValidateRunNuGetWorkflow
{
    /// <summary>
    ///     Optional test hook invoked after fixture files are written and immediately before
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> is called.
    /// </summary>
    /// <remarks>
    ///     This property is <c>null</c> in production. Tests may set it to a delegate that
    ///     corrupts <c>validate.tmp/workflow.yaml</c> so that the run-workflow command fails
    ///     with a non-zero exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the NuGet workflow self-test and records the result.
    /// </summary>
    /// <remarks>
    ///     Calls <see cref="DoValidate"/> and records a <see cref="TestResult"/> named
    ///     <c>SpdxTool_RunNuGetWorkflow</c> with <see cref="TestOutcome.Passed"/> or
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
            context.WriteLine("✓ SpdxTool_RunNuGetWorkflow - Passed");
        }
        else
        {
            context.WriteError("✗ SpdxTool_RunNuGetWorkflow - Failed");
        }

        // Add validation result to test results collection
        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_RunNuGetWorkflow",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateRunNuGetWorkflow",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Performs the actual NuGet workflow validation in a temporary directory.
    /// </summary>
    /// <returns><c>true</c> if the workflow resolved and executed with exit code zero; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     Creates <c>validate.tmp</c>, writes a workflow YAML that references the
    ///     <c>DemaConsulting.SpdxWorkflows</c> NuGet package and executes the
    ///     <c>GetDotNetVersion.yaml</c> workflow within it, mapping the version output to the
    ///     <c>dotnet-version</c> variable and printing it. Invokes
    ///     <see cref="Validate.RunSpdxTool(string, string[])"/> with <c>--silent</c> and
    ///     <c>run-workflow</c> arguments. The <c>validate.tmp</c> directory is deleted in a
    ///     <c>finally</c> block only if it exists, guarding against a secondary
    ///     <see cref="DirectoryNotFoundException"/> masking the original exception when
    ///     <see cref="Directory.CreateDirectory(string)"/> fails. Returns <c>false</c> if the NuGet package cannot be resolved because it is
    ///     absent from the local cache and network access is unavailable.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the temporary directory or files cannot be created or deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        try
        {
            // Create the temporary validation folder
            Directory.CreateDirectory("validate.tmp");

            // Write test workflow file that runs the GetDotNetVersion workflow from NuGet
            File.WriteAllText("validate.tmp/workflow.yaml",
                """
                steps:
                - command: run-workflow
                  inputs:
                    nuget: "DemaConsulting.SpdxWorkflows:1.0.0"
                    file: "contentFiles/any/any/workflows/GetDotNetVersion.yaml"
                    outputs:
                      version: dotnet-version

                - command: print
                  inputs:
                    text:
                    - DotNet version is ${{ dotnet-version }}
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
            return exitCode == 0;
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
