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
using DemaConsulting.SpdxModel.Transform;
using DemaConsulting.SpdxTool.Spdx;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Command to copy package from one SPDX document to another
/// </summary>
public sealed class CopyPackage : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "copy-package";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly CopyPackage Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "copy-package <spdx.json> <args>",
        "Copy package between SPDX documents (workflow only).",
        [
            "This command copies a package from one SPDX document to another.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool copy-package <from.spdx.json> <to.spdx.json> <package> [recursive] [files]",
            "",
            "From a YAML file this can be used as:",
            "  - command: copy-package",
            "    inputs:",
            "      from: <from.spdx.json>        # Source SPDX file name",
            "      to: <to.spdx.json>            # Destination SPDX file name",
            "      package: <package>            # Package ID",
            "      recursive: true               # Optional recursive flag",
            "      files: true                   # Optional copy-files flag",
            "      relationships:                # Optional relationships",
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
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private CopyPackage()
    {
    }

    /// <inheritdoc />
    public override void Run(Context context, string[] args)
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
        var files = false;
        foreach (var option in args.Skip(3))
        {
            switch (option)
            {
                case "recursive":
                    recursive = true;
                    break;

                case "files":
                    files = true;
                    break;

                default:
                    throw new CommandUsageException($"'copy-package' command invalid option {option}");
            }
        }

        // Copy the package
        CopyPackageBetweenSpdxFiles(fromFile, toFile, packageId, [], recursive, files);
    }

    /// <inheritdoc />
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
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

        // Get the 'files' input
        var filesText = GetMapString(inputs, "files", variables) ?? "false";
        if (!bool.TryParse(filesText, out var files))
            throw new YamlException(step.Start, step.End, "'copy-package' invalid 'files' input");

        // Parse the relationships
        var relationshipsSequence = GetMapSequence(inputs, "relationships");
        var relationships = AddRelationship.Parse(Command, packageId, relationshipsSequence, variables);

        // Copy the package
        CopyPackageBetweenSpdxFiles(fromFile, toFile, packageId, relationships, recursive, files);
    }

    /// <summary>
    ///     Copy a package from one SPDX document to another
    /// </summary>
    /// <param name="fromFile">Source SPDX document filename</param>
    /// <param name="toFile">Destination SPDX document filename</param>
    /// <param name="packageId">Package to copy</param>
    /// <param name="relationships">Relationships of package to elements in destination</param>
    /// <param name="recursive">Recursive copy option</param>
    /// <param name="files">Copy files option</param>
    public static void CopyPackageBetweenSpdxFiles(string fromFile, string toFile, string packageId,
        SpdxRelationship[] relationships, bool recursive, bool files)
    {
        // Verify package name
        if (packageId.Length == 0 || packageId == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid package name");

        // Read the SPDX documents
        var fromDoc = SpdxHelpers.LoadJsonDocument(fromFile);
        var toDoc = SpdxHelpers.LoadJsonDocument(toFile);

        // Copy the package
        Copy(fromDoc, toDoc, packageId, files);

        // Append the root relationships to the destination document
        AddRelationship.Add(toDoc, relationships);

        // Copy child packages if recursive
        if (recursive)
        {
            var copied = new HashSet<string> { packageId };
            CopyChildren(fromDoc, toDoc, packageId, copied, files);
        }

        // Write the destination document
        SpdxHelpers.SaveJsonDocument(toDoc, toFile);
    }

    /// <summary>
    ///     Copy the package from one SPDX document to another
    /// </summary>
    /// <param name="fromDoc">SPDX document to copy from</param>
    /// <param name="toDoc">SPDX document to copy to</param>
    /// <param name="packageId">ID of the SPDX package to copy</param>
    /// <param name="files">Copy files option</param>
    /// <exception cref="CommandErrorException">On error</exception>
    public static void Copy(SpdxDocument fromDoc, SpdxDocument toDoc, string packageId, bool files)
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
            // Append copy to the to-document (without files)
            toPackage = fromPackage.DeepCopy();
            toPackage.FilesAnalyzed = false;
            toPackage.HasFiles = [];
            toDoc.Packages = [..toDoc.Packages.Append(toPackage)];
        }

        // Skip if we don't need to copy files
        if (!files || fromPackage.FilesAnalyzed == false || fromPackage.HasFiles.Length == 0)
            return;

        // Indicate the to-package has had analyzed files
        toPackage.FilesAnalyzed = true;

        // Get the new file IDs
        var newFiles = fromPackage.HasFiles.Except(toPackage.HasFiles).ToArray();
        if (newFiles.Length == 0)
            return;

        // Copy the files
        foreach (var file in newFiles)
        {
            // Find the file in the source
            var fromFile = Array.Find(fromDoc.Files, f => f.Id == file) ??
                           throw new CommandErrorException($"Package {packageId} refers to missing file {file}");

            // Test if the to-file exists
            var toFile = Array.Find(toDoc.Files, f => SpdxFile.Same.Equals(f, fromFile));
            if (toFile != null)
            {
                // Enhance the to-file and rename if necessary
                toFile.Enhance(fromFile);
                RenameId.Rename(toDoc, toFile.Id, fromFile.Id);
            }
            else
            {
                // Append copy to the to-document
                toFile = fromFile.DeepCopy();
                toDoc.Files = [..toDoc.Files.Append(toFile)];
            }
        }

        // Add the new files
        toPackage.HasFiles = [..toPackage.HasFiles.Concat(newFiles)];
    }

    /// <summary>
    ///     Copy child packages from one SPDX document to another
    /// </summary>
    /// <param name="fromDoc">SPDX document to copy from</param>
    /// <param name="toDoc">SPDX document to copy to</param>
    /// <param name="parentId">ID of the parent package</param>
    /// <param name="copied">Packages already copied</param>
    /// <param name="files">Copy files option</param>
    public static void CopyChildren(SpdxDocument fromDoc, SpdxDocument toDoc, string parentId, HashSet<string> copied,
        bool files)
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
            Copy(fromDoc, toDoc, childId, files);

            // Add/enhance the relationship
            SpdxRelationships.Add(toDoc, relationship);

            // Report copied, and process children if not already processed
            if (copied.Add(childId))
                CopyChildren(fromDoc, toDoc, childId, copied, files);
        }
    }

    /// <summary>
    ///     Test if a relationship indicates a child package
    /// </summary>
    /// <param name="relationship">SPDX relationship</param>
    /// <param name="parentId">Parent package ID</param>
    /// <returns>Child package ID or null</returns>
    public static string? GetChild(SpdxRelationship relationship, string parentId)
    {
        return relationship.RelationshipType.GetDirection() switch
        {
            RelationshipDirection.Parent => relationship.Id == parentId ? relationship.RelatedSpdxElement : null,
            RelationshipDirection.Child => relationship.RelatedSpdxElement == parentId ? relationship.Id : null,
            _ => null
        };
    }
}