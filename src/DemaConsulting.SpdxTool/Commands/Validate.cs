using DemaConsulting.SpdxTool.Spdx;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command to validate SPDX documents
/// </summary>
public class Validate : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly Validate Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "validate",
        "validate <spdx.json> [ntia]",
        "Validate SPDX document for issues",
        new[]
        {
            "This command validates an SPDX document for issues.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool validate <spdx.json> [ntia]",
            "",
            "From a YAML file this can be used as:",
            "  - command: validate",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      ntia: true                    # Optional NTIA checking"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private Validate()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if for missing arguments
        if (args.Length == 0)
            throw new CommandUsageException("'validate' command missing arguments");

        // Process the arguments
        var spdxFile = args[0];
        var ntia = args.Skip(1).Any(a => a == "ntia");
        
        // Perform validation
        DoValidate(spdxFile, ntia);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'to-markdown' command missing 'spdx' input");

        // Get the 'ntia' input
        var ntiaValue = GetMapString(inputs, "ntia", variables);
        var ntia = ntiaValue?.ToLowerInvariant() == "true";

        // Perform validation
        DoValidate(spdxFile, ntia);
    }

    /// <summary>
    /// Validate SPDX document for issues
    /// </summary>
    /// <param name="spdxFile">SPDX document file name</param>
    /// <param name="ntia">NTIA flag</param>
    /// <exception cref="CommandErrorException">on issues</exception>
    public static void DoValidate(string spdxFile, bool ntia)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Get the issues
        var issues = new List<string>();
        doc.Validate(issues, ntia);

        // Skip if no issues detected
        if (issues.Count == 0)
            return;

        // Report issues to console
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        foreach (var issue in issues)
            Console.WriteLine(issue);
        Console.ResetColor();
        Console.WriteLine();
        
        // Throw error
        throw new CommandErrorException($"Found {issues.Count} Issues in {spdxFile}");
    }
}