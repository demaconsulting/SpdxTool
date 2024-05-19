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
            "dotnet",
            "DemaConsulting.SpdxTool.dll");

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
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
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
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--help");

        // Verify an error was reported
        Assert.AreEqual(1, exitCode);

        // Verify the output contains the usage information
        Assert.IsTrue(output.Contains("Usage: spdx-tool"));
    }
}