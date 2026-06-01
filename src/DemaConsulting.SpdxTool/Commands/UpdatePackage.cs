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
///     Update a package in an SPDX document
/// </summary>
/// <remarks>
///     UpdatePackage is a stateless singleton that implements the update-package workflow-only
///     command. It reads the spdx and package inputs from a YAML step, parses the update fields,
///     and writes the updated SPDX document. Direct CLI invocation is always rejected. This class
///     is thread-safe for concurrent calls on different files; concurrent calls on the same file
///     are not recommended.
/// </remarks>
public sealed class UpdatePackage : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "update-package";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    /// <remarks>
    ///     The singleton is registered with <see cref="CommandsRegistry"/> at startup so that
    ///     workflow YAML dispatch routes to the same instance.
    /// </remarks>
    public static readonly UpdatePackage Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    /// <remarks>
    ///     The entry record associates the command name, usage string, help lines, and singleton
    ///     instance for registration with <see cref="CommandsRegistry"/>.
    /// </remarks>
    public static readonly CommandEntry Entry = new(
        Command,
        "update-package",
        "Update package in SPDX document (workflow only).",
        [
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
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private UpdatePackage()
    {
    }

    /// <summary>
    ///     Rejects CLI invocation of the update-package command.
    /// </summary>
    /// <param name="context">Program context (unused).</param>
    /// <param name="args">Command-line arguments (unused).</param>
    /// <exception cref="CommandUsageException">
    ///     Always thrown, because update-package is only valid within a workflow context.
    /// </exception>
    public override void Run(Context context, string[] args)
    {
        throw new CommandUsageException("'update-package' command is only valid in a workflow");
    }

    /// <summary>
    ///     Runs the update-package command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context (unused).</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">Workflow variable map used to expand input values.</param>
    /// <exception cref="YamlException">
    ///     Thrown when the <c>spdx</c>, <c>package</c>, or <c>package.id</c> input is absent
    ///     from the step.
    /// </exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
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
    ///     Update a package in an SPDX document file
    /// </summary>
    /// <param name="spdxFile">SPDX document filename</param>
    /// <param name="packageId">Package ID to locate within the document.</param>
    /// <param name="updates">
    ///     Map of field names to new values. Supported keys: <c>name</c>, <c>download</c>,
    ///     <c>version</c>, <c>filename</c>, <c>supplier</c>, <c>originator</c>,
    ///     <c>homepage</c>, <c>copyright</c>, <c>summary</c>, <c>description</c>,
    ///     <c>license</c>. Any key not in this set causes a
    ///     <see cref="CommandErrorException"/> to be thrown.
    /// </param>
    /// <exception cref="CommandErrorException">
    ///     Thrown when no package with <paramref name="packageId"/> exists in the document;
    ///     also thrown when <paramref name="updates"/> contains a key that is not one of the
    ///     recognized field names listed above.
    /// </exception>
    /// <remarks>
    ///     When the <c>license</c> field is specified, both
    ///     <see cref="DemaConsulting.SpdxModel.SpdxLicenseElement.ConcludedLicense"/> and
    ///     <see cref="DemaConsulting.SpdxModel.SpdxPackage.DeclaredLicense"/> are set to the
    ///     same value. This dual assignment is intentional: SPDX requires both fields, and a
    ///     single user-supplied license expression is applied to both.
    /// </remarks>
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
    ///     Read the package update fields from the YAML inputs.
    /// </summary>
    /// <param name="map">
    ///     Package sub-map containing the update field entries. May be <see langword="null"/>; when
    ///     null, all field reads return null and no entries are added. The null check before the
    ///     unrecognized-keys loop prevents the error injection path from executing.
    /// </param>
    /// <param name="variables">Currently defined variables</param>
    /// <param name="updates">Updates dictionary to populate</param>
    /// <remarks>
    ///     Only fields present in <paramref name="map"/> are added to <paramref name="updates"/>.
    ///     Fields absent from the map produce a null from <see cref="Command.GetMapString"/> and
    ///     are silently omitted, so unspecified fields are never updated on the target package.
    ///     Unrecognized keys found in <paramref name="map"/> are also added to
    ///     <paramref name="updates"/> with an empty-string value; <see cref="UpdatePackageInSpdxFile"/>
    ///     will throw a <see cref="CommandErrorException"/> for each such key via the
    ///     <c>default</c> branch of its switch statement.
    /// </remarks>
    public static void ParseUpdates(
        YamlMappingNode? map,
        Dictionary<string, string> variables,
        Dictionary<string, string> updates)
    {
        // Read each recognized field and add it to updates when present; fields absent from
        // the map return null from GetMapString and are silently skipped
        foreach (var field in new[]
            { "name", "download", "version", "filename", "supplier",
              "originator", "homepage", "copyright", "summary", "description", "license" })
        {
            var value = GetMapString(map, field, variables);
            if (value != null)
            {
                updates[field] = value;
            }
        }

        // Detect any unrecognized keys — these will be passed to UpdatePackageInSpdxFile,
        // which will throw CommandErrorException for any key not in its switch statement.
        // This ensures users receive a clear diagnostic rather than silent data loss.
        if (map == null)
        {
            return;
        }

        var knownKeys = new HashSet<string>
        {
            "id", "name", "download", "version", "filename",
            "supplier", "originator", "homepage", "copyright",
            "summary", "description", "license"
        };

        foreach (var (keyNode, _) in map.Children)
        {
            var key = keyNode.ToString();
            if (!knownKeys.Contains(key))
            {
                // Add the unknown key so UpdatePackageInSpdxFile's switch default throws
                updates[key] = string.Empty;
            }
        }
    }
}
