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
public class TestSelfValidation
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
        StringAssert.Contains(output, "Validation Passed");
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
        StringAssert.Contains(output, "### DemaConsulting.SpdxTool");
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
            StringAssert.Contains(results, "DemaConsulting.SpdxTool Validation Results -");
            StringAssert.Contains(results, "SpdxTool_AddPackage");
            StringAssert.Contains(results, "SpdxTool_AddRelationship");
            StringAssert.Contains(results, "SpdxTool_CopyPackage");
            StringAssert.Contains(results, "SpdxTool_FindPackage");
            StringAssert.Contains(results, "SpdxTool_GetVersion");
            StringAssert.Contains(results, "SpdxTool_Query");
            StringAssert.Contains(results, "SpdxTool_RenameId");
            StringAssert.Contains(results, "SpdxTool_UpdatePackage");
            StringAssert.Contains(results, """
                                             <ResultSummary outcome="Completed">
                                               <Counters total="8" executed="8" passed="8" failed="0" />
                                             </ResultSummary>
                                           """);
        }
        finally
        {
            // Delete the output file
            File.Delete(resultFile);
        }
    }
}