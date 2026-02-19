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
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Add a package to an SPDX document
/// </summary>
public sealed class AddPackage : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "add-package";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly AddPackage Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "add-package",
        "Add package to SPDX document (workflow only).",
        [
            "This command adds a package to an SPDX document.",
            "",
            "  - command: add-package",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      package:                      # New package information",
            "        id: <id>                    # New package ID",
            "        name: <name>                # New package name",
            "        download: <download-url>    # New package download URL",
            "        version: <version>          # Optional package version",
            "        filename: <filename>        # Optional package filename",
            "        supplier: <supplier>        # Optional package supplier",
            "        originator: <originator>    # Optional package originator",
            "        homepage: <homepage>        # Optional package homepage",
            "        copyright: <copyright>      # Optional package copyright",
            "        summary: <summary>          # Optional package summary",
            "        description: <description>  # Optional package description",
            "        license: <license>          # Optional package license",
            "        purl: <package-url>         # Optional package purl",
            "        cpe23: <cpe-identifier>     # Optional package cpe23",
            "      relationships:                # Optional relationships",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
            "        comment: <comment>          # Optional comment",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
            "        comment: <comment>          # Optional comment",
            "",
            "The <relationship> argument describes the <package> relationship to <element>.",
            "The <element> argument is the name of an element in the <to.spdx.json> file.",
            "",
            "The <relationship> is defined by the SPDX specification, and is usually one of:",
            "  DESCRIBES, DESCRIBED_BY, CONTAINS, BUILD_TOOL_OF, ..."
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private AddPackage()
    {
    }

    /// <inheritdoc />
    public override void Run(Context context, string[] args)
    {
        throw new CommandUsageException("'add-package' command is only valid in a workflow");
    }

    /// <inheritdoc />
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'add-package' missing 'spdx' input");

        // Parse the package
        var packageMap = GetMapMap(inputs, "package") ??
                         throw new YamlException(step.Start, step.End, "'add-package' missing 'package' input");
        var package = ParsePackage(Command, packageMap, variables);

        // Parse the relationships
        var relationshipsSequence = GetMapSequence(inputs, "relationships");
        var relationships = AddRelationship.Parse(Command, package.Id, relationshipsSequence, variables);

        // Add the package
        AddPackageToSpdxFile(spdxFile, package, relationships);
    }

    /// <summary>
    ///     Add a package to the SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX file</param>
    /// <param name="package">Package to add</param>
    /// <param name="relationships">Relationships to add</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    public static void AddPackageToSpdxFile(string spdxFile, SpdxPackage package, SpdxRelationship[] relationships)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Add the package
        Add(doc, package);

        // Add the relationships
        AddRelationship.Add(doc, relationships);

        // Save the SPDX document
        SpdxHelpers.SaveJsonDocument(doc, spdxFile);
    }

    /// <summary>
    ///     Add SPDX package to document with optional enhance.
    /// </summary>
    /// <param name="doc">SPDX document</param>
    /// <param name="package">SPDX package to add</param>
    public static void Add(SpdxDocument doc, SpdxPackage package)
    {
        // Look for the same package
        var p = Array.Find(doc.Packages, p => SpdxPackage.Same.Equals(p, package));
        if (p != null)
        {
            // Enhance the existing package and rename it
            p.Enhance(package);
            RenameId.Rename(doc, p.Id, package.Id);
        }
        else
        {
            // Copy the new package
            p = package.DeepCopy();
            doc.Packages = [.. doc.Packages.Append(p)];
        }
    }

    /// <summary>
    ///     Create an SPDX package from a YAML mapping node
    /// </summary>
    /// <param name="command">Command to blame for errors</param>
    /// <param name="packageMap">Package YAML mapping node</param>
    /// <param name="variables">Variables for expansion</param>
    /// <returns>New SPDX package</returns>
    /// <exception cref="YamlException">On parse error</exception>
    public static SpdxPackage ParsePackage(string command, YamlMappingNode packageMap,
        Dictionary<string, string> variables)
    {
        // Get the package ID
        var packageId = GetMapString(packageMap, "id", variables) ??
                        throw new YamlException(packageMap.Start, packageMap.End,
                            $"'{command}' missing package 'id' input");

        // Verify package ID
        if (packageId.Length == 0 || packageId == "SPDXRef-DOCUMENT")
        {
            throw new CommandUsageException("Invalid package ID");
        }

        // Construct the package
        var package = new SpdxPackage
        {
            // Package ID
            Id = packageId,

            // Get the package name
            Name = GetMapString(packageMap, "name", variables) ??
                   throw new YamlException(packageMap.Start, packageMap.End,
                       $"'{command}' missing package 'name' input"),

            // Get the download location
            DownloadLocation = GetMapString(packageMap, "download", variables) ??
                               throw new YamlException(packageMap.Start, packageMap.End,
                                   $"'{command}' missing package 'download' input"),

            // Get the package version (optional)
            Version = GetMapString(packageMap, "version", variables),

            // Get the package filename (optional)
            FileName = GetMapString(packageMap, "filename", variables),

            // Get the package supplier (optional)
            Supplier = GetMapString(packageMap, "supplier", variables),

            // Get the package originator (optional)
            Originator = GetMapString(packageMap, "originator", variables),

            // Get the package homepage (optional)
            HomePage = GetMapString(packageMap, "homepage", variables),

            // Get the package copyright (optional)
            CopyrightText = GetMapString(packageMap, "copyright", variables) ?? "NOASSERTION",

            // Get the package summary (optional)
            Summary = GetMapString(packageMap, "summary", variables),

            // Get the package description (optional)
            Description = GetMapString(packageMap, "description", variables),

            // Get the package license
            ConcludedLicense = GetMapString(packageMap, "license", variables) ?? "NOASSERTION",
            DeclaredLicense = GetMapString(packageMap, "license", variables) ?? "NOASSERTION"
        };

        // Append the PURL if specified
        var purl = GetMapString(packageMap, "purl", variables);
        if (!string.IsNullOrEmpty(purl))
        {
            package.ExternalReferences =
            [
                ..package.ExternalReferences.Append(
                    new SpdxExternalReference
                    {
                        Category = SpdxReferenceCategory.PackageManager,
                        Type = "purl",
                        Locator = purl
                    })
            ];
        }

        // Append the CPE23 if specified
        var cpe23 = GetMapString(packageMap, "cpe23", variables);
        if (!string.IsNullOrEmpty(cpe23))
        {
            package.ExternalReferences =
            [
                ..package.ExternalReferences.Append(
                    new SpdxExternalReference
                    {
                        Category = SpdxReferenceCategory.Security,
                        Type = "cpe23Type",
                        Locator = cpe23
                    })
            ];
        }

        // Return the package
        return package;
    }
}
