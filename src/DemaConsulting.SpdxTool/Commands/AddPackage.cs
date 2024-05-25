using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Add a package to an SPDX document
/// </summary>
public class AddPackage : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly AddPackage Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "add-package",
        "add-package",
        "Add package to SPDX document (workflow only).",
        new[]
        {
            "This command adds a package to an SPDX document.",
            "",
            "  - command: add-package",
            "    inputs:",
            "      package:",
            "        id: <id>",
            "        name: <name>",
            "        copyright: <copyright>",
            "        version: <version>",
            "        download: <download-url>",
            "        license: <license>       # optional",
            "        purl: <package-url>      # optional",
            "        cpe23: <cpe-identifier>  # optional",
            "      spdx: <spdx.json>",
            "      relationship: <relationship>",
            "      element: <element>",
            "",
            "The <relationship> argument describes the <package> relationship to <element>.",
            "The <element> argument is the name of an element in the <to.spdx.json> file.",
            "",
            "The <relationship> is defined by the SPDX specification, and is usually one of:",
            "  DESCRIBES, DESCRIBED_BY, CONTAINS, BUILD_TOOL_OF, ..."
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private AddPackage()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        throw new CommandUsageException("'add-package' command is only valid in a workflow");
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the package map
        var packageMap = GetMapMap(inputs, "package") ??
                         throw new YamlException(step.Start, step.End, "'add-package' missing 'package' input");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'copy-package' missing 'spdx' input");

        // Get the 'relationship' input
        var relationship = GetMapString(inputs, "relationship", variables) ??
                           throw new YamlException(step.Start, step.End, "'copy-package' missing 'relationship' input");

        // Get the 'element' input
        var element = GetMapString(inputs, "element", variables) ??
                      throw new YamlException(step.Start, step.End, "'copy-package' missing 'element' input");

        // Construct the package
        var package = new SpdxPackage
        {
            // Get the package ID
            Id = GetMapString(packageMap, "id", variables) ??
                 throw new YamlException(step.Start, step.End, "'add-package' missing package 'id' input"),

            // Get the package name
            Name = GetMapString(packageMap, "name", variables) ??
                   throw new YamlException(step.Start, step.End, "'add-package' missing package 'name' input"),

            // Get the package version
            Version = GetMapString(packageMap, "version", variables),

            // Get the download location
            DownloadLocation = GetMapString(packageMap, "download", variables) ??
                               throw new YamlException(step.Start, step.End, "'add-package' missing package 'download' input"),

            // Get the package copyright
            CopyrightText = GetMapString(packageMap, "copyright", variables),

            // Get the package license
            ConcludedLicense = GetMapString(packageMap, "license", variables) ?? "NOASSERTION",
            DeclaredLicense = GetMapString(packageMap, "license", variables) ?? "NOASSERTION"
        };

        // Append the PURL if specified
        var purl = GetMapString(packageMap, "purl", variables);
        if (!string.IsNullOrEmpty(purl))
            package.ExternalReferences = package.ExternalReferences.Append(
                new SpdxExternalReference
                {
                    Category = SpdxReferenceCategory.PackageManager,
                    Type = "purl",
                    Locator = purl
                }).ToArray();

        // Append the CPE23 if specified
        var cpe23 = GetMapString(packageMap, "cpe23", variables);
        if (!string.IsNullOrEmpty(cpe23))
            package.ExternalReferences = package.ExternalReferences.Append(
                new SpdxExternalReference
                {
                    Category = SpdxReferenceCategory.Security,
                    Type = "cpe23Type",
                    Locator = cpe23
                }).ToArray();

        // Add the package
        AddPackageToSpdxFile(package, spdxFile, relationship, element);
    }

    /// <summary>
    /// Add a package to the SPDX document
    /// </summary>
    /// <param name="package">Package to add</param>
    /// <param name="spdxFile">SPDX file</param>
    /// <param name="relationshipName">Relationship type</param>
    /// <param name="elementId">Element to relate package to</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    public static void AddPackageToSpdxFile(SpdxPackage package, string spdxFile, string relationshipName, string elementId)
    {
        // Verify to file exists
        if (!File.Exists(spdxFile))
            throw new CommandUsageException($"File not found: {spdxFile}");

        // Verify package ID
        if (package.Id.Length == 0 || package.Id == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid package ID");

        // Parse the relationship
        var relationship = SpdxRelationshipTypeExtensions.FromText(relationshipName);
        if (relationship == SpdxRelationshipType.Missing)
            throw new CommandUsageException("Invalid relationship");

        // Verify element name
        if (elementId.Length == 0)
            throw new CommandUsageException("Invalid element name");

        // Load the SPDX document
        var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText(spdxFile));

        // Add the package (if not already present)
        if (!Array.Exists(doc.Packages, p => p.Id == package.Id))
            doc.Packages = doc.Packages.Append(package).ToArray();

        // Add the relationship
        doc.Relationships = doc.Relationships.Append(
            new SpdxRelationship
            {
                Id = package.Id,
                RelationshipType = relationship,
                RelatedSpdxElement = elementId
            }).ToArray();

        // Save the SPDX document
        File.WriteAllText(spdxFile, Spdx2JsonSerializer.Serialize(doc));
    }
}