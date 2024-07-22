// Copyright (c) 2024 DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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