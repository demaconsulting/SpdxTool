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
///     Unit tests for the ValidateDiagram self-validation unit.
/// </summary>
/// <remarks>
///     All tests in this class belong to the <c>SelfTestValidation</c> collection to serialize
///     execution, preventing races on the current working directory and the <c>validate.tmp</c>
///     temporary directory used by the self-test step.
/// </remarks>
[Collection("SelfTestValidation")]
public class ValidateDiagramTests
{
    /// <summary>
    ///     Test that ValidateDiagram validation passes.
    /// </summary>
    /// <remarks>
    ///     This is a deliberate formal deviation: the method name <c>SpdxTool_Diagram</c> matches
    ///     the <c>TestResult.Name</c> identifier recorded by <see cref="ValidateDiagram.Run"/> so
    ///     that ReqStream can trace this xUnit test to the self-test result it exercises. This method
    ///     name is therefore exempt from the 4-segment naming rule per the csharp-testing.md standard.
    /// </remarks>
    [Fact]
    public void SpdxTool_Diagram()
    {
        // Arrange: create a validate context and an empty results collection
        using var context = Context.Create(["--validate"]);
        var results = new DemaConsulting.TestResults.TestResults();

        // Act: run the ValidateDiagram self-test step
        ValidateDiagram.Run(context, results);

        // Assert: one passing TestResult is recorded with the correct name
        Assert.Single(results.Results);
        Assert.Equal(TestOutcome.Passed, results.Results[0].Outcome);
        Assert.Equal("SpdxTool_Diagram", results.Results[0].Name);
    }

    /// <summary>
    ///     Verifies that an I/O error in DoValidate propagates as an uncaught exception from Run.
    /// </summary>
    /// <remarks>
    ///     Pre-creates <c>validate.tmp</c> as a file in a temporary directory and sets that as the
    ///     working directory before calling <see cref="ValidateDiagram.Run"/>. When
    ///     <see cref="Directory.CreateDirectory(string)"/> encounters the blocking file it throws
    ///     <see cref="IOException"/>, which propagates uncaught from <c>Run</c>. The test asserts
    ///     both that the exception propagates and that no <see cref="DemaConsulting.TestResults.TestResult"/>
    ///     is recorded in the results collection. This exercises the failure path of Run() as
    ///     documented in the design: exceptions thrown by DoValidate propagate uncaught and no
    ///     TestResult is recorded.
    /// </remarks>
    [Fact]
    public void ValidateDiagram_Run_IoError_PropagatesException()
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
            Assert.Throws<IOException>(() => ValidateDiagram.Run(context, results));
            Assert.Empty(results.Results);
        }
        finally
        {
            Validate.TemporaryDirectoryFactory = originalFactory;
        }
    }
}
