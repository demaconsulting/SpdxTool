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
using Validate = DemaConsulting.SpdxTool.SelfTest.Validate;

namespace DemaConsulting.SpdxTool;

/// <summary>
///     Entry point and top-level dispatcher for the SPDX tool command-line application.
/// </summary>
/// <remarks>
///     Constructs a Context from the command-line arguments and dispatches execution to the version
///     reporter, help printer, self-validation suite, or the matching registered command. All mutable
///     runtime state is held in Context so Program methods are stateless and independently testable.
/// </remarks>
public static class Program
{
    /// <summary>
    ///     Gets the version of this assembly.
    /// </summary>
    /// <remarks>
    ///     Reads <see cref="AssemblyInformationalVersionAttribute"/> from the entry assembly at
    ///     startup. Falls back to <c>"Unknown"</c> when the attribute is absent (e.g., in unit-test
    ///     host processes). The value is determined once at class initialization and is read-only
    ///     thereafter.
    /// </remarks>
    public static readonly string Version =
        typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "Unknown";

    /// <summary>
    ///     Application entry point
    /// </summary>
    /// <param name="args">Command-line arguments. May be empty (triggers error and usage output); must not be null.</param>
    /// <remarks>
    ///     InvalidOperationException from Context.Create (e.g., missing argument, invalid depth,
    ///     negative depth) is caught and reported as 'Error: {message}' with exit code 1. All other
    ///     unhandled exceptions are reported (message only, no stack trace) and re-thrown.
    ///     Environment.ExitCode is set from context.ExitCode (1 if any errors were recorded, 0 otherwise).
    /// </remarks>
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
            Console.WriteLine($"Error: {e.Message}");
            Console.ResetColor();
            throw;
        }
    }

    /// <summary>
    ///     Run the program context
    /// </summary>
    /// <param name="context">Program context</param>
    /// <remarks>
    ///     Each call to context.WriteError increments the error counter; the caller should check
    ///     context.ExitCode after Run returns to determine whether any errors occurred.
    /// </remarks>
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
    /// <remarks>
    ///     Shared by multiple error paths (missing arguments, unknown command, usage exception)
    ///     and the help flag handler so that usage output is consistent regardless of the trigger.
    /// </remarks>
    public static void PrintUsage(Context context)
    {
        context.WriteLine(
            """
            Usage: spdx-tool [options] <command> [arguments]

            Options:
              -h, -?, --help                           Show this help message and exit
              -v, --version                            Show version information and exit
              -l, --log <log-file>                     Log output to file
              -s, --silent                             Silence console output
                  --validate                           Perform self-validation
              -r, --result <file>                      Self-validation result file (.trx or .xml for JUnit)
                  --depth <level>                      Self-validation report depth level

            Commands:
            """);
        foreach (var command in CommandsRegistry.Commands.Values)
        {
            context.WriteLine($"  {command.CommandLine,-40} {command.Summary}");
        }
    }
}
