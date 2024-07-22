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

using DemaConsulting.SpdxTool.Spdx;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Update a package in an SPDX document
/// </summary>
public class UpdatePackage : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly UpdatePackage Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "update-package",
        "update-package",
        "Update package in SPDX document (workflow only).",
        new[]
        {
            "This command updates a package in an SPDX document.",
            "",
            "  - command: update-package",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX filename",
            "      package:                      # Package information",
            "        id: <id>                    # Package ID",
            "        name: <name>                # Optional new package name",
            "        download: <download-url>    # Optional new package download URL",
            "        version: <version>          # Optional new package version",
            "        filename: <filename>        # Optional new package filename",
            "        supplier: <supplier>        # Optional new package supplier",
            "        originator: <originator>    # Optional new package originator",
            "        homepage: <homepage>        # Optional new package homepage",
            "        copyright: <copyright>      # Optional new package copyright",
            "        summary: <summary>          # Optional new package summary",
            "        description: <description>  # Optional new package description",
            "        license: <license>          # Optional new package license"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private UpdatePackage()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        throw new CommandUsageException("'update-package' command is only valid in a workflow");
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'update-package' missing 'spdx' input");

        // Parse the package
        var packageMap = GetMapMap(inputs, "package") ??
                         throw new YamlException(step.Start, step.End, "'update-package' missing 'package' input");

        // Get the package 'id' input
        var packageId = GetMapString(packageMap, "id", variables) ??
                        throw new YamlException(step.Start, step.End, "'update-package' missing 'package.id' input");

        // Get the updates
        var updates = new Dictionary<string, string>();
        ParseUpdates(packageMap, variables, updates);

        // Update the package
        UpdatePackageInSpdxFile(spdxFile, packageId, updates);
    }

    /// <summary>
    /// Update a package in an SPDX document file
    /// </summary>
    /// <param name="spdxFile">SPDX document filename</param>
    /// <param name="packageId">Package ID</param>
    /// <param name="updates">Update information</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    /// <exception cref="CommandErrorException">On error</exception>
    public static void UpdatePackageInSpdxFile(string spdxFile, string packageId, Dictionary<string, string> updates)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Find the package
        var package = Array.Find(doc.Packages, p => p.Id == packageId) ??
                      throw new CommandErrorException($"Package '{packageId}' not found in {spdxFile}");

        // Update the package
        foreach (var (key, value) in updates)
        {
            switch (key)
            {
                case "name":
                    package.Name = value;
                    break;
                case "download":
                    package.DownloadLocation = value;
                    break;
                case "version":
                    package.Version = value;
                    break;
                case "filename":
                    package.FileName = value;
                    break;
                case "supplier":
                    package.Supplier = value;
                    break;
                case "originator":
                    package.Originator = value;
                    break;
                case "homepage":
                    package.HomePage = value;
                    break;
                case "copyright":
                    package.CopyrightText = value;
                    break;
                case "summary":
                    package.Summary = value;
                    break;
                case "description":
                    package.Description = value;
                    break;
                case "license":
                    package.ConcludedLicense = value;
                    package.DeclaredLicense = value;
                    break;
                default:
                    throw new CommandErrorException($"Invalid package update key '{key}'");
            }
        }

        // Save the SPDX document
        SpdxHelpers.SaveJsonDocument(doc, spdxFile);
    }

    /// <summary>
    /// Read the package criteria from the inputs
    /// </summary>
    /// <param name="map">Criteria map</param>
    /// <param name="variables">Currently defined variables</param>
    /// <param name="updates">Criteria dictionary to populate</param>
    public static void ParseUpdates(
        YamlMappingNode? map,
        Dictionary<string, string> variables,
        Dictionary<string, string> updates)
    {
        // Get the 'name' input
        var name = GetMapString(map, "name", variables);
        if (name != null)
            updates["name"] = name;

        // Get the 'download' input
        var download = GetMapString(map, "download", variables);
        if (download != null)
            updates["download"] = download;

        // Get the 'version' input
        var version = GetMapString(map, "version", variables);
        if (version != null)
            updates["version"] = version;

        // Get the 'filename' input
        var filename = GetMapString(map, "filename", variables);
        if (filename != null)
            updates["filename"] = filename;

        // Get the 'supplier' input
        var supplier = GetMapString(map, "supplier", variables);
        if (supplier != null)
            updates["supplier"] = supplier;

        // Get the 'originator' input
        var originator = GetMapString(map, "originator", variables);
        if (originator != null)
            updates["originator"] = originator;

        // Get the 'homepage' input
        var homepage = GetMapString(map, "homepage", variables);
        if (homepage != null)
            updates["homepage"] = homepage;

        // Get the 'copyright' input
        var copyright = GetMapString(map, "copyright", variables);
        if (copyright != null)
            updates["copyright"] = copyright;

        // Get the 'summary' input
        var summary = GetMapString(map, "summary", variables);
        if (summary != null)
            updates["summary"] = summary;

        // Get the 'description' input
        var description = GetMapString(map, "description", variables);
        if (description != null)
            updates["description"] = description;

        // Get the 'license' input
        var license = GetMapString(map, "license", variables);
        if (license != null)
            updates["license"] = license;
    }
}