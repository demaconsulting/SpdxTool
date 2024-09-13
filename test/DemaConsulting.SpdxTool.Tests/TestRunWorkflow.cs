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

using System.Text.RegularExpressions;

namespace DemaConsulting.SpdxTool.Tests;

[TestClass]
public partial class TestRunWorkflow
{
    /// <summary>
    /// Regular expression to check for dotnet version
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"DotNet version is \d+\.\d+\.\d+")]
    private static partial Regex DotnetVersionRegex();

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
        StringAssert.Contains(output, "'run-workflow' command missing arguments");
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
        StringAssert.Contains(output, "File not found: does-not-exist.yaml");
    }

    [TestMethod]
    public void RunWorkflowFileInvalid()
    {
        const string fileContents =
            "missing-steps: 123";

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
            StringAssert.Contains(output, "Error: Workflow invalid.yaml missing steps");
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
        const string fileContents =
            """
            steps:
            - command: help
            """;

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
            StringAssert.Contains(output, "'help' command missing 'about' input");
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
        const string fileContents = 
            """
            steps:
            - command: help
              inputs:
                about: help
            """;

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
            StringAssert.Contains(output, "This command displays extended help information about the specified command");
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
        const string fileContents = 
            """
            parameters:
              about: help
            
            steps:
            - command: help
              inputs:
                about: ${{ about }}
            """;

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
            StringAssert.Contains(output, "This command displays extended help information about the specified command");
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
        const string fileContents = 
            """
            parameters:
              about: help
            
            steps:
            - command: help
              inputs:
                about: ${{ about }}
            """;

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
            StringAssert.Contains(output, "This command produces a Markdown summary of an SPDX document");
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
        const string workflow1 = 
            """
            parameters:
              arg: unknown
            
            steps:
            - command: run-workflow
              inputs:
                file: workflow2.yaml
                integrity: 7c8cbebe55ab1094e513bd50e05823820c4b0229c19d4e8edfbfa3a3765b2be2
                parameters:
                  in: ${{ arg }}
                outputs:
                  out: out-var
            
            - command: print
              inputs:
                text:
                - Output is ${{ out-var }}
            """;

        // Workflow2 file with exact string representation
        const string workflow2 = 
            "parameters:\n" +
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
            StringAssert.Contains(output, "Output is Got Fred Param");
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
        const string workflow1 = 
            """
            parameters:
              arg: unknown
            
            steps:
            - command: run-workflow
              inputs:
                file: workflow2.yaml
                integrity: 0000000000000000000000000000000000000000000000000000000000000000
                parameters:
                  in: ${{ arg }}
                outputs:
                  out: out-var
            
            - command: print
              inputs:
                text:
                - Output is ${{ out-var }}
            """;

        const string workflow2 = 
            """
            parameters:
              in: unknown
            
            steps:
            - command: set-variable
              inputs:
                value: Got ${{ in }} Param
                output: out
            """;

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
            StringAssert.Contains(output, "Error: Integrity check of workflow2.yaml failed");
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
        const string workflow = 
            """
            steps:
            - command: run-workflow
              inputs:
                url: 'https://raw.githubusercontent.com/demaconsulting/SpdxWorkflows/main/GetDotNetVersion.yaml'
                outputs:
                  version: dotnet-version
            
            - command: print
              inputs:
                text:
                - DotNet version is ${{ dotnet-version }}
            """;

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
            StringAssert.Matches(output, DotnetVersionRegex());
        }
        finally
        {
            // Delete the files
            File.Delete("workflow.yaml");
        }
    }
}