using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command to copy package from one SPDX document to another
/// </summary>
public class CopyPackageCommand : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly CopyPackageCommand Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "copy-package",
        "copy-package <arguments>",
        "Copy package information from one SPDX document to another.",
        new[]
        {
            "This command copies a package from one SPDX document to another.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool copy-package <from.spdx.json> <to.spdx.json> <package> <relationship> <element>",
            "",
            "From a YAML file this can be used as:",
            "  - command: copy-package",
            "    inputs:",
            "      from: <from.spdx.json>",
            "      to: <to.spdx.json>",
            "      package: <package>",
            "      relationship: <relationship>",
            "      element: <element>",
            "",
            "The <package> argument is the name of a package in <from.spdx.json> to copy.",
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
    private CopyPackageCommand()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is not 5
        if (args.Length != 5)
            throw new CommandUsageException("'copy-package' command missing arguments");

        // Copy the package
        CopyPackage(args[0], args[1], args[2], args[3], args[4]);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'from' input
        var fromFile = GetMapString(inputs, "from", variables) ??
                       throw new YamlException(step.Start, step.End, "'copy-package' missing 'from' input");

        // Get the 'to' input
        var toFile = GetMapString(inputs, "to", variables) ??
                     throw new YamlException(step.Start, step.End, "'copy-package' missing 'to' input");

        // Get the 'package' input
        var package = GetMapString(inputs, "package", variables) ??
                      throw new YamlException(step.Start, step.End, "'copy-package' missing 'package' input");

        // Get the 'relationship' input
        var relationship = GetMapString(inputs, "relationship", variables) ??
                           throw new YamlException(step.Start, step.End, "'copy-package' missing 'relationship' input");

        // Get the 'element' input
        var element = GetMapString(inputs, "element", variables) ??
                      throw new YamlException(step.Start, step.End, "'copy-package' missing 'element' input");

        // Copy the package
        CopyPackage(fromFile, toFile, package, relationship, element);
    }

    /// <summary>
    /// Copy a package from one SPDX document to another
    /// </summary>
    /// <param name="fromFile">Source SPDX document filename</param>
    /// <param name="toFile">Destination SPDX document filename</param>
    /// <param name="packageId">Package to copy</param>
    /// <param name="relationshipName">Relationship of package to element in destination</param>
    /// <param name="elementId">Destination element</param>
    public static void CopyPackage(string fromFile, string toFile, string packageId, string relationshipName,
        string elementId)
    {
        // Verify from file exists
        if (!File.Exists(fromFile))
            throw new CommandUsageException($"File not found: {fromFile}");

        // Verify to file exists
        if (!File.Exists(toFile))
            throw new CommandUsageException($"File not found: {toFile}");

        // Verify package name
        if (packageId.Length == 0 || packageId == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid package name");

        // Parse the relationship
        var relationship = SpdxRelationshipTypeExtensions.FromText(relationshipName);
        if (relationship == SpdxRelationshipType.Missing)
            throw new CommandUsageException("Invalid relationship");

        // Verify element name
        if (elementId.Length == 0)
            throw new CommandUsageException("Invalid element name");

        // Read the SPDX documents
        var fromDoc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText(fromFile));
        var toDoc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText(toFile));

        // Verify the package exists in the source
        var package = Array.Find(fromDoc.Packages, p => p.Id == packageId) ??
                      throw new CommandErrorException($"Package {packageId} not found in {fromFile}");

        // Verify the package does not exist in the destination
        if (Array.Exists(toDoc.Packages, p => p.Id == package.Id))
            throw new CommandErrorException($"Package {package} already exists in {toFile}");

        // Append the package to the destination document
        toDoc.Packages = toDoc.Packages.Append(package).ToArray();

        // Append any files from the package to the destination document
        foreach (var fileId in package.HasFiles)
        {
            // Find the file
            var file = Array.Find(fromDoc.Files, f => f.Id == fileId) ??
                       throw new CommandErrorException($"File {fileId} not found in {fromFile}");

            // Skip if the file already exists in the destination
            if (Array.Exists(toDoc.Files, f => f.Id == file.Id))
                throw new CommandErrorException($"File {fileId} already exists in {toFile}");

            // Append the file to the destination document
            toDoc.Files = toDoc.Files.Append(file).ToArray();
        }

        // Append the relationship to the destination document
        var newRelationship = new SpdxRelationship
        {
            Id = package.Id,
            RelationshipType = relationship,
            RelatedSpdxElement = elementId
        };
        toDoc.Relationships = toDoc.Relationships.Append(newRelationship).ToArray();

        // Write the destination document
        File.WriteAllText(toFile, Spdx2JsonSerializer.Serialize(toDoc));
    }
}