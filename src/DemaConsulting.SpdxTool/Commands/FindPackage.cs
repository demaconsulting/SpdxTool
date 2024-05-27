using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxTool.Spdx;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Find the ID of a package in an SPDX file
/// </summary>
public class FindPackage : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly FindPackage Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "find-package",
        "find-package <spdx.json> [criteria]",
        "Find package ID in SPDX document",
        new[]
        {
            "This command finds the package ID for a package in an SPDX document.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool find-package <spdx.json> [criteria]",
            "",
            "  The supported criteria are:",
            "    name=<name>                     # Optional package name",
            "    version=<version>               # Optional package version",
            "    filename=<filename>             # Optional package filename",
            "    download=<url>                  # Optional package download URL",
            "",
            "From a YAML file this can be used as:",
            "  - command: find-package",
            "    inputs:",
            "      output: <variable>            # Output variable for package ID",
            "      spdx: <spdx.json>             # SPDX file name",
            "      name: <name>                  # Optional package name",
            "      version: <version>            # Optional package version",
            "      filename: <filename>          # Optional package filename",
            "      download: <url>               # Optional package download URL"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private FindPackage()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is not 1
        if (args.Length < 2)
            throw new CommandUsageException("'find-package' command missing arguments");

        // Parse the criteria
        var criteria = new Dictionary<string, string>();
        foreach (var arg in args.Skip(1))
        {
            // Split the argument into key and value
            var parts = arg.Split('=', 2);
            if (parts.Length != 2)
                throw new CommandUsageException($"Invalid criteria '{arg}'");

            // Add to the criteria
            criteria[parts[0]] = parts[1];
        }
    
        // Find the package ID
        var spdxFile = args[0];
        var packageId = FindPackageByCriteria(spdxFile, criteria)?.Id ?? 
                        throw new CommandErrorException($"Package not found in {spdxFile} matching search criteria");

        // Write the package ID to the console
        Console.WriteLine(packageId);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'output' input
        var output = GetMapString(inputs, "output", variables) ??
                     throw new YamlException(step.Start, step.End, "'find-package' command missing 'output' input");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'find-package' missing 'spdx' input");

        // Get the criteria
        var criteria = new Dictionary<string, string>();
        ParseCriteria(inputs, variables, criteria);

        // Find the package ID
        var packageId = FindPackageByCriteria(spdxFile, criteria)?.Id ??
                        throw new CommandErrorException($"Package not found in {spdxFile} matching search criteria");

        // Save the package ID to the variables
        variables[output] = packageId;
    }

    /// <summary>
    /// Read the package criteria from the inputs
    /// </summary>
    /// <param name="map">Criteria map</param>
    /// <param name="variables">Currently defined variables</param>
    /// <param name="criteria">Criteria dictionary to populate</param>
    public static void ParseCriteria(
        YamlMappingNode? map,
        Dictionary<string, string> variables,
        Dictionary<string, string> criteria)
    {
        // Get the 'name' input
        var name = GetMapString(map, "name", variables);
        if (name != null)
            criteria["name"] = name;

        // Get the 'version' input
        var version = GetMapString(map, "version", variables);
        if (version != null)
            criteria["version"] = version;

        // Get the 'filename' input
        var filename = GetMapString(map, "filename", variables);
        if (filename != null)
            criteria["filename"] = filename;

        // Get the 'download' input
        var download = GetMapString(map, "download", variables);
        if (download != null)
            criteria["download"] = download;
    }

    /// <summary>
    /// Find the package in the SPDX document matching the specified criteria
    /// </summary>
    /// <param name="spdxFile">SPDX document filename</param>
    /// <param name="criteria">Search criteria</param>
    /// <returns>SPDX package or null</returns>
    /// <exception cref="CommandUsageException"></exception>
    public static SpdxPackage? FindPackageByCriteria(string spdxFile, IReadOnlyDictionary<string, string> criteria)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Find the package
        return FindPackageByCriteria(doc, criteria);
    }

    /// <summary>
    /// Find the package in the SPDX document matching the specified criteria
    /// </summary>
    /// <param name="doc">SPDX document</param>
    /// <param name="criteria">Search criteria</param>
    /// <returns>SPDX package or null</returns>
    public static SpdxPackage? FindPackageByCriteria(SpdxDocument doc, IReadOnlyDictionary<string, string> criteria)
    {
        // Find the package
        return Array.Find(doc.Packages, p => IsPackageMatch(p, criteria));
    }

    /// <summary>
    /// Test if the package matches the given criteria
    /// </summary>
    /// <param name="package">Package to match</param>
    /// <param name="criteria">Criteria</param>
    /// <returns></returns>
    public static bool IsPackageMatch(SpdxPackage package, IReadOnlyDictionary<string, string> criteria)
    {
        // Check the name
        if (criteria.TryGetValue("name", out var name) && !package.Name.StartsWith(name))
            return false;

        // Check the version
        if (criteria.TryGetValue("version", out var version) && (package.Version == null || !package.Version.StartsWith(version)))
            return false;

        // Check the filename
        if (criteria.TryGetValue("filename", out var filename) && (package.FileName == null || !package.FileName.StartsWith(filename)))
            return false;

        // Check the download location
        if (criteria.TryGetValue("download", out var download) && !package.DownloadLocation.StartsWith(download))
            return false;

        // Package matches all specified criteria
        return true;
    }
}