using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Set Variable Command
/// </summary>
public class SetVariable : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly SetVariable Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "set-variable",
        "set-variable",
        "Set workflow variable (workflow only).",
        new[]
        {
            "This command sets a workflow variable.",
            "",
            "  - command: set-variable",
            "    inputs:",
            "      value: <value>                # New value",
            "      output: <variable>            # Variable to set"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private SetVariable()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        throw new CommandUsageException("'set-variable' command is only valid in a workflow");
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'value' input
        var value = GetMapString(inputs, "value", variables) ??
                    throw new YamlException(step.Start, step.End, "'set-variable' command missing 'value' input");

        // Get the 'output' input
        var output = GetMapString(inputs, "output", variables) ??
                     throw new YamlException(step.Start, step.End, "'set-variable' command missing 'output' input");

        // Save the value to the variables
        variables[output] = value;
    }
}