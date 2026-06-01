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
    ///     corrupts the workflow YAML so that the NuGet workflow command fails with a non-zero
    ///     exit code, exercising the CommandFailure path.
    ///     Callers must reset this property to <c>null</c> after the test completes.
    /// </remarks>
    internal static Action? PreRunSpdxToolHookForTest { get; set; }

    /// <summary>
    ///     Executes the NuGet workflow self-test and records the result.
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
        Validate.RecordResult(context, results, "SpdxTool_RunNuGetWorkflow", "DemaConsulting.SpdxTool.SelfTest.ValidateRunNuGetWorkflow", passed);
    }

    /// <summary>
    ///     Performs the actual NuGet workflow validation. Called by <see cref="Validate.RunInTempDir"/>,
    ///     which creates and cleans up the temporary directory.
    /// </summary>
    /// <returns><c>true</c> if the workflow resolved and executed with exit code zero; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     Writes a workflow YAML that references the <c>DemaConsulting.SpdxWorkflows</c> NuGet
    ///     package and executes the <c>GetDotNetVersion.yaml</c> workflow within it, mapping the
    ///     version output to the <c>dotnet-version</c> variable and printing it. Returns
    ///     <c>false</c> if the NuGet package cannot be resolved because it is absent from the
    ///     local cache and network access is unavailable.
    /// </remarks>
    /// <exception cref="System.IO.IOException">Thrown if the test files cannot be created or deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write access to the working directory.</exception>
    private static bool DoValidate()
    {
        const string tempDir = Validate.TempDir;

        // Write test workflow file that runs the GetDotNetVersion workflow from NuGet
        File.WriteAllText($"{tempDir}/workflow.yaml",
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
            tempDir,
            [
                "--silent",
                "run-workflow",
                "workflow.yaml"
            ]);

        return exitCode == 0;
    }
}
