using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxTool.Spdx;
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
        "copy-package <spdx.json> <args>",
        "Copy package between SPDX documents (workflow only).",
        new[]
        {
            "This command copies a package from one SPDX document to another.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool copy-package <from.spdx.json> <to.spdx.json> <package>",
            "",
            "From a YAML file this can be used as:",
            "  - command: copy-package",
            "    inputs:",
            "      from: <from.spdx.json>        # Source SPDX file name",
            "      to: <to.spdx.json>            # Destination SPDX file name",
            "      package: <package>            # Package ID",
            "      relationships:                # Relationships",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
            "        comment: <comment>          # Optional comment",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
            "        comment: <comment>          # Optional comment",
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
        // Report an error if the number of arguments not 3
        if (args.Length != 3)
            throw new CommandUsageException("'copy-package' command missing arguments");

        var fromFile = args[0];
        var toFile = args[1];
        var packageId = args[2];

        // Copy the package
        CopyPackageBetweenSpdxFiles(fromFile, toFile, packageId, Array.Empty<SpdxRelationship>());
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
        var relationships = AddRelationship.Parse("add-package", packageId, relationshipsSequence, variables);

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
        // Verify package name
        if (packageId.Length == 0 || packageId == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid package name");

        // Read the SPDX documents
        var fromDoc = SpdxHelpers.LoadJsonDocument(fromFile);
        var toDoc = SpdxHelpers.LoadJsonDocument(toFile);

        // Copy the package, and rename if necessary
        Copy(fromDoc, toDoc, packageId);

        // Append the relationships to the destination document
        AddRelationship.Add(toDoc, relationships);

        // Write the destination document
        SpdxHelpers.SaveJsonDocument(toDoc, toFile);
    }

    /// <summary>
    /// Copy the package from one SPDX document to another
    /// </summary>
    /// <param name="fromDoc">SPDX document to copy from</param>
    /// <param name="toDoc">SPDX document to copy to</param>
    /// <param name="packageId">ID of the SPDX package to copy</param>
    /// <exception cref="CommandErrorException">On error</exception>
    public static void Copy(SpdxDocument fromDoc, SpdxDocument toDoc, string packageId)
    {
        // Verify the package exists in the source
        var fromPackage = Array.Find(fromDoc.Packages, p => p.Id == packageId) ??
                      throw new CommandErrorException($"Package {packageId} not found");

        // Test if the to-package exists
        var toPackage = Array.Find(toDoc.Packages, p => SpdxPackage.Same.Equals(p, fromPackage));
        if (toPackage != null)
        {
            // Enhance the to-package and rename if necessary
            toPackage.Enhance(fromPackage);
            RenameId.Rename(toDoc, toPackage.Id, packageId);
        }
        else
        {
            // Append copy to the to-document
            toPackage = fromPackage.DeepCopy();
            toDoc.Packages = toDoc.Packages.Append(toPackage).ToArray();
        }
    }
}