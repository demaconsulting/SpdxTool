using System.Text.RegularExpressions;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestQuery
{
    [TestMethod]
    public void QueryCommandMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'query' command missing arguments"));
    }

    [TestMethod]
    public void QueryCommandBadPattern()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            "pattern",
            "dotnet",
            "--version");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("Pattern must contain a 'value' capture group"));
    }

    [TestMethod]
    public void QueryCommandInvalidProgram()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            @"(?<value>\d+\.\d+\.\d+)",
            "does-not-exist");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains(@"Pattern '(?<value>\d+\.\d+\.\d+)' not found in program output"));
    }

    [TestMethod]
    public void QueryDotNetConsole()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "query",
            @"(?<value>\d+\.\d+\.\d+)",
            "dotnet",
            "--version");

        // Verify error reported
        Assert.AreEqual(0, exitCode);
        Assert.IsTrue(Regex.IsMatch(output, @"\d+\.\d+\.\d+"));
    }

    [TestMethod]
    public void QueryDotNetWorkflow()
    {
        // Workflow contents
        const string workflowContents = "parameters:\n" +
                                        "  version: unknown\n" +
                                        "" +
                                        "steps:\n" +
                                        "- command: query\n" +
                                        "  inputs:\n" +
                                        "    output: version\n" +
                                        "    pattern: (?<value>\\d+\\.\\d+\\.\\d+)\n" +
                                        "    command: dotnet --version\n" +
                                        "\n" +
                                        "- command: print\n" +
                                        "  inputs:\n" +
                                        "    text:\n" +
                                        "    - ${{ version }}";

        try
        {
            // Write the SPDX files
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify success
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(Regex.IsMatch(output, @"\d+\.\d+\.\d+"));
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }
}