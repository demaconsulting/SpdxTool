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

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Tests for the self-validation feature.
/// </summary>
[TestClass]
public class SelfValidationTests
{
    /// <summary>
    ///     Test that the self-validation command succeeds.
    /// </summary>
    [TestMethod]
    public void SelfValidation()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--validate");

        // Assert: Verify success
        Assert.AreEqual(0, exitCode);
        Assert.Contains("Validation Passed", output);
    }

    /// <summary>
    ///     Test that the self-validation command supports depth.
    /// </summary>
    [TestMethod]
    public void SelfValidation_Depth()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--validate",
            "--depth", "3");

        // Assert: Verify success
        Assert.AreEqual(0, exitCode);

        // Assert: Verify depth of result
        Assert.Contains("### DemaConsulting.SpdxTool", output);
    }

    /// <summary>
    ///     Test that the self-validation command produces TRX results file.
    /// </summary>
    [TestMethod]
    public void SelfValidation_TrxResults()
    {
        const string resultFile = "results.trx";

        try
        {
            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "--validate",
                "--result",
                resultFile);

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Read results file
            var results = File.ReadAllText(resultFile);
            Assert.IsNotNull(results);

            // Assert: Verify the results contain expected content
            Assert.Contains("DemaConsulting.SpdxTool Validation Results -", results);
            Assert.Contains("SpdxTool_AddPackage", results);
            Assert.Contains("SpdxTool_AddRelationship", results);
            Assert.Contains("SpdxTool_CopyPackage", results);
            Assert.Contains("SpdxTool_FindPackage", results);
            Assert.Contains("SpdxTool_GetVersion", results);
            Assert.Contains("SpdxTool_Ntia", results);
            Assert.Contains("SpdxTool_Query", results);
            Assert.Contains("SpdxTool_RenameId", results);
            Assert.Contains("SpdxTool_UpdatePackage", results);
            Assert.Contains("""
                                             <ResultSummary outcome="Completed">
                                               <Counters total="9" executed="9" passed="9" failed="0" />
                                             </ResultSummary>
                                           """, results);
        }
        finally
        {
            // Delete the output file
            File.Delete(resultFile);
        }
    }
}