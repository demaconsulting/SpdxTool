using System.Text.RegularExpressions;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestVersion
{
    [TestMethod]
    public void VersionShort()
    {
        // Run the SPDX tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "-v");

        // Check the output
        Assert.AreEqual(0, exitCode);

        // Verify version response
        Assert.IsTrue(Regex.IsMatch(output, @"\d.\d.\d.*"));
    }

    [TestMethod]
    public void VersionLong()
    {
        // Run the SPDX tool
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "--version");

        // Check the output
        Assert.AreEqual(0, exitCode);

        // Verify version response
        Assert.IsTrue(Regex.IsMatch(output, @"\d.\d.\d.*"));
    }
}