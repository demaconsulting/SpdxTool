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

using DemaConsulting.SpdxTool.SelfValidation;

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Tests for the SelfTest subsystem.
/// </summary>
[TestClass]
public class SelfTestTests
{
    /// <summary>
    ///     Test that Validate.Run succeeds with a --validate context
    /// </summary>
    [TestMethod]
    public void SelfTest_Validate_Succeeds()
    {
        // Arrange: create context with --validate flag
        using var context = Context.Create(["--validate"]);

        // Act: run the self-test subsystem directly
        Validate.Run(context);

        // Assert: no errors
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that Validate.Run succeeds with depth control
    /// </summary>
    [TestMethod]
    public void SelfTest_ValidateWithDepth_Succeeds()
    {
        // Arrange: create context with --validate --depth flags
        using var context = Context.Create(["--validate", "--depth", "2"]);

        // Act: run the self-test subsystem directly
        Validate.Run(context);

        // Assert: no errors
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that Validate.Run generates a TRX result file
    /// </summary>
    [TestMethod]
    public void SelfTest_ValidateWithTrxResult_GeneratesTrxFile()
    {
        var resultFile = Path.Combine(Path.GetTempPath(), $"spdxtool-st-{Guid.NewGuid():N}.trx");

        try
        {
            // Arrange: create context with --validate --result flags
            using var context = Context.Create(["--validate", "--result", resultFile]);

            // Act: run the self-test subsystem directly
            Validate.Run(context);

            // Assert: file created and contains expected content
            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(resultFile));
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
    [TestMethod]
    public void SelfTest_ValidateWithJUnitResult_GeneratesJUnitFile()
    {
        var resultFile = Path.Combine(Path.GetTempPath(), $"spdxtool-st-{Guid.NewGuid():N}.xml");

        try
        {
            // Arrange: create context with --validate --result flags
            using var context = Context.Create(["--validate", "--result", resultFile]);

            // Act: run the self-test subsystem directly
            Validate.Run(context);

            // Assert: file created and contains expected content
            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(resultFile));
            var results = File.ReadAllText(resultFile);
            Assert.Contains("DemaConsulting.SpdxTool Validation Results -", results);
        }
        finally
        {
            File.Delete(resultFile);
        }
    }
}
