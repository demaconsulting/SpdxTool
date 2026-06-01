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
using DemaConsulting.SpdxTool.Utility;
using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.Tests.SelfTest;

/// <summary>
///     Unit tests for the ValidateUpdatePackage self-validation unit.
/// </summary>
/// <remarks>
///     These tests exercise the <see cref="ValidateUpdatePackage"/> self-test step.
///     All tests are part of the <c>SelfTestValidation</c> collection to serialize
///     self-test execution and prevent working-directory conflicts between steps.
/// </remarks>
[Collection("SelfTestValidation")]
public class ValidateUpdatePackageTests
{
    /// <summary>
    ///     Test that ValidateUpdatePackage validation passes.
    /// </summary>
    /// <remarks>
    ///     This is a deliberate formal deviation: the method name <c>SpdxTool_UpdatePackage</c> matches
    ///     the <c>TestResult.Name</c> identifier recorded by <see cref="ValidateUpdatePackage.Run"/> so
    ///     that ReqStream can trace this xUnit test to the self-test result it exercises. This method
    ///     name is therefore exempt from the 4-segment naming rule per the csharp-testing.md standard.
    /// </remarks>
    [Fact]
    public void SpdxTool_UpdatePackage()
    {
        // Arrange: create a context and empty results collection
        using var context = Context.Create(["--validate"]);
        var results = new DemaConsulting.TestResults.TestResults();

        // Act: run the update-package self-test step
        ValidateUpdatePackage.Run(context, results);

        // Assert: single passing result recorded
        Assert.Single(results.Results);
        Assert.Equal(TestOutcome.Passed, results.Results[0].Outcome);
    }

    /// <summary>
    ///     Test that ValidateUpdatePackage.Run records TestOutcome.Failed when the update-package command
    ///     exits with a non-zero exit code.
    /// </summary>
    /// <remarks>
    ///     The <see cref="ValidateUpdatePackage.PreRunSpdxToolHookForTest"/> hook is set to corrupt
    ///     <c>test.spdx.json</c> with invalid JSON immediately before the in-process update-package
    ///     command reads it. This causes update-package to fail with a non-zero exit code, which causes
    ///     <c>DoValidate</c> to return <c>false</c> and <c>Run</c> to record
    ///     <see cref="TestOutcome.Failed"/>.
    /// </remarks>
    [Fact]
    public void ValidateUpdatePackage_Run_CommandFailure_RecordsFailedOutcome()
    {
        try
        {
            // Arrange: hook corrupts test.spdx.json immediately before update-package reads it,
            // causing the command to fail with a non-zero exit code
            ValidateUpdatePackage.PreRunSpdxToolHookForTest = () =>
                File.WriteAllText("validate.tmp/test.spdx.json", "{}");

            using var context = Context.Create(["--validate"]);
            var results = new DemaConsulting.TestResults.TestResults();

            // Act: run the update-package self-test step with the poisoned hook active
            ValidateUpdatePackage.Run(context, results);

            // Assert: single failing result recorded
            Assert.Single(results.Results);
            Assert.Equal(TestOutcome.Failed, results.Results[0].Outcome);
        }
        finally
        {
            ValidateUpdatePackage.PreRunSpdxToolHookForTest = null;
        }
    }

    /// <summary>
    ///     Test that ValidateUpdatePackage.Run propagates an I/O exception when the working
    ///     directory prevents validate.tmp from being used correctly.
    /// </summary>
    /// <remarks>
    ///     This exercises the failure path of Run() as documented in the design: exceptions
    ///     thrown by DoValidate propagate uncaught and no TestResult is recorded.
    /// </remarks>
    [Fact]
    public void ValidateUpdatePackage_Run_IoError_PropagatesException()
    {
        // Arrange: inject a temporary directory that already contains validate.tmp as a file,
        // blocking Directory.CreateDirectory("validate.tmp")
        var originalFactory = Validate.TemporaryDirectoryFactory;
        using var tempDirectory = new TemporaryDirectory();
        Validate.TemporaryDirectoryFactory = () => tempDirectory;
        try
        {
            // Create validate.tmp as a FILE (not a directory) to block DoValidate
            File.WriteAllText(tempDirectory.GetFilePath("validate.tmp"), "blocking file");

            using var context = Context.Create(["--validate"]);
            var results = new DemaConsulting.TestResults.TestResults();

            // Act + Assert: Run() propagates the IOException — no TestResult is recorded
            Assert.Throws<IOException>(() => ValidateUpdatePackage.Run(context, results));
            Assert.Empty(results.Results);
        }
        finally
        {
            Validate.TemporaryDirectoryFactory = originalFactory;
        }
    }
}
