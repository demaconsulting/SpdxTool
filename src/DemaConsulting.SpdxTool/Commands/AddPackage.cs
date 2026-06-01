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
    /// <exception cref="CommandUsageException">Always thrown — the add-package command is only valid in a workflow.</exception>
    public override void Run(Context context, string[] args)
    {
        throw new CommandUsageException("'add-package' command is only valid in a workflow");
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Follows a parse-then-delegate flow: the inputs map is extracted from the step node,
    ///     then the spdx file path, package map, and optional relationships sequence are parsed
    ///     in order before delegating to <see cref="AddPackageToSpdxFile"/>. When the
    ///     <c>inputs:</c> block is entirely absent from the workflow step, <c>GetMapMap</c>
    ///     returns null; the subsequent <c>GetMapString</c> call on that null map also
    ///     returns null, causing the null-coalescing guard to raise a <see cref="YamlException"/>
    ///     with the expected error message.
    /// </remarks>
    /// <exception cref="YamlException">Thrown when the spdx or package inputs are absent from the workflow step.</exception>
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
    /// <remarks>
    ///     Loads the document from disk, applies Add and AddRelationship.Add in sequence, and
    ///     saves the result back to disk. The two mutation calls are not wrapped in a transaction:
    ///     if AddRelationship.Add fails after Add succeeds the file is not saved (the write only
    ///     occurs after both calls succeed), so the on-disk file remains unchanged. However, the
    ///     in-memory document is partially mutated in that case, which is why this method is
    ///     considered non-atomic.
    /// </remarks>
    /// <param name="spdxFile">
    ///     Path to an existing, valid SPDX JSON file. Must not be null or empty.
    /// </param>
    /// <param name="package">
    ///     Package to add or enhance. Must not be null. The package identity (name and version) determines
    ///     whether an existing entry is enhanced in place or a new entry is appended.
    /// </param>
    /// <param name="relationships">
    ///     Relationships to add to the document alongside the package. Must not be null; pass an empty
    ///     array when no relationships are required.
    /// </param>
    /// <exception cref="CommandUsageException">Thrown when <paramref name="spdxFile"/> does not exist on disk (propagated from <see cref="Spdx.SpdxHelpers.LoadJsonDocument"/>).</exception>
    /// <exception cref="CommandErrorException">Thrown when the relationships cannot be applied to the document.</exception>
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
    /// <remarks>
    ///     When an existing package with the same identity (as determined by <see cref="SpdxPackage.Same"/>
    ///     equality) is found, it is enhanced in place and its SPDX element ID is renamed to the supplied
    ///     package ID so that any downstream references remain valid. The existing package ID is captured
    ///     before <c>Enhance</c> is called; <c>RenameId.Rename</c> receives the pre-enhance ID to guarantee
    ///     all document references are correctly updated regardless of whether <c>Enhance</c> modifies the
    ///     <c>Id</c> field. When no matching package exists, a deep copy of the supplied package is appended
    ///     to the document.
    /// </remarks>
    /// <param name="doc">
    ///     The SPDX document to modify. Must not be null.
    /// </param>
    /// <param name="package">
    ///     SPDX package to add or merge. Must not be null. When a same-identity package already exists in
    ///     <paramref name="doc"/>, this package's fields are used to enhance the existing entry rather than
    ///     appending a duplicate.
    /// </param>
    public static void Add(SpdxDocument doc, SpdxPackage package)
    {
        // Look for the same package
        var p = Array.Find(doc.Packages, p => SpdxPackage.Same.Equals(p, package));
        if (p != null)
        {
            // Capture the old ID before Enhance can overwrite it, then rename all
            // document-level references so any relationships pointing to the old ID
            // are updated before the enhance merges in the new field values
            var oldId = p.Id;
            p.Enhance(package);
            RenameId.Rename(doc, oldId, package.Id);
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
    /// <remarks>
    ///     <c>CopyrightText</c> and both license fields default to <c>NOASSERTION</c> when absent
    ///     because SPDX requires these fields to be populated; <c>NOASSERTION</c> is the standard
    ///     sentinel value indicating that the information was not determined. The <c>license</c>
    ///     input is mapped to both <c>ConcludedLicense</c> and <c>DeclaredLicense</c> because
    ///     a workflow author supplying a single <c>license</c> field most commonly intends both
    ///     the concluded and declared license to be identical; providing separate fields for each
    ///     is not currently supported.
    /// </remarks>
    /// <param name="command">
    ///     Command name used to prefix error messages so that callers can identify which command step
    ///     triggered the error. Must not be null or empty.
    /// </param>
    /// <param name="packageMap">
    ///     YAML mapping node containing the package fields. Must not be null and must include the
    ///     required keys <c>id</c>, <c>name</c>, and <c>download</c>. Optional keys (<c>version</c>,
    ///     <c>filename</c>, <c>supplier</c>, <c>originator</c>, <c>homepage</c>, <c>copyright</c>,
    ///     <c>summary</c>, <c>description</c>, <c>license</c>, <c>purl</c>, <c>cpe23</c>) are silently
    ///     omitted when absent.
    /// </param>
    /// <param name="variables">
    ///     Variable map used to expand <c>${{ variable }}</c> tokens in field values. Must not be null;
    ///     pass an empty dictionary when no expansion is required.
    /// </param>
    /// <returns>New SPDX package</returns>
    /// <exception cref="YamlException">
    ///     Thrown when a required field (id, name, or download) is absent from <paramref name="packageMap"/>.
    /// </exception>
    /// <exception cref="CommandUsageException">
    ///     Thrown when the package ID in <paramref name="packageMap"/> is empty or equals the reserved
    ///     value "SPDXRef-DOCUMENT".
    /// </exception>
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

            // Get the package license (read once, assign to both fields)
            ConcludedLicense = GetMapString(packageMap, "license", variables) ?? "NOASSERTION"
        };
        package.DeclaredLicense = package.ConcludedLicense;

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
