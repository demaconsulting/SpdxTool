namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestSetVariable
{
    [TestMethod]
    public void SetVariableCommandLine()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "set-variable");

        // Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(output.Contains("'set-variable' command is only valid in a workflow"));
    }

    [TestMethod]
    public void PrintWorkflow()
    {
        // Workflow contents
        const string workflowContents = "parameters:\n" +
                                        "  p1: Hello\n" +
                                        "  p2: World\n" +
                                        "" +
                                        "steps:\n" +
                                        "- command: set-variable\n" +
                                        "  inputs:\n" +
                                        "    value: ${{ p1 }} and ${{ p2 }}\n" +
                                        "    output: p1p2\n" +
                                        "\n" +
                                        "- command: print\n" +
                                        "  inputs:\n" +
                                        "    text:\n" +
                                        "    - p1p2 is ${{ p1p2 }}";

        try
        {
            // Write the SPDX files
            File.WriteAllText("workflow.yaml", workflowContents);

            // Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Verify success
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(output.Contains("p1p2 is Hello and World"));
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }
}