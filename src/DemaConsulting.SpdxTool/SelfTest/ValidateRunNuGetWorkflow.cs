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

using DemaConsulting.TestResults;

namespace DemaConsulting.SpdxTool.SelfTest;

/// <summary>
///     Self-validation of RunWorkflow with NuGet package
/// </summary>
internal static class ValidateRunNuGetWorkflow
{
    /// <summary>
    ///     Run validation test
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="results">Test results</param>
    public static void Run(Context context, TestResults.TestResults results)
    {
        var passed = DoValidate();

        // Report validation result
        if (passed)
        {
            context.WriteLine($"✓ SpdxTool_RunNuGetWorkflow - Passed");
        }
        else
        {
            context.WriteError($"✗ SpdxTool_RunNuGetWorkflow - Failed");
        }

        results.Results.Add(
            new TestResult
            {
                Name = "SpdxTool_RunNuGetWorkflow",
                ClassName = "DemaConsulting.SpdxTool.SelfTest.ValidateRunNuGetWorkflow",
                ComputerName = Environment.MachineName,
                StartTime = DateTime.Now,
                Outcome = passed ? TestOutcome.Passed : TestOutcome.Failed
            });
    }

    /// <summary>
    ///     Do the validation
    /// </summary>
    /// <returns>True on success</returns>
    private static bool DoValidate()
    {
        try
        {
            // Create the temporary validation folder
            Directory.CreateDirectory("validate.tmp");

            // Write test workflow file that runs the GetDotNetVersion workflow from NuGet
            File.WriteAllText("validate.tmp/workflow.yaml",
                """
                steps:
                - command: run-workflow
                  inputs:
                    nuget: "DemaConsulting.SpdxWorkflows:1.0.0"
                    file: "contentFiles/any/any/workflows/GetDotNetVersion.yaml"
                    outputs:
                      version: dotnet-version

                - command: print
                  inputs:
                    text:
                    - DotNet version is ${{ dotnet-version }}
                """);

            // Run the workflow file
            var exitCode = Validate.RunSpdxTool(
                "validate.tmp",
                [
                    "--silent",
                    "run-workflow",
                    "workflow.yaml"
                ]);

            // Fail if SpdxTool reported an error
            return exitCode == 0;
        }
        finally
        {
            // Delete the temporary validation folder
            Directory.Delete("validate.tmp", true);
        }
    }
}
