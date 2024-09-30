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

using System.Reflection;
using DemaConsulting.SpdxTool.Commands;

namespace DemaConsulting.SpdxTool;

/// <summary>
/// Program class
/// </summary>
public static class Program
{
    /// <summary>
    /// Gets the version of this assembly.
    /// </summary>
    public static readonly string Version =
        typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "Unknown";

    /// <summary>
    /// Application entry point
    /// </summary>
    /// <param name="args">Program arguments</param>
    public static void Main(string[] args)
    {
        // Handle querying for version
        if (args.Length == 1 && (args[0] == "-v" || args[0] == "--version"))
        {
            Console.WriteLine(Version);
            return;
        }

        // Print version banner
        Console.WriteLine($"DemaConsulting.SpdxTool {Version}\n");

        // Fail if no arguments specified
        if (args.Length == 0)
        {
            ReportError("No arguments specified");
            PrintUsage();
            return;
        }

        // Handle printing usage information
        if (args[0] == "-h" || args[0] == "--help")
        {
            PrintUsage();
            return;
        }

        try
        {
            if (CommandsRegistry.Commands.TryGetValue(args[0], out var entry))
            {
                // Run the command
                entry.Instance.Run(args.Skip(1).ToArray());
            }
            else
            {
                // Report unknown command
                ReportError($"Unknown command: '{args[0]}'");
                PrintUsage();
            }
        }
        catch (CommandUsageException ex)
        {
            // Report usage exception and usage information
            ReportError(ex.Message);
            PrintUsage();
        }
        catch (CommandErrorException ex)
        {
            // Report error exception
            ReportError(ex.Message);
        }
        catch (Exception ex)
        {
            // Report unknown exception
            ReportError(ex.ToString());
        }
    }

    /// <summary>
    /// Print usage information
    /// </summary>
    public static void PrintUsage()
    {
        Console.WriteLine("Usage: spdx-tool [options] <command> [arguments]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -h, --help                               Show this help message and exit");
        Console.WriteLine("  -v, --version                            Show version information and exit");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        foreach (var command in CommandsRegistry.Commands.Values)
            Console.WriteLine($"  {command.CommandLine,-40} {command.Summary}");
    }

    /// <summary>
    /// Report an error message to the console in red
    /// </summary>
    /// <param name="message">Error message</param>
    private static void ReportError(string message)
    {
        // Write an error message to the console
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
        Console.ResetColor();
        Console.WriteLine();

        // Set the exit code to 1 as an error has occurred
        Environment.ExitCode = 1;
    }
}