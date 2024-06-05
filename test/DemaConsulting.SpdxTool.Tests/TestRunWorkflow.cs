using System.Text.RegularExpressions;

namespace DemaConsulting.SpdxTool.Tests;

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
            Assert.IsTrue(output.Contains("Error: Workflow invalid.yaml missing steps"));
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
            Assert.IsTrue(output.Contains("'help' command missing 'about' input"));
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
                                    "  inputs:\n" +
                                    "    about: help\n";

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

    [TestMethod]
    public void RunWorkflowWithDefaultParameters()
    {
        const string fileContents = "parameters:\n" +
                                    "  about: help\n" +
                                    "\n" +
                                    "steps:\n" +
                                    "- command: help\n" +
                                    "  inputs:\n" +
                                    "    about: ${{ about }}\n";

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

    [TestMethod]
    public void RunWorkflowWithSpecifiedParameters()
    {
        const string fileContents = "parameters:\n" +
                                    "  about: help\n" +
                                    "\n" +
                                    "steps:\n" +
                                    "- command: help\n" +
                                    "  inputs:\n" +
                                    "    about: ${{ about }}\n";

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
                "help.yaml",
                "about=to-markdown");

            // Verify success
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(
                output.Contains("This command produces a Markdown summary of an SPDX document"));
        }
        finally
        {
            // Delete the file
            File.Delete("help.yaml");
        }
    }

    [TestMethod]
    public void RunWorkflowWithOutputs()
    {
        const string workflow1 = "parameters:\n" +
                                 "  arg: unknown\n" +
                                 "\n" +
                                 "steps:\n" +
                                 "- command: run-workflow\n" +
                                 "  inputs:\n" +
                                 "    file: workflow2.yaml\n" +
                                 "    integrity: 7c8cbebe55ab1094e513bd50e05823820c4b0229c19d4e8edfbfa3a3765b2be2\n" +
                                 "    parameters:\n" +
                                 "      in: ${{ arg }}\n" +
                                 "    outputs:\n" +
                                 "      out: out-var\n" +
                                 "\n" +
                                 "- command: print\n" +
                                 "  inputs:\n" +
                                 "    text:\n" +
                                 "    - Output is ${{ out-var }}\n" +
                                 "";

        const string workflow2 = "parameters:\n" +
                                 "  in: unknown\n" +
                                 "\n" +
                                 "steps:\n" +
                                 "- command: set-variable\n" +
                                 "  inputs:\n" +
                                 "    value: Got ${{ in }} Param\n" +
                                 "    output: out\n";

        try
        {
            // Write the file2
            File.WriteAllText("workflow1.yaml", workflow1);
            File.WriteAllText("workflow2.yaml", workflow2);

            // Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml",
                "arg=Fred");

            // Verify success
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("Output is Got Fred Param"));
        }
        finally
        {
            // Delete the files
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }

    [TestMethod]
    public void RunWorkflowWithBadIntegrity()
    {
        const string workflow1 = "parameters:\n" +
                                 "  arg: unknown\n" +
                                 "\n" +
                                 "steps:\n" +
                                 "- command: run-workflow\n" +
                                 "  inputs:\n" +
                                 "    file: workflow2.yaml\n" +
                                 "    integrity: 0000000000000000000000000000000000000000000000000000000000000000\n" +
                                 "    parameters:\n" +
                                 "      in: ${{ arg }}\n" +
                                 "    outputs:\n" +
                                 "      out: out-var\n" +
                                 "\n" +
                                 "- command: print\n" +
                                 "  inputs:\n" +
                                 "    text:\n" +
                                 "    - Output is ${{ out-var }}\n" +
                                 "";

        const string workflow2 = "parameters:\n" +
                                 "  in: unknown\n" +
                                 "\n" +
                                 "steps:\n" +
                                 "- command: set-variable\n" +
                                 "  inputs:\n" +
                                 "    value: Got ${{ in }} Param\n" +
                                 "    output: out\n";

        try
        {
            // Write the file2
            File.WriteAllText("workflow1.yaml", workflow1);
            File.WriteAllText("workflow2.yaml", workflow2);

            // Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml",
                "arg=Fred");

            // Verify success
            Assert.AreEqual(1, exitCode);
            Assert.IsTrue(output.Contains("Error: Integrity check of workflow2.yaml failed"));
        }
        finally
        {
            // Delete the files
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }

    [TestMethod]
    public void RunWorkflowUrl()
    {
        const string workflow = "steps:\n" +
                                "- command: run-workflow\n" +
                                "  inputs:\n" +
                                "    url: 'https://raw.githubusercontent.com/demaconsulting/SpdxWorkflows/main/GetDotNetVersion.yaml'\n" +
                                "    outputs:\n" +
                                "      version: dotnet-version\n" +
                                "\n" +
                                "- command: print\n" +
                                "  inputs:\n" +
                                "    text:\n" +
                                "    - DotNet version is ${{ dotnet-version }}\n";

        try
        {
            // Write the file
            File.WriteAllText("workflow.yaml", workflow);

            // Run the workflow
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify success
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(Regex.IsMatch(output, @"DotNet version is \d+\.\d+\.\d+"));
        }
        finally
        {
            // Delete the files
            File.Delete("workflow.yaml");
        }
    }
}