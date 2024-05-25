using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Print command
/// </summary>
public class Print : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly Print Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "print",
        "print <text>",
        "Print text to the console",
        new[]
        {
            "This command prints text to the console.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool print <text>",
            "",
            "From a YAML file this can be used as:",
            "  - command: help",
            "    inputs:",
            "      text:" +
            "      - Some text to print" +
            "      - The value of variable is ${{ variable }}"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private Print()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        foreach (var arg in args)
            Console.WriteLine(arg);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'text' input
        var text = GetMapSequence(inputs, "text") ??
                    throw new YamlException(step.Start, step.End, "'print' command missing 'text' input");

        // Write all text
        for (var i = 0; i < text.Children.Count; i++)
        {
            var line = GetSequenceString(text, i, variables) ?? string.Empty;
            Console.WriteLine(line);
        }
    }
}