namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestUsage
{
    [TestMethod]
    public void UsageNoArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "DemaConsulting.SpdxTool.exe");

        // Verify an error was reported
        Assert.AreEqual(1, exitCode);

        // Verify the output contains the usage information
        Assert.IsTrue(output.Contains("Usage: spdx-tool"));
    }

    [TestMethod]
    public void UsageHelpShort()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "DemaConsulting.SpdxTool.exe",
            "-h");

        // Verify an error was reported
        Assert.AreEqual(1, exitCode);

        // Verify the output contains the usage information
        Assert.IsTrue(output.Contains("Usage: spdx-tool"));
    }

    [TestMethod]
    public void UsageHelpLong()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "DemaConsulting.SpdxTool.exe",
            "--help");

        // Verify an error was reported
        Assert.AreEqual(1, exitCode);

        // Verify the output contains the usage information
        Assert.IsTrue(output.Contains("Usage: spdx-tool"));
    }
}