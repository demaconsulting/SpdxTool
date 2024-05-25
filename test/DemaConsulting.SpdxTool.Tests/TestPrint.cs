namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public class TestPrint
{
    [TestMethod]
    public void PrintConsole()
    {
        // Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "print",
            "Hello, World!");

        // Verify output
        Assert.AreEqual(0, exitCode);
        Assert.AreEqual("Hello, World!", output.Trim());
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
                                        "- command: print\n" +
                                        "  inputs:\n" +
                                        "    text:\n" +
                                        "    - The first parameter is ${{ p1 }}." +
                                        "    - ${{ p2 }} is the second parameter.";

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
            Assert.IsTrue(output.Contains("The first parameter is Hello."));
            Assert.IsTrue(output.Contains("World is the second parameter."));
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }
}