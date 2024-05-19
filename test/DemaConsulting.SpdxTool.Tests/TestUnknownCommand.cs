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
            "DemaConsulting.SpdxTool.exe",
            "unknown-command");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("Unknown command: unknown-command"));
    }
}