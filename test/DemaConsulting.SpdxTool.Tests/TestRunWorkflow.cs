namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestRunWorkflow
{
    [TestMethod]
    public void FileNotSpecified()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "DemaConsulting.SpdxTool.exe",
            "run-workflow");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("Missing workflow filename"));
    }

    [TestMethod]
    public void FileNotFound()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "DemaConsulting.SpdxTool.exe",
            "run-workflow",
            "does-not-exist.yaml");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("File not found: does-not-exist.yaml"));
    }

    [TestMethod]
    public void FileInvalid()
    {
        const string fileContents = "missing-steps: 123\n";

        try
        {
            // Write the file
            File.WriteAllText("invalid.yaml", fileContents);

            // Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "DemaConsulting.SpdxTool.exe",
                "run-workflow",
                "invalid.yaml");

            // Verify error reported
            Assert.AreEqual(1, exitCode);
            Assert.IsTrue(output.Contains("Error: Workflow invalid.yaml invalid"));
        }
        finally
        {
            // Delete the file
            File.Delete("invalid.yaml");
        }
    }
}