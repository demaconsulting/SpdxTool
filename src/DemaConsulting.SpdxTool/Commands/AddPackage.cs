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
            "      relationships:                # Relationships",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
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

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'add-package' missing 'spdx' input");

        // Parse the package
        var packageMap = GetMapMap(inputs, "package") ??
                         throw new YamlException(step.Start, step.End, "'add-package' missing 'package' input");
        var package = ParsePackage("add-package", packageMap, variables);

        // Parse the relationships
        var relationshipsSequence = GetMapSequence(inputs, "relationships") ??
                                    throw new YamlException(step.Start, step.End, "'add-package' missing 'relationships' input");
        var relationships = ParseRelationships("add-package", package.Id, relationshipsSequence, variables);

        // Add the package
        AddPackageToSpdxFile(spdxFile, package, relationships);
    }

    /// <summary>
    /// Add a package to the SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX file</param>
    /// <param name="package">Package to add</param>
    /// <param name="relationships">Relationships to add</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    public static void AddPackageToSpdxFile(string spdxFile, SpdxPackage package, SpdxRelationship[] relationships)
    {
        // Verify to file exists
        if (!File.Exists(spdxFile))
            throw new CommandUsageException($"File not found: {spdxFile}");

        // Load the SPDX document
        var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText(spdxFile));

        // Add the package (if not already present)
        if (!Array.Exists(doc.Packages, p => p.Id == package.Id))
            doc.Packages = doc.Packages.Append(package).ToArray();

        // Add the relationships
        foreach (var relationship in relationships)
        {
            // Verify the relationship is not already present
            if (Array.Exists(
                    doc.Relationships,
                    r =>
                        r.Id == relationship.Id &&
                        r.RelationshipType == relationship.RelationshipType &&
                        r.RelatedSpdxElement == relationship.RelatedSpdxElement))
                continue;

            // Add the relationship
            doc.Relationships = doc.Relationships.Append(relationship).ToArray();
        }

        // Save the SPDX document
        File.WriteAllText(spdxFile, Spdx2JsonSerializer.Serialize(doc));
    }

    /// <summary>
    /// Create an SPDX package from a YAML mapping node
    /// </summary>
    /// <param name="command">Command to blame for errors</param>
    /// <param name="packageMap">Package YAML mapping node</param>
    /// <param name="variables">Variables for expansion</param>
    /// <returns>New SPDX package</returns>
    /// <exception cref="YamlException">On parse error</exception>
    public static SpdxPackage ParsePackage(string command, YamlMappingNode packageMap, Dictionary<string, string> variables)
    {
        // Get the package ID
        var packageId = GetMapString(packageMap, "id", variables) ??
                        throw new YamlException(packageMap.Start, packageMap.End,
                            $"'{command}' missing package 'id' input");

        // Verify package ID
        if (packageId.Length == 0 || packageId == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid package ID");

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
            CopyrightText = GetMapString(packageMap, "copyright", variables),

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

        // Return the package
        return package;
    }

    /// <summary>
    /// Parse SPDX relationships from a YAML sequence node
    /// </summary>
    /// <param name="command">Command to blame for errors</param>
    /// <param name="packageId">Package ID</param>
    /// <param name="relationships">Relationships YAML sequence node</param>
    /// <param name="variables">Variables for expansion</param>
    /// <returns>Array of SPDX relationships</returns>
    /// <exception cref="YamlException">On error</exception>
    public static SpdxRelationship[] ParseRelationships(
        string command,
        string packageId,
        YamlSequenceNode relationships,
        Dictionary<string, string> variables)
    {
        // Parse each relationship
        return relationships.Children.Select(node =>
        {
            // Get the relationship map
            if (node is not YamlMappingNode relationshipMap)
                throw new YamlException(node.Start, node.End, $"'{command}' relationship must be a mapping");

            // Parse the relationship
            return ParseRelationship(command, packageId, relationshipMap, variables);
        }).ToArray();
    }

    /// <summary>
    /// Parse an SPDX relationship from a YAML mapping node
    /// </summary>
    /// <param name="command">Command to blame for errors</param>
    /// <param name="packageId">Package ID</param>
    /// <param name="relationshipMap">Relationship YAML mapping node</param>
    /// <param name="variables">Variables for expansion</param>
    /// <returns>SPDX relationship</returns>
    /// <exception cref="YamlException">On error</exception>
    public static SpdxRelationship ParseRelationship(
        string command, 
        string packageId,
        YamlMappingNode relationshipMap,
        Dictionary<string, string> variables)
    {
        // Construct the relationship
        var relationship = new SpdxRelationship
        {
            // Package ID
            Id = packageId,

            // Get the relationship type
            RelationshipType = SpdxRelationshipTypeExtensions.FromText(
                GetMapString(relationshipMap, "type", variables) ??
                throw new YamlException(relationshipMap.Start, relationshipMap.End,
                    $"'{command}' missing relationship 'type' input")),

            // Get the related element
            RelatedSpdxElement = GetMapString(relationshipMap, "element", variables) ??
                                 throw new YamlException(relationshipMap.Start, relationshipMap.End,
                                     $"'{command}' missing relationship 'element' input")
        };

        // Return the relationship
        return relationship;
    }
}