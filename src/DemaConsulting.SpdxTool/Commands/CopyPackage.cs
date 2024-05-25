using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command to copy package from one SPDX document to another
/// </summary>
public class CopyPackage : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly CopyPackage Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "copy-package",
        "copy-package",
        "Copy package between SPDX documents (workflow only).",
        new[]
        {
            "This command copies a package from one SPDX document to another.",
            "",
            "  - command: copy-package",
            "    inputs:",
            "      from: <from.spdx.json>        # Source SPDX file name",
            "      to: <to.spdx.json>            # Destination SPDX file name",
            "      package: <package>            # Package ID",
            "      relationships:                # Relationships",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
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
    private CopyPackage()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        throw new CommandUsageException("'copy-package' command is only valid in a workflow");
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
        var packageId = GetMapString(inputs, "package", variables) ??
                      throw new YamlException(step.Start, step.End, "'copy-package' missing 'package' input");

        // Parse the relationships
        var relationshipsSequence = GetMapSequence(inputs, "relationships") ??
                                    throw new YamlException(step.Start, step.End, "'copy-package' missing 'relationships' input");
        var relationships = AddPackage.ParseRelationships("add-package", packageId, relationshipsSequence, variables);

        // Copy the package
        CopyPackageBetweenSpdxFiles(fromFile, toFile, packageId, relationships);
    }

    /// <summary>
    /// Copy a package from one SPDX document to another
    /// </summary>
    /// <param name="fromFile">Source SPDX document filename</param>
    /// <param name="toFile">Destination SPDX document filename</param>
    /// <param name="packageId">Package to copy</param>
    /// <param name="relationships">Relationships of package to elements in destination</param>
    public static void CopyPackageBetweenSpdxFiles(string fromFile, string toFile, string packageId, SpdxRelationship[] relationships)
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

        // Append the relationships to the destination document
        toDoc.Relationships = toDoc.Relationships.Concat(relationships).ToArray();

        // Write the destination document
        File.WriteAllText(toFile, Spdx2JsonSerializer.Serialize(toDoc));
    }
}