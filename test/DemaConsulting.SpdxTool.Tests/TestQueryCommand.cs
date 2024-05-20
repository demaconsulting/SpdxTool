using System.Text.RegularExpressions;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestQueryCommand
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
        Assert.IsTrue(output.Contains("Unable to start program 'does-not-exist'"));
    }

    [TestMethod]
    public void QueryDotNet()
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
}