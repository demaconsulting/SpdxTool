using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command to display extended help about a command
/// </summary>
public class HelpCommand : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly HelpCommand Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "help",
        "help <command>",
        "Display extended help about a command",
        new[]
        {
            "This command displays extended help information about the specified command.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool help <command>",
            "",
            "From a YAML file this can be used as:",
            "  - command: help",
            "    inputs:",
            "      about: <command>"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private HelpCommand()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is not 1
        if (args.Length != 1)
            throw new CommandUsageException("'help' command missing arguments");

        // Generate the markdown
        ShowUsage(args[0]);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'about' input
        var about = GetMapString(inputs, "about", variables) ??
                    throw new YamlException(step.Start, step.End, "'help' command missing 'about' input");

        // Generate the markdown
        ShowUsage(about);
    }

    /// <summary>
    /// Show the usage for the requested command
    /// </summary>
    /// <param name="command"></param>
    /// <exception cref="CommandUsageException">On error</exception>
    public static void ShowUsage(string command)
    {
        // Get the entry for the command
        if (!CommandsRegistry.Commands.TryGetValue(command, out var entry))
            throw new CommandUsageException($"Unknown command: '{command}'");

        // Display the command entry
        foreach (var line in entry.Details)
            Console.WriteLine(line);
    }
}