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

namespace DemaConsulting.SpdxTool.Tests.SelfTest;

/// <summary>
///     Tests for the SelfTest subsystem.
/// </summary>
/// <remarks>
///     This class belongs to the <c>SelfTestValidation</c> xUnit collection defined in
///     <see cref="SelfTestValidationCollection"/>, which enforces serial execution to prevent
///     races on the process-wide current directory and <c>Console.Out</c> caused by the
///     <c>ValidateXxx</c> step classes and <see cref="Validate.Run"/> respectively.
/// </remarks>
[Collection("SelfTestValidation")]
public class SelfTestTests
{
    /// <summary>
    ///     Test that Validate.Run succeeds with a --validate context
    /// </summary>
    /// <remarks>
    ///     Verifies requirement scenarios <c>SpdxTool-SelfTest-Orchestrate</c> (orchestration and
    ///     outcome collection), <c>SpdxTool-SelfTest-Orchestrate-SummaryReport</c> (pass/fail
    ///     summary written to the output stream), and
    ///     <c>SpdxTool-SelfTest-Orchestrate-SystemInfoHeader</c> (system-information header fields
    ///     present in captured output). The test confirms that all validation steps complete without
    ///     errors and that the expected system-information header fields appear in the captured output.
    /// </remarks>
    [Fact]
    public void SelfTest_Validate_ValidContext_Succeeds()
    {
        // Arrange: create context with --validate flag; capture console output
        using var context = Context.Create(["--validate"]);
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act: run the self-test subsystem directly
            Validate.Run(context);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        // Assert: no errors
        Assert.Equal(0, context.ExitCode);

        // Assert: system-information header fields are present in the output
        var output = writer.ToString();
        Assert.Contains("SpdxTool Version", output);
        Assert.Contains("Machine Name", output);
        Assert.Contains("OS Version", output);
        Assert.Contains("DotNet Runtime", output);
        Assert.Contains("Time Stamp", output);

        // Assert: pass/fail summary is written to the output (SpdxTool-SelfTest-Orchestrate-SummaryReport)
        Assert.Contains("Total Tests", output);
        Assert.Contains("Passed", output);
        Assert.Contains("Validation Passed", output);
    }

    /// <summary>
    ///     Test that Validate.Run succeeds with depth control
    /// </summary>
    /// <remarks>
    ///     Verifies requirement scenario <c>SpdxTool-SelfTest-Orchestrate-DepthHeading</c>: when
    ///     <c>--depth 2</c> is supplied, the self-test report uses level-2 Markdown headers in its
    ///     output, confirming that depth-controlled output is applied to the validation result summary.
    /// </remarks>
    [Fact]
    public void SelfTest_Validate_WithDepth_Succeeds()
    {
        // Arrange: create context with --validate --depth flags; capture console output
        using var context = Context.Create(["--validate", "--depth", "2"]);
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act: run the self-test subsystem directly
            Validate.Run(context);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        // Assert: no errors and depth-structured output was produced (depth=2 produces "## " header)
        Assert.Equal(0, context.ExitCode);
        var output = writer.ToString();
        Assert.Contains("## ", output);
    }

    /// <summary>
    ///     Test that Validate.Run generates a TRX result file
    /// </summary>
    /// <remarks>
    ///     Verifies requirement scenario <c>SpdxTool-SelfTest-Orchestrate-SerializeResults</c>
    ///     (TRX format): when a <c>.trx</c> result file path is supplied, the self-test subsystem
    ///     writes a TRX-format result file containing the expected validation result header text.
    /// </remarks>
    [Fact]
    public void SelfTest_Validate_WithTrxResult_GeneratesTrxFile()
    {
        var resultFile = Path.Join(Path.GetTempPath(), $"spdxtool-st-{Guid.NewGuid():N}.trx");

        try
        {
            // Arrange: create context with --validate --result flags
            using var context = Context.Create(["--validate", "--result", resultFile]);

            // Act: run the self-test subsystem directly
            Validate.Run(context);

            // Assert: file created and contains expected content
            Assert.Equal(0, context.ExitCode);
            Assert.True(File.Exists(resultFile));
            var results = File.ReadAllText(resultFile).Replace("\r\n", "\n");
            Assert.Contains("DemaConsulting.SpdxTool Validation Results -", results);
        }
        finally
        {
            File.Delete(resultFile);
        }
    }

    /// <summary>
    ///     Test that Validate.Run generates a JUnit XML result file
    /// </summary>
    /// <remarks>
    ///     Verifies requirement scenario <c>SpdxTool-SelfTest-Orchestrate-SerializeResults</c>
    ///     (JUnit XML format): when a <c>.xml</c> result file path is supplied, the self-test
    ///     subsystem auto-detects the JUnit format from the extension and writes a JUnit XML result
    ///     file containing the expected validation result header text.
    /// </remarks>
    [Fact]
    public void SelfTest_Validate_WithJUnitResult_GeneratesJUnitFile()
    {
        var resultFile = Path.Join(Path.GetTempPath(), $"spdxtool-st-{Guid.NewGuid():N}.xml");

        try
        {
            // Arrange: create context with --validate --result flags
            using var context = Context.Create(["--validate", "--result", resultFile]);

            // Act: run the self-test subsystem directly
            Validate.Run(context);

            // Assert: file created and contains expected content
            Assert.Equal(0, context.ExitCode);
            Assert.True(File.Exists(resultFile));
            var results = File.ReadAllText(resultFile);
            Assert.Contains("DemaConsulting.SpdxTool Validation Results -", results);
        }
        finally
        {
            File.Delete(resultFile);
        }
    }

    /// <summary>
    ///     Test that Validate.Run reports an error for unsupported result file extension
    /// </summary>
    /// <remarks>
    ///     Verifies requirement scenario <c>SpdxTool-SelfTest-Orchestrate-UnsupportedExtension</c>:
    ///     when a result file with an unsupported extension (e.g., <c>.txt</c>) is supplied,
    ///     the self-test subsystem reports an error and does not create the result file, ensuring
    ///     users detect configuration mistakes immediately rather than silently getting no output.
    /// </remarks>
    [Fact]
    public void SelfTest_Validate_UnsupportedResultExtension_ReportsError()
    {
        var resultFile = Path.Join(Path.GetTempPath(), $"spdxtool-st-{Guid.NewGuid():N}.txt");

        // Arrange: create context with --validate --result flags and unsupported .txt extension
        using var context = Context.Create(["--validate", "--result", resultFile]);

        // Act: run the self-test subsystem directly
        Validate.Run(context);

        // Assert: error reported for unsupported extension, no file created
        Assert.Equal(1, context.ExitCode);
        Assert.False(File.Exists(resultFile));
    }
}
