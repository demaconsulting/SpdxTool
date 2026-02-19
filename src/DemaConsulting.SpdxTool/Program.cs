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
using Validate = DemaConsulting.SpdxTool.SelfValidation.Validate;

namespace DemaConsulting.SpdxTool;

/// <summary>
///     Program class
/// </summary>
public static class Program
{
    /// <summary>
    ///     Gets the version of this assembly.
    /// </summary>
    public static readonly string Version =
        typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "Unknown";

    /// <summary>
    ///     Application entry point
    /// </summary>
    /// <param name="args">Program arguments</param>
    public static void Main(string[] args)
    {
        try
        {
            using var context = Context.Create(args);
            Run(context);
            Environment.ExitCode = context.ExitCode;
        }
        catch (InvalidOperationException e)
        {
            // Report standard failure
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {e.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {e}");
            Console.ResetColor();
            throw;
        }
    }

    /// <summary>
    ///     Run the program context
    /// </summary>
    /// <param name="context">Program context</param>
    public static void Run(Context context)
    {
        // Handle version query
        if (context.Version)
        {
            context.WriteLine(Version);
            return;
        }

        // Print version banner
        context.WriteLine($"DemaConsulting.SpdxTool {Version}\n");

        // Handle help query
        if (context.Help)
        {
            PrintUsage(context);
            return;
        }

        // Handle self-validation
        if (context.Validate)
        {
            Validate.Run(context);
            return;
        }

        // Handle missing arguments
        if (context.Arguments.Count == 0)
        {
            context.WriteError("Error: Missing arguments");
            PrintUsage(context);
            return;
        }

        try
        {
            var command = context.Arguments.First();
            if (CommandsRegistry.Commands.TryGetValue(command, out var entry))
            {
                // Run the command
                entry.Instance.Run(context, [.. context.Arguments.Skip(1)]);
            }
            else
            {
                // Report unknown command
                context.WriteError($"Error: Unknown command '{command}'");
                PrintUsage(context);
            }
        }
        catch (CommandUsageException ex)
        {
            // Report usage exception and usage information
            context.WriteError($"Error: {ex.Message}");
            PrintUsage(context);
        }
        catch (CommandErrorException ex)
        {
            // Report error exception
            context.WriteError($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Report unknown exception
            context.WriteError(ex.ToString());
        }
    }

    /// <summary>
    ///     Print usage information
    /// </summary>
    /// <param name="context">Program context</param>
    public static void PrintUsage(Context context)
    {
        context.WriteLine(
            """
            Usage: spdx-tool [options] <command> [arguments]

            Options:
              -h, --help                               Show this help message and exit
              -v, --version                            Show version information and exit
              -l, --log <log-file>                     Log output to file
              -s, --silent                             Silence console output
                  --validate                           Perform self-validation
              -r, --result <file>                      Self-validation result TRX file

            Commands:
            """);
        foreach (var command in CommandsRegistry.Commands.Values)
            context.WriteLine($"  {command.CommandLine,-40} {command.Summary}");
    }
}
