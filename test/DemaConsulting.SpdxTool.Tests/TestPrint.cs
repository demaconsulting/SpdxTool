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

/// <summary>
/// Tests for the 'print' command
/// </summary>
[TestClass]
public class TestPrint
{
    /// <summary>
    /// Tests the 'print' command from the command line
    /// </summary>
    [TestMethod]
    public void Print_CommandLine()
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
        StringAssert.Contains(output, "Hello, World!");
    }

    /// <summary>
    /// Tests the 'print' command from a workflow
    /// </summary>
    [TestMethod]
    public void Print_Workflow()
    {
        // Workflow contents
        const string workflowContents = 
            """
            parameters:
              p1: Hello
              p2: World
            
            steps:
            - command: print
              inputs:
                text:
                - The first parameter is ${{ p1 }}.
                - ${{ p2 }} is the second parameter.
            """;

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
            StringAssert.Contains(output, "The first parameter is Hello.");
            StringAssert.Contains(output, "World is the second parameter.");
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }
}