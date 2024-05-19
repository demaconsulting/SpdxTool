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
            "    from: <from.spdx.json>",
            "    to: <to.spdx.json>",
            "    package: <package>",
            "    relationship: <relationship>",
            "    element: <element>",
            "",
            "The <package> argument is the name of a package in <from.spdx.json> to copy.",
            "The <relationship> argument describes the <element> relationship to <package>.",
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
    public override void Run(YamlMappingNode step)
    {
        // Get the from-filename
        if (!step.Children.TryGetValue("from", out var fromFile))
            throw new YamlException(step.Start, step.End, "'copy-package' command missing 'from' parameter");

        // Get the to-filename
        if (!step.Children.TryGetValue("to", out var toFile))
            throw new YamlException(step.Start, step.End, "'copy-package' command missing 'to' parameter");

        // Get the package name
        if (!step.Children.TryGetValue("package", out var package))
            throw new YamlException(step.Start, step.End, "'copy-package' command missing 'package' parameter");

        // Get the relationship type
        if (!step.Children.TryGetValue("relationship", out var relationship))
            throw new YamlException(step.Start, step.End, "'copy-package' command missing 'relationship' parameter");

        // Get the element name
        if (!step.Children.TryGetValue("element", out var element))
            throw new YamlException(step.Start, step.End, "'copy-package' command missing 'element' parameter");

        // Copy the package
        CopyPackage(fromFile.ToString(), toFile.ToString(), package.ToString(), relationship.ToString(),
            element.ToString());
    }

    /// <summary>
    /// Copy a package from one SPDX document to another
    /// </summary>
    /// <param name="fromFile">Source SPDX document filename</param>
    /// <param name="toFile">Destination SPDX document filename</param>
    /// <param name="packageName">Package to copy</param>
    /// <param name="relationshipName">Relationship of package to element in destination</param>
    /// <param name="elementName">Destination element</param>
    public static void CopyPackage(string fromFile, string toFile, string packageName, string relationshipName,
        string elementName)
    {
        // Verify from file exists
        if (!File.Exists(fromFile))
            throw new CommandUsageException($"File not found: {fromFile}");

        // Verify to file exists
        if (!File.Exists(toFile))
            throw new CommandUsageException($"File not found: {toFile}");

        // Verify package name
        if (packageName.Length == 0 || packageName == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid package name");

        // Parse the relationship
        var relationship = SpdxRelationshipTypeExtensions.FromText(relationshipName);
        if (relationship == SpdxRelationshipType.Missing)
            throw new CommandUsageException("Invalid relationship");

        // Verify element name
        if (elementName.Length == 0)
            throw new CommandUsageException("Invalid element name");

        // Read the SPDX documents
        var fromDoc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText(fromFile));
        var toDoc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText(toFile));

        // Verify the package exists in the source
        var package = Array.Find(fromDoc.Packages, p => p.Id == packageName) ??
                      throw new CommandErrorException($"Package {packageName} not found in {fromFile}");

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
            Id = elementName,
            RelationshipType = relationship,
            RelatedSpdxElement = package.Id
        };
        toDoc.Relationships = toDoc.Relationships.Append(newRelationship).ToArray();

        // Write the destination document
        File.WriteAllText(toFile, Spdx2JsonSerializer.Serialize(toDoc));
    }
}