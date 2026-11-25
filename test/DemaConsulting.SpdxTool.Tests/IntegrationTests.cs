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

using DemaConsulting.HelpCards.Catalog;
using DemaConsulting.HelpCards.Testing;
using DemaConsulting.SpdxTool.Utility;

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Integration tests for the self-validation feature.
/// </summary>
public class IntegrationTests
{
    /// <summary>
    ///     Test that the validate flag succeeds on self-validation
    /// </summary>
    [Fact]
    public void SpdxTool_SelfTest_ValidateFlag_Succeeds()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--validate");

        // Assert: Verify success
        Assert.Equal(0, exitCode);
        Assert.Contains("Validation Passed", output);
    }

    /// <summary>
    ///     Test that the validate flag with depth shows depth in output
    /// </summary>
    [Fact]
    public void SpdxTool_SelfTest_ValidateFlagWithDepth_ShowsDepth()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--validate",
            "--depth", "3");

        // Assert: Verify success
        Assert.Equal(0, exitCode);

        // Assert: Verify depth of result
        Assert.Contains("### DemaConsulting.SpdxTool", output);
    }

    /// <summary>
    ///     Test that the validate flag with results generates a TRX file
    /// </summary>
    [Fact]
    public void SpdxTool_SelfTest_ValidateFlagWithResults_GeneratesTrxFile()
    {
        const string resultFile = "results.trx";

        // Arrange: no setup required

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
            Assert.Equal(0, exitCode);

            // Read results file (normalize line endings for cross-platform compatibility)
            var results = File.ReadAllText(resultFile).Replace("\r\n", "\n");
            Assert.NotNull(results);

            // Assert: Verify the results contain expected content
            Assert.Contains("DemaConsulting.SpdxTool Validation Results -", results);
            Assert.Contains("SpdxTool_AddPackage", results);
            Assert.Contains("SpdxTool_AddRelationship", results);
            Assert.Contains("SpdxTool_CopyPackage", results);
            Assert.Contains("SpdxTool_Diagram", results);
            Assert.Contains("SpdxTool_FindPackage", results);
            Assert.Contains("SpdxTool_GetVersion", results);
            Assert.Contains("SpdxTool_Hash", results);
            Assert.Contains("SpdxTool_Ntia", results);
            Assert.Contains("SpdxTool_Query", results);
            Assert.Contains("SpdxTool_RenameId", results);
            Assert.Contains("SpdxTool_RunNuGetWorkflow", results);
            Assert.Contains("SpdxTool_ToMarkdown", results);
            Assert.Contains("SpdxTool_UpdatePackage", results);
            Assert.Contains("SpdxTool_Basic", results);
            Assert.Contains("""
                                             <ResultSummary outcome="Completed">
                                               <Counters total="14" executed="14" passed="14" failed="0" />
                                             </ResultSummary>
                                           """, results);
        }
        finally
        {
            // Delete the output file
            File.Delete(resultFile);
        }
    }

    /// <summary>
    ///     Test that the -r short flag generates a TRX file
    /// </summary>
    [Fact]
    public void SpdxTool_SelfTest_ValidateFlagWithResults_ShortFlag_GeneratesTrxFile()
    {
        const string resultFile = "results-short.trx";

        try
        {
            // Arrange: no setup required

            // Act: Run the command using the short -r flag
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "--validate",
                "-r",
                resultFile);

            // Assert: Verify success
            Assert.Equal(0, exitCode);

            // Assert: Verify result file was created
            Assert.True(File.Exists(resultFile));

            // Read results file (normalize line endings for cross-platform compatibility)
            var results = File.ReadAllText(resultFile).Replace("\r\n", "\n");
            Assert.NotNull(results);

            // Assert: Verify the results contain expected content
            Assert.Contains("DemaConsulting.SpdxTool Validation Results -", results);
            Assert.Contains("SpdxTool_Basic", results);
        }
        finally
        {
            // Delete the output file
            File.Delete(resultFile);
        }
    }

    /// <summary>
    ///     Test that the validate flag with results generates a JUnit file
    /// </summary>
    [Fact]
    public void SpdxTool_SelfTest_ValidateFlagWithResults_GeneratesJUnitFile()
    {
        const string resultFile = "results.xml";

        try
        {
            // Arrange: no setup required

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "--validate",
                "--result",
                resultFile);

            // Assert: Verify success
            Assert.Equal(0, exitCode);

            // Read results file
            var results = File.ReadAllText(resultFile);
            Assert.NotNull(results);

            // Assert: Verify the results contain expected content
            Assert.Contains("DemaConsulting.SpdxTool Validation Results -", results);
            Assert.Contains("SpdxTool_AddPackage", results);
            Assert.Contains("SpdxTool_AddRelationship", results);
            Assert.Contains("SpdxTool_CopyPackage", results);
            Assert.Contains("SpdxTool_Diagram", results);
            Assert.Contains("SpdxTool_FindPackage", results);
            Assert.Contains("SpdxTool_GetVersion", results);
            Assert.Contains("SpdxTool_Hash", results);
            Assert.Contains("SpdxTool_Ntia", results);
            Assert.Contains("SpdxTool_Query", results);
            Assert.Contains("SpdxTool_RenameId", results);
            Assert.Contains("SpdxTool_RunNuGetWorkflow", results);
            Assert.Contains("SpdxTool_ToMarkdown", results);
            Assert.Contains("SpdxTool_UpdatePackage", results);
            Assert.Contains("SpdxTool_Basic", results);
            Assert.Contains("<testsuites name=\"DemaConsulting.SpdxTool Validation Results -", results);
        }
        finally
        {
            // Delete the output file
            File.Delete(resultFile);
        }
    }

    /// <summary>
    ///     Test that SpdxTool --validate --result with unsupported extension reports an error
    /// </summary>
    [Fact]
    public void SpdxTool_SelfTest_UnsupportedResultExtension_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command with --validate and a .txt result file (unsupported)
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--validate",
            "--result", "output-validate.txt");

        // Assert: Verify error reported for unsupported extension
        Assert.Equal(1, exitCode);
        Assert.Contains("Unsupported results file format", output);
    }

    /// <summary>
    ///     Test that the help verb lists the help topics.
    /// </summary>
    [Fact]
    public void SpdxTool_HelpTopics_HelpVerb_ListsTopics()
    {
        // Arrange: no setup required

        // Act: run the tool with the help verb
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help");

        // Assert: every topic is listed with its command, without the banner
        Assert.Equal(0, exitCode);
        foreach (var topic in HelpCommand.RootCardIds)
        {
            Assert.Contains($"{HelpCommand.ToolName} help {topic}", output);
        }

        Assert.DoesNotContain($"DemaConsulting.SpdxTool {Program.Version}", output);
    }

    /// <summary>
    ///     Test that an unknown help topic returns the usage-error exit code.
    /// </summary>
    [Fact]
    public void SpdxTool_HelpTopics_UnknownTopic_ReturnsUsageError()
    {
        // Arrange: no setup required

        // Act: ask for a topic that does not exist
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "no-such-topic");

        // Assert: exit code 2 and a message that names the topic
        Assert.Equal(2, exitCode);
        Assert.Contains("no-such-topic", output);
    }

    /// <summary>
    ///     Test that every command example in the help cards gives the result that its card states.
    /// </summary>
    [Fact]
    public void SpdxTool_HelpExamples_AllCommandExamples_GiveStatedResults()
    {
        // Arrange: the catalog the tool ships, the tool assembly, and the SPDX documents that the examples read
        var catalog = HelpCatalog.Create(HelpCommand.CreateOptions().GetCatalogConfiguration());
        var dllPath = PathHelpers.SafePathCombine(AppContext.BaseDirectory, "DemaConsulting.SpdxTool.dll");
        var exampleFiles = Directory.GetFiles(
            PathHelpers.SafePathCombine(AppContext.BaseDirectory, "TestData", "HelpExamples"));

        // Act: run every command example against the built tool in the runner's scratch folder; the runner
        // compares the exit codes and the 'expect' fragments. The SPDX documents are copied in before every
        // command, so that a command that changes a document cannot affect the next one.
        var report = HelpExampleRunner.Run(catalog, invocation =>
        {
            foreach (var file in exampleFiles)
            {
                File.Copy(file, PathHelpers.SafePathCombine(invocation.WorkingDirectory, Path.GetFileName(file)), true);
            }

            var exitCode = Runner.RunInDirectory(
                out var output,
                invocation.WorkingDirectory,
                "dotnet",
                [dllPath, .. invocation.Arguments]);
            return new HelpExampleOutcome(exitCode, output);
        });

        // Assert: the cards hold command examples, and every one gave the result its card states
        Assert.True(report.CommandsRun > 0, "The help cards contain no command examples.");
        report.AssertSucceeded();
    }
}
