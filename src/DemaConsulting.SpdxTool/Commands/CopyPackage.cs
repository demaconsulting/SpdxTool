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
            "  spdx-tool copy-package <from.spdx.json> <to.spdx.json> <package> [recursive]",
            "",
            "From a YAML file this can be used as:",
            "  - command: copy-package",
            "    inputs:",
            "      from: <from.spdx.json>        # Source SPDX file name",
            "      to: <to.spdx.json>            # Destination SPDX file name",
            "      package: <package>            # Package ID",
            "      recursive: true               # Optional recursive flag",
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
        // Report an error if the number of arguments is less than 3
        if (args.Length < 3)
            throw new CommandUsageException("'copy-package' command missing arguments");

        // Get fixed options
        var fromFile = args[0];
        var toFile = args[1];
        var packageId = args[2];

        // Check for recursive option
        var recursive = false;
        if (args.Length > 3)
        {
            if (args[3] != "recursive")
                throw new CommandUsageException($"'copy-package' command invalid option {args[3]}");
            recursive = true;
        }

        // Copy the package
        CopyPackageBetweenSpdxFiles(fromFile, toFile, packageId, Array.Empty<SpdxRelationship>(), recursive);
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

        // Get the 'recursive' input
        var recursiveText = GetMapString(inputs, "recursive", variables) ?? "false";
        if (!bool.TryParse(recursiveText, out var recursive))
            throw new YamlException(step.Start, step.End, "'copy-package' invalid 'recursive' input");

        // Parse the relationships
        var relationshipsSequence = GetMapSequence(inputs, "relationships") ??
                                    throw new YamlException(step.Start, step.End, "'copy-package' missing 'relationships' input");
        var relationships = AddRelationship.Parse("add-package", packageId, relationshipsSequence, variables);

        // Copy the package
        CopyPackageBetweenSpdxFiles(fromFile, toFile, packageId, relationships, recursive);
    }

    /// <summary>
    /// Copy a package from one SPDX document to another
    /// </summary>
    /// <param name="fromFile">Source SPDX document filename</param>
    /// <param name="toFile">Destination SPDX document filename</param>
    /// <param name="packageId">Package to copy</param>
    /// <param name="relationships">Relationships of package to elements in destination</param>
    /// <param name="recursive">Recursive copy option</param>
    public static void CopyPackageBetweenSpdxFiles(string fromFile, string toFile, string packageId, SpdxRelationship[] relationships, bool recursive)
    {
        // Verify package name
        if (packageId.Length == 0 || packageId == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid package name");

        // Read the SPDX documents
        var fromDoc = SpdxHelpers.LoadJsonDocument(fromFile);
        var toDoc = SpdxHelpers.LoadJsonDocument(toFile);

        // Copy the package
        Copy(fromDoc, toDoc, packageId);

        // Append the root relationships to the destination document
        AddRelationship.Add(toDoc, relationships);

        // Copy child packages if recursive
        if (recursive)
        {
            var copied = new HashSet<string> { packageId };
            CopyChildren(fromDoc, toDoc, packageId, copied);
        }

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

    /// <summary>
    /// Copy child packages from one SPDX document to another
    /// </summary>
    /// <param name="fromDoc">SPDX document to copy from</param>
    /// <param name="toDoc">SPDX document to copy to</param>
    /// <param name="parentId">ID of the parent package</param>
    /// <param name="copied">Packages already copied</param>
    public static void CopyChildren(SpdxDocument fromDoc, SpdxDocument toDoc, string parentId, HashSet<string> copied)
    {
        // Process each relationship dealing with the parent package
        foreach (var relationship in fromDoc.Relationships)
        {
            var childId = GetChild(relationship, parentId);
            if (childId == null)
                continue;

            // Skip if the child is not a package
            if (!Array.Exists(fromDoc.Packages, p => p.Id == childId))
                continue;

            // Copy/enhance the child-package
            Copy(fromDoc, toDoc, childId);

            // Add/enhance the relationship
            AddRelationship.Add(toDoc, relationship);

            // Report copied, and process children if not already processed
            if (copied.Add(childId))
                CopyChildren(fromDoc, toDoc, childId, copied);
        }
    }

    /// <summary>
    /// Test if a relationship indicates a child package
    /// </summary>
    /// <param name="relationship">SPDX relationship</param>
    /// <param name="parentId">Parent package ID</param>
    /// <returns>Child package ID or null</returns>
    public static string? GetChild(SpdxRelationship relationship, string parentId)
    {
        return relationship.RelationshipType switch
        {
            SpdxRelationshipType.Describes => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            SpdxRelationshipType.DescribedBy => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.Contains => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            SpdxRelationshipType.ContainedBy => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.DependsOn => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            SpdxRelationshipType.DependencyOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.DependencyManifestOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.BuildDependencyOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.DevDependencyOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.OptionalDependencyOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.ProvidedDependencyOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.TestDependencyOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.RuntimeDependencyOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.Generates => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.GeneratedFrom => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            SpdxRelationshipType.DistributionArtifact => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            SpdxRelationshipType.PatchFor => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.PatchApplied => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.DynamicLink => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            SpdxRelationshipType.StaticLink => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            SpdxRelationshipType.BuildToolOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.DevToolOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.TestToolOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.DocumentationOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.OptionalComponentOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.PackageOf => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.PrerequisiteFor => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            SpdxRelationshipType.HasPrerequisite => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            _ => null
        };
    }
}