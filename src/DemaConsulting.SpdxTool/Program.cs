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
    public static string Version =>
        typeof(Program)
            .Assembly
            .GetCustomAttributes(false)
            .OfType<AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()
            ?.InformationalVersion ?? "Unknown";

    /// <summary>
    /// Application entry point
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // Handle querying for version
        if (args.Length == 1 && (args[0] == "-v" || args[0] == "--version"))
        {
            Console.WriteLine(Version);
            Environment.Exit(0);
        }

        // Print version banner
        Console.WriteLine($"DemaConsulting.SpdxTool {Version}");
        Console.WriteLine();

        // Handle printing usage information
        if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
        {
            PrintUsage();
            Environment.Exit(1);
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unknown command: '{args[0]}'");
                Console.ResetColor();
                Console.WriteLine();
                PrintUsage();
                Environment.Exit(1);
            }
        }
        catch (CommandUsageException ex)
        {
            // Report usage exception and usage information
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
            PrintUsage();
            Environment.Exit(1);
        }
        catch (CommandErrorException ex)
        {
            // Report error exception
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            // Report unknown exception
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{ex}");
            Console.ResetColor();
            Environment.Exit(1);
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
}