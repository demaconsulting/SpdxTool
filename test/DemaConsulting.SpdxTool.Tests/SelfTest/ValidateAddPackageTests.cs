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
///     Unit tests for the ValidateAddPackage self-validation unit.
/// </summary>
/// <remarks>
///     All tests in this class belong to the <c>SelfTestValidation</c> collection to serialize
///     execution, preventing races on the current working directory and the <c>validate.tmp</c>
///     temporary directory used by the self-test step.
/// </remarks>
[Collection("SelfTestValidation")]
public class ValidateAddPackageTests
{
    /// <summary>
    ///     Test that ValidateAddPackage validation passes.
    /// </summary>
    /// <remarks>
    ///     This is a deliberate formal deviation: the method name <c>SpdxTool_AddPackage</c> matches
    ///     the <c>TestResult.Name</c> identifier recorded by <see cref="ValidateAddPackage.Run"/> so
    ///     that ReqStream can trace this xUnit test to the self-test result it exercises. This method
    ///     name is therefore exempt from the 4-segment naming rule per the csharp-testing.md standard.
    /// </remarks>
    [Fact]
    public void SpdxTool_AddPackage()
    {
        // Arrange: create a context and empty results collection
        using var context = Context.Create(["--validate"]);
        var results = new DemaConsulting.TestResults.TestResults();

        // Act: run the add-package self-test step
        ValidateAddPackage.Run(context, results);

        // Assert: single passing result recorded
        Assert.Single(results.Results);
        Assert.Equal(TestOutcome.Passed, results.Results[0].Outcome);
    }

    /// <summary>
    ///     Test that ValidateAddPackage.Run records TestOutcome.Failed when the add-package command
    ///     exits with a non-zero exit code.
    /// </summary>
    /// <remarks>
    ///     The <see cref="ValidateAddPackage.PreRunSpdxToolHookForTest"/> hook is set to corrupt
    ///     <c>test.spdx.json</c> with invalid JSON immediately before the in-process add-package
    ///     command reads it. This causes add-package to fail with a non-zero exit code, which causes
    ///     <c>DoValidate</c> to return <c>false</c> and <c>Run</c> to record
    ///     <see cref="TestOutcome.Failed"/>.
    /// </remarks>
    [Fact]
    public void ValidateAddPackage_Run_CommandFailure_RecordsFailedOutcome()
    {
        var originalDirectory = Directory.GetCurrentDirectory();
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"spdxtool-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        try
        {
            Directory.SetCurrentDirectory(tempDirectory);

            // Arrange: hook corrupts test.spdx.json immediately before add-package reads it,
            // causing the command to fail with a non-zero exit code
            ValidateAddPackage.PreRunSpdxToolHookForTest = () =>
                File.WriteAllText("validate.tmp/test.spdx.json", "{}");

            using var context = Context.Create(["--validate"]);
            var results = new DemaConsulting.TestResults.TestResults();

            // Act: run the add-package self-test step with the poisoned hook active
            ValidateAddPackage.Run(context, results);

            // Assert: single failing result recorded
            Assert.Single(results.Results);
            Assert.Equal(TestOutcome.Failed, results.Results[0].Outcome);
        }
        finally
        {
            ValidateAddPackage.PreRunSpdxToolHookForTest = null;
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(tempDirectory, true);
        }
    }

    /// <summary>
    ///     Test that ValidateAddPackage.Run records TestOutcome.Failed when the output SPDX document
    ///     does not match the expected package and relationship content.
    /// </summary>
    /// <remarks>
    ///     The <see cref="ValidateAddPackage.PostRunSpdxToolHookForTest"/> hook is set to overwrite
    ///     <c>validate.tmp/test.spdx.json</c> with a valid SPDX document containing wrong package
    ///     IDs after the add-package command succeeds. This causes the content-verification step in
    ///     <c>DoValidate</c> to return <c>false</c> and <c>Run</c> to record
    ///     <see cref="TestOutcome.Failed"/>.
    /// </remarks>
    [Fact]
    public void ValidateAddPackage_Run_ContentMismatch_RecordsFailedOutcome()
    {
        var originalDirectory = Directory.GetCurrentDirectory();
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"spdxtool-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        try
        {
            Directory.SetCurrentDirectory(tempDirectory);

            // Arrange: hook overwrites the output SPDX file after the command succeeds but before
            // content verification, replacing correct package IDs with wrong ones
            ValidateAddPackage.PostRunSpdxToolHookForTest = () =>
                File.WriteAllText(
                    "validate.tmp/test.spdx.json",
                    """
                    {
                      "files": [],
                      "packages": [
                        {
                          "SPDXID": "SPDXRef-Wrong-1",
                          "name": "Wrong Package",
                          "versionInfo": "1.0.0",
                          "downloadLocation": "https://example.com",
                          "licenseConcluded": "MIT"
                        }
                      ],
                      "relationships": [],
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

            using var context = Context.Create(["--validate"]);
            var results = new DemaConsulting.TestResults.TestResults();

            // Act: run the add-package self-test step with the content-mismatch hook active
            ValidateAddPackage.Run(context, results);

            // Assert: single failing result recorded
            Assert.Single(results.Results);
            Assert.Equal(TestOutcome.Failed, results.Results[0].Outcome);
        }
        finally
        {
            ValidateAddPackage.PostRunSpdxToolHookForTest = null;
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(tempDirectory, true);
        }
    }

    /// <summary>
    ///     Test that ValidateAddPackage.Run propagates an I/O exception when the working
    ///     directory prevents validate.tmp from being used correctly.
    ///     This exercises the failure path of Run() as documented in the design: exceptions
    ///     thrown by DoValidate propagate uncaught and no TestResult is recorded.
    /// </summary>
    [Fact]
    public void ValidateAddPackage_Run_IoError_PropagatesException()
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
            Assert.Throws<IOException>(() => ValidateAddPackage.Run(context, results));
            Assert.Empty(results.Results);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(tempDirectory, true);
        }
    }
}
