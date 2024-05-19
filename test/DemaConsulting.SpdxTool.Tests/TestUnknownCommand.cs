namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestUnknownCommand
{
    [TestMethod]
    public void UnknownCommand()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "unknown-command");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("Unknown command: unknown-command"));
    }
}