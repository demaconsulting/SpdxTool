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

using DemaConsulting.SpdxTool.SelfTest;
using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.Tests.SelfTest;

/// <summary>
///     Unit tests for the ValidateRunNuGetWorkflow self-validation unit.
/// </summary>
/// <remarks>
///     All tests in this class belong to the <c>SelfTestValidation</c> collection to serialize
///     execution, preventing races on the current working directory and the <c>validate.tmp</c>
///     temporary directory used by the self-test step.
/// </remarks>
[Collection("SelfTestValidation")]
public class ValidateRunNuGetWorkflowTests
{
    /// <summary>
    ///     Test that ValidateRunNuGetWorkflow validation passes.
    /// </summary>
    /// <remarks>
    ///     The <c>TestResult.Name</c> recorded by <see cref="ValidateRunNuGetWorkflow.Run"/> is
    ///     <c>SpdxTool_RunNuGetWorkflow</c>; the assertion in this test guards against regressions
    ///     where the wrong name is recorded. The test method name follows the standard 4-segment
    ///     convention; <c>SpdxTool_RunNuGetWorkflow</c> is the <c>TestResult.Name</c> identifier
    ///     recorded by the self-test step, not the test method name.
    /// </remarks>
    [Fact]
    public void ValidateRunNuGetWorkflow_Run_ValidNuGetWorkflow_Passes()
    {
        // Arrange: create a context and empty results collection
        using var context = Context.Create(["--validate"]);
        var results = new DemaConsulting.TestResults.TestResults();

        // Act: run the ValidateRunNuGetWorkflow step
        ValidateRunNuGetWorkflow.Run(context, results);

        // Assert: one passing result recorded
        Assert.Single(results.Results);
        Assert.Equal(TestOutcome.Passed, results.Results[0].Outcome);
        Assert.Equal("SpdxTool_RunNuGetWorkflow", results.Results[0].Name);
    }

    /// <summary>
    ///     Test that ValidateRunNuGetWorkflow.Run records TestOutcome.Failed when the run-workflow
    ///     command exits with a non-zero exit code.
    /// </summary>
    /// <remarks>
    ///     The <see cref="ValidateRunNuGetWorkflow.PreRunSpdxToolHookForTest"/> hook is set to
    ///     corrupt <c>workflow.yaml</c> with invalid content immediately before the in-process
    ///     run-workflow command reads it. This causes the command to fail with a non-zero exit code,
    ///     which causes <c>DoValidate</c> to return <c>false</c> and <c>Run</c> to record
    ///     <see cref="TestOutcome.Failed"/>.
    /// </remarks>
    [Fact]
    public void ValidateRunNuGetWorkflow_Run_CommandFailure_RecordsFailedOutcome()
    {
        var originalDirectory = Directory.GetCurrentDirectory();
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"spdxtool-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        try
        {
            Directory.SetCurrentDirectory(tempDirectory);

            // Arrange: hook corrupts workflow.yaml immediately before the run-workflow command
            // reads it, causing the command to fail with a non-zero exit code
            ValidateRunNuGetWorkflow.PreRunSpdxToolHookForTest = () =>
                File.WriteAllText("validate.tmp/workflow.yaml", "not: valid: yaml: content:");

            using var context = Context.Create(["--validate"]);
            var results = new DemaConsulting.TestResults.TestResults();

            // Act: run the ValidateRunNuGetWorkflow step with the poisoned hook active
            ValidateRunNuGetWorkflow.Run(context, results);

            // Assert: single failing result recorded
            Assert.Single(results.Results);
            Assert.Equal(TestOutcome.Failed, results.Results[0].Outcome);
        }
        finally
        {
            ValidateRunNuGetWorkflow.PreRunSpdxToolHookForTest = null;
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(tempDirectory, true);
        }
    }

    /// <summary>
    ///     Verifies that an I/O error in DoValidate propagates as an uncaught exception from Run.
    /// </summary>
    /// <remarks>
    ///     Pre-creates <c>validate.tmp</c> as a file in a temporary directory and sets that as the
    ///     working directory before calling <see cref="ValidateRunNuGetWorkflow.Run"/>. When
    ///     <see cref="Directory.CreateDirectory(string)"/> encounters the blocking file it throws
    ///     <see cref="IOException"/>, which propagates uncaught from <c>Run</c>. The test asserts
    ///     both that the exception propagates and that no <see cref="DemaConsulting.TestResults.TestResult"/>
    ///     is recorded in the results collection. This exercises the failure path documented in the
    ///     design: exceptions thrown by DoValidate propagate uncaught and no TestResult is recorded.
    /// </remarks>
    [Fact]
    public void ValidateRunNuGetWorkflow_Run_IoError_PropagatesException()
    {
        // Arrange: save original directory and change to a temp directory where validate.tmp
        // is pre-created as a file, blocking Directory.CreateDirectory("validate.tmp")
        var originalDirectory = Directory.GetCurrentDirectory();
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"spdxtool-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        try
        {
            Directory.SetCurrentDirectory(tempDirectory);

            // Create validate.tmp as a FILE (not a directory) to block DoValidate
            File.WriteAllText("validate.tmp", "blocking file");

            using var context = Context.Create(["--validate"]);
            var results = new DemaConsulting.TestResults.TestResults();

            // Act + Assert: Run() propagates the IOException — no TestResult is recorded
            Assert.Throws<IOException>(() => ValidateRunNuGetWorkflow.Run(context, results));
            Assert.Empty(results.Results);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(tempDirectory, true);
        }
    }
}
