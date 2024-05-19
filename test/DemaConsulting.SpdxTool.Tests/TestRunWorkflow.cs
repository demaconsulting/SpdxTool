﻿namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestRunWorkflow
{
    [TestMethod]
    public void RunWorkflowMissingArguments()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "run-workflow");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'run-workflow' command missing arguments"));
    }

    [TestMethod]
    public void RunWorkflowMissingFile()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "run-workflow",
            "does-not-exist.yaml");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("File not found: does-not-exist.yaml"));
    }

    [TestMethod]
    public void RunWorkflowFileInvalid()
    {
        const string fileContents = "missing-steps: 123\n";

        try
        {
            // Write the file
            File.WriteAllText("invalid.yaml", fileContents);

            // Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
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

    [TestMethod]
    public void RunWorkflowMissingParameterInFile()
    {
        const string fileContents = "steps:\n" +
                                    "- command: help\n";

        try
        {
            // Write the file
            File.WriteAllText("invalid.yaml", fileContents);

            // Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "invalid.yaml");

            // Verify error reported
            Assert.AreEqual(1, exitCode);
            Assert.IsTrue(output.Contains("'help' command missing 'about' parameter"));
        }
        finally
        {
            // Delete the file
            File.Delete("invalid.yaml");
        }
    }

    [TestMethod]
    public void RunWorkflow()
    {
        const string fileContents = "steps:\n" +
                                    "- command: help\n" +
                                    "  about: help\n";

        try
        {
            // Write the file
            File.WriteAllText("help.yaml", fileContents);

            // Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "help.yaml");

            // Verify success
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(
                output.Contains("This command displays extended help information about the specified command"));
        }
        finally
        {
            // Delete the file
            File.Delete("help.yaml");
        }
    }
}