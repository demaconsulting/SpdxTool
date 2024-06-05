using DemaConsulting.SpdxTool.Spdx;
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
        "get-version <spdx.json> <id>",
        "Get the version of an SPDX package.",
        new[]
        {
            "This command gets the version of an SPDX package.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool get-version <spdx.json> <id>",
            "",
            "From a YAML file this can be used as:",
            "  - command: get-version",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      id: <id>                      # Package ID",
            "      output: <variable>            # Output variable"
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
        // Report an error if the number of arguments is not 2
        if (args.Length != 2)
            throw new CommandUsageException("'get-version' command missing arguments");

        // Print the version
        Console.WriteLine(Get(args[0], args[1]));
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'get-version' command missing 'spdx' input");

        // Get the 'id' input
        var id = GetMapString(inputs, "id", variables) ??
                 throw new YamlException(step.Start, step.End, "'get-version' command missing 'id' input");

        // Get the 'output' input
        var output = GetMapString(inputs, "output", variables) ??
                     throw new YamlException(step.Start, step.End, "'get-version' command missing 'output' input");

        // Save the version
        variables[output] = Get(spdxFile, id);
    }

    /// <summary>
    /// Get the version of a package in an SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX document file name</param>
    /// <param name="id">SPDX package ID</param>
    /// <returns>Version</returns>
    /// <exception cref="CommandErrorException">on error</exception>
    public static string Get(string spdxFile, string id)
    {
        // Load the document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Find the package
        var package = Array.Find(doc.Packages, p => p.Id == id) ??
                      throw new CommandErrorException($"Package {id} not found in {spdxFile}");

        // Return the version
        return package.Version ??
               throw new CommandErrorException($"Package {id} in {spdxFile} has no version");
    }
}