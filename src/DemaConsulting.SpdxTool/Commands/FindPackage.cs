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

using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxTool.Spdx;
using DemaConsulting.SpdxTool.Utility;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Find the ID of a package in an SPDX file
/// </summary>
public sealed class FindPackage : Command
{
    /// <summary>
    /// Command name
    /// </summary>
    private const string Command = "find-package";

    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly FindPackage Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "find-package <spdx.json> <criteria>",
        "Find package ID in SPDX document",
        [
            "This command finds the package ID for a package in an SPDX document.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool find-package <spdx.json> [criteria]",
            "",
            "  The supported criteria are:",
            "    id=<id>                         # Optional package ID header",
            "    name=<name>                     # Optional package name header",
            "    version=<version>               # Optional package version header",
            "    filename=<filename>             # Optional package filename header",
            "    download=<url>                  # Optional package download URL header",
            "",
            "From a YAML file this can be used as:",
            "  - command: find-package",
            "    inputs:",
            "      output: <variable>            # Output variable for package ID",
            "      spdx: <spdx.json>             # SPDX file name",
            "      id: <id>                      # Optional package ID header",
            "      name: <name>                  # Optional package name header",
            "      version: <version>            # Optional package version header",
            "      filename: <filename>          # Optional package filename header",
            "      download: <url>               # Optional package download URL header"
        ],
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private FindPackage()
    {
    }

    /// <inheritdoc />
    public override void Run(Context context, string[] args)
    {
        // Report an error if insufficient arguments
        if (args.Length < 2)
            throw new CommandUsageException("'find-package' command missing arguments");

        // Parse the arguments
        var spdxFile = args[0];
        var criteria = new Dictionary<string, string>();
        ParseCriteria(args.Skip(1), criteria);
    
        // Find the package ID
        var packageId = FindPackageByCriteria(spdxFile, criteria).Id;

        // Write the package ID to the console
        context.WriteLine(packageId);
    }

    /// <inheritdoc />
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
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
        var packageId = FindPackageByCriteria(spdxFile, criteria).Id;

        // Save the package ID to the variables
        variables[output] = packageId;
    }

    /// <summary>
    /// Parse the package criteria from the arguments
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <param name="criteria">Criteria dictionary to populate</param>
    /// <exception cref="CommandUsageException">on error</exception>
    public static void ParseCriteria(
        IEnumerable<string> args,
        Dictionary<string, string> criteria)
    {
        foreach (var arg in args)
        {
            // Split the argument into key and value
            var parts = arg.Split('=', 2);
            if (parts.Length != 2)
                throw new CommandUsageException($"Invalid criteria '{arg}'");

            // Add to the criteria
            criteria[parts[0]] = parts[1];
        }
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
        // Get the 'id' input
        var id = GetMapString(map, "id", variables);
        if (id != null)
            criteria["id"] = id;

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
    public static SpdxPackage FindPackageByCriteria(string spdxFile, IReadOnlyDictionary<string, string> criteria)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Find the packages
        var matches = doc.Packages.Where(p => IsPackageMatch(p, criteria)).ToArray();

        // Return the package
        return matches.Length switch
        {
            0 => throw new CommandErrorException($"Package not found in {spdxFile} matching search criteria"),
            1 => matches[0],
            _ => throw new CommandErrorException($"Multiple packages found in {spdxFile} matching search criteria")
        };
    }

    /// <summary>
    /// Test if the package matches the given criteria
    /// </summary>
    /// <param name="package">Package to match</param>
    /// <param name="criteria">Criteria</param>
    /// <returns></returns>
    public static bool IsPackageMatch(SpdxPackage package, IReadOnlyDictionary<string, string> criteria)
    {
        // Check the id
        if (criteria.TryGetValue("id", out var id) && !Wildcard.IsMatch(package.Id, id))
            return false;

        // Check the name
        if (criteria.TryGetValue("name", out var name) && !Wildcard.IsMatch(package.Name, name))
            return false;

        // Check the version
        if (criteria.TryGetValue("version", out var version) && (package.Version == null || !Wildcard.IsMatch(package.Version, version)))
            return false;

        // Check the filename
        if (criteria.TryGetValue("filename", out var filename) && (package.FileName == null || !Wildcard.IsMatch(package.FileName, filename)))
            return false;

        // Check the download location
        if (criteria.TryGetValue("download", out var download) && !Wildcard.IsMatch(package.DownloadLocation, download))
            return false;

        // Package matches all specified criteria
        return true;
    }
}