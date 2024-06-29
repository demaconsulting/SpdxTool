using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Get Version Command
/// </summary>
public class GetVersion : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly GetVersion Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "get-version",
        "get-version <spdx.json> <criteria>",
        "Get the version of an SPDX package.",
        new[]
        {
            "This command gets the version of an SPDX package.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool get-version <spdx.json> [criteria]",
            "",
            "  The supported criteria are:",
            "    id=<id>                         # Optional package ID header",
            "    name=<name>                     # Optional package name header",
            "    version=<version>               # Optional package version header",
            "    filename=<filename>             # Optional package filename header",
            "    download=<url>                  # Optional package download URL header",
            "",
            "From a YAML file this can be used as:",
            "  - command: get-version",
            "    inputs:",
            "      output: <variable>            # Output variable",
            "      spdx: <spdx.json>             # SPDX file name",
            "      id: <id>                      # Optional package ID header",
            "      name: <name>                  # Optional package name header",
            "      version: <version>            # Optional package version header",
            "      filename: <filename>          # Optional package filename header",
            "      download: <url>               # Optional package download URL header"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private GetVersion()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if insufficient arguments
        if (args.Length < 2)
            throw new CommandUsageException("'get-version' command missing arguments");

        // Parse the arguments
        var spdxFile = args[0];
        var criteria = new Dictionary<string, string>();
        FindPackage.ParseCriteria(args.Skip(1), criteria);

        // Find the package version
        var packageVersion = FindPackage.FindPackageByCriteria(spdxFile, criteria)?.Version ??
                             throw new CommandErrorException($"Package not found in {spdxFile} matching search criteria");

        // Print the version
        Console.WriteLine(packageVersion);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'get-version' command missing 'spdx' input");

        // Get the criteria
        var criteria = new Dictionary<string, string>();
        FindPackage.ParseCriteria(inputs, variables, criteria);

        // Find the package version
        var packageVersion = FindPackage.FindPackageByCriteria(spdxFile, criteria)?.Version ??
                        throw new CommandErrorException($"Package not found in {spdxFile} matching search criteria");

        // Get the 'output' input
        var output = GetMapString(inputs, "output", variables) ??
                     throw new YamlException(step.Start, step.End, "'get-version' command missing 'output' input");

        // Save the version
        variables[output] = packageVersion;
    }
}