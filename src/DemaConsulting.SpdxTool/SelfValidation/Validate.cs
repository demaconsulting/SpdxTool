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

using System.Runtime.InteropServices;
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.SpdxTool.SelfValidation;

/// <summary>
///     Self-validation class
/// </summary>
public static class Validate
{
    /// <summary>
    ///     Run self-validation
    /// </summary>
    /// <param name="context">Program context</param>
    public static void Run(Context context)
    {
        // Write validation header
        context.WriteLine(
            $"""
             {new string('#', context.Depth)} DemaConsulting.SpdxTool

             | Information         | Value                                              |
             | :------------------ | :------------------------------------------------- |
             | SpdxTool Version    | {Program.Version,-50} |
             | Machine Name        | {Environment.MachineName,-50} |
             | OS Version          | {RuntimeInformation.OSDescription,-50} |
             | DotNet Runtime      | {Environment.Version,-50} |
             | Time Stamp          | {DateTime.UtcNow,-50:u} |

             Tests:
              
             """);

        var results = new TestResults.TestResults
        {
            Name = $"DemaConsulting.SpdxTool Validation Results - {Program.Version}"
        };

        // Run validation tests
        ValidateAddPackage.Run(context, results);
        ValidateAddRelationship.Run(context, results);
        ValidateBasic.Run(context, results);
        ValidateCopyPackage.Run(context, results);
        ValidateDiagram.Run(context, results);
        ValidateFindPackage.Run(context, results);
        ValidateGetVersion.Run(context, results);
        ValidateHash.Run(context, results);
        ValidateNtia.Run(context, results);
        ValidateQuery.Run(context, results);
        ValidateRenameId.Run(context, results);
        ValidateToMarkdown.Run(context, results);
        ValidateUpdatePackage.Run(context, results);

        // If all validations succeeded (no errors) then report validation passed
        if (context.Errors == 0)
        {
            context.WriteLine("\nValidation Passed");
        }

        // Save test results
        if (!string.IsNullOrEmpty(context.ValidationFile))
        {
            File.WriteAllText(context.ValidationFile, TrxSerializer.Serialize(results));
        }
    }

    /// <summary>
    ///     Run SpdxTool with the specified arguments
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <returns>Exit code</returns>
    internal static int RunSpdxTool(string[] args)
    {
        // Create the context
        using var context = Context.Create(args);

        // Run SpdxTool
        Program.Run(context);

        // Return the exit code
        return context.ExitCode;
    }

    /// <summary>
    ///     Run SpdxTool in the specified folder with the specified arguments
    /// </summary>
    /// <param name="workingFolder">Working folder</param>
    /// <param name="args">Arguments</param>
    /// <returns>Exit code</returns>
    internal static int RunSpdxTool(string workingFolder, string[] args)
    {
        var cwd = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingFolder);
            return RunSpdxTool(args);
        }
        finally
        {
            Directory.SetCurrentDirectory(cwd);
        }
    }
}
