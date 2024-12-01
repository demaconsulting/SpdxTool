﻿// Copyright (c) 2024 DEMA Consulting
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
             | OS Version          | {Environment.OSVersion.VersionString,-50} |
             | DotNet Runtime      | {Environment.Version,-50} |
             | Time Stamp          | {DateTime.UtcNow,-50:u} |

             Tests:
              
             """);

        // Run validation tests
        ValidateAddPackage.Run(context);
        ValidateAddRelationship.Run(context);
        ValidateCopyPackage.Run(context);
        ValidateFindPackage.Run(context);
        ValidateGetVersion.Run(context);
        ValidateQuery.Run(context);
        ValidateRenameId.Run(context);
        ValidateUpdatePackage.Run(context);

        // If all validations succeeded (no errors) then report validation passed
        if (context.Errors == 0)
            context.WriteLine("\nValidation Passed");
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
