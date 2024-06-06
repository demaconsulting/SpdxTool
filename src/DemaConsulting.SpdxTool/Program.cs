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
            // Report usage exception
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
            PrintUsage();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            // Report exception
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex}");
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