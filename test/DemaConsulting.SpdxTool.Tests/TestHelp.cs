namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestHelp
{
    [TestMethod]
    public void HelpMissingArguments()
    {
        // Run the help command with no arguments
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help");

        // Verify an error was detected
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'help' command missing arguments"));
    }

    [TestMethod]
    public void HelpUnknownCommand()
    {
        // Run the help command with an unknown command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "unknown-command");

        // Verify an error was detected
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("Unknown command: 'unknown-command'"));
    }

    [TestMethod]
    public void HelpRunWorkflow()
    {
        // Run the help command with an unknown command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "help",
            "run-workflow");

        // Verify success
        Assert.AreEqual(0, exitCode);
        Assert.IsTrue(output.Contains("This command runs the steps specified in the workflow.yaml file"));
    }
}