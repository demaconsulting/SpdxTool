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
///     Rename an element ID in an SPDX document
/// </summary>
/// <remarks>
///     RenameId is a stateless singleton that implements the rename-id command. It renames an SPDX
///     element ID throughout an SPDX document, updating all packages, files, snippets, relationships,
///     HasFiles arrays, and the Describes array. The static <see cref="Rename(SpdxDocument, string,
///     string)"/> overload is also called directly by AddPackage and CopyPackage to reconcile IDs
///     during package enhancement. This class is thread-safe for concurrent calls on different files;
///     concurrent calls on the same file are not recommended.
/// </remarks>
public sealed class RenameId : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string CommandName = "rename-id";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    /// <remarks>
    ///     The singleton is registered with <see cref="CommandsRegistry"/> at startup so that both
    ///     CLI dispatch and workflow YAML dispatch route to the same instance.
    /// </remarks>
    public static readonly RenameId Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    /// <remarks>
    ///     The entry record associates the command name, usage string, help lines, and singleton
    ///     instance for registration with <see cref="CommandsRegistry"/>.
    /// </remarks>
    public static readonly CommandEntry Entry = new(
        CommandName,
        "rename-id <arguments>",
        "Rename an element ID in an SPDX document.",
        [
            "This command renames an element ID in an SPDX document.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool rename-id <spdx.json> <old-id> <new-id>",
            "",
            "From a YAML file this can be used as:",
            "  - command: rename-id",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      new: <new-id>                 # New element ID",
            "      old: <old-id>                 # Old element ID"
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private RenameId()
    {
    }

    /// <summary>
    ///     Runs the rename-id command from the CLI.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="args">
    ///     Command-line arguments. Must contain exactly three elements: the SPDX file path,
    ///     the old element ID, and the new element ID.
    /// </param>
    /// <exception cref="CommandUsageException">
    ///     Thrown when the argument count is not exactly 3.
    /// </exception>
    public override void Run(Context context, string[] args)
    {
        // Report an error if the number of arguments is not 3
        if (args.Length != 3)
        {
            throw new CommandUsageException("'rename-id' command missing arguments");
        }

        // Rename the ID
        Rename(args[0], args[1], args[2]);
    }

    /// <summary>
    ///     Runs the rename-id command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">Workflow variable map used to expand input values.</param>
    /// <exception cref="YamlException">
    ///     Thrown when the <c>spdx</c>, <c>new</c>, or <c>old</c> input is absent from the step.
    /// </exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'rename-id' command missing 'spdx' input");

        // Get the 'new' input
        var newId = GetMapString(inputs, "new", variables) ??
                    throw new YamlException(step.Start, step.End, "'rename-id' command missing 'new' input");

        // Get the 'old' input
        var oldId = GetMapString(inputs, "old", variables) ??
                    throw new YamlException(step.Start, step.End, "'rename-id' command missing 'old' input");

        // Rename the ID
        Rename(spdxFile, oldId, newId);
    }

    /// <summary>
    ///     Rename an element ID in an SPDX document
    /// </summary>
    /// <remarks>
    ///     Loads the SPDX document from <paramref name="spdxFile"/>, delegates to
    ///     <see cref="Rename(SpdxDocument, string, string)"/> for the in-memory rename, then saves
    ///     the updated document back to the same path. If <paramref name="oldId"/> matches no element
    ///     in the document the inner call returns silently with no changes applied, and the file is
    ///     still rewritten to disk (no-op round-trip). <see cref="System.IO.FileNotFoundException"/>
    ///     from the load step is propagated directly to the caller.
    /// </remarks>
    /// <param name="spdxFile">SPDX file name</param>
    /// <param name="oldId">Old element ID</param>
    /// <param name="newId">New element ID</param>
    /// <exception cref="CommandUsageException">
    ///     When oldId or newId is empty or equals "SPDXRef-DOCUMENT"
    /// </exception>
    /// <exception cref="CommandErrorException">
    ///     When newId is already in use by an existing package, file, or snippet in the document
    /// </exception>
    /// <exception cref="System.IO.FileNotFoundException">
    ///     Propagated from <see cref="Spdx.SpdxHelpers.LoadJsonDocument"/> when the SPDX file does not exist
    /// </exception>
    public static void Rename(string spdxFile, string oldId, string newId)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Rename the element
        Rename(doc, oldId, newId);

        // Save the SPDX document
        SpdxHelpers.SaveJsonDocument(doc, spdxFile);
    }

    /// <summary>
    ///     Rename an element ID in an SPDX document
    /// </summary>
    /// <remarks>
    ///     SPDXRef-DOCUMENT is the reserved element identifier for the document root and cannot be
    ///     renamed; rejecting it as oldId or newId preserves document-level invariants. The
    ///     distinction between <see cref="CommandUsageException"/> (invalid ID argument) and
    ///     <see cref="CommandErrorException"/> (ID collision within the document) reflects whether
    ///     the error is a caller contract violation or a document-state conflict. When oldId equals
    ///     newId the method returns immediately so callers such as AddPackage and CopyPackage can
    ///     pass the same ID without incurring an unnecessary document scan.
    /// </remarks>
    /// <param name="doc">SPDX document</param>
    /// <param name="oldId">Old element ID</param>
    /// <param name="newId">New element ID</param>
    /// <exception cref="CommandUsageException">
    ///     When oldId or newId is empty or equals "SPDXRef-DOCUMENT"
    /// </exception>
    /// <exception cref="CommandErrorException">
    ///     When newId is already used by an existing package, file, or snippet in doc
    /// </exception>
    public static void Rename(SpdxDocument doc, string oldId, string newId)
    {
        // Skip if no rename
        if (oldId == newId)
        {
            return;
        }

        // Verify the old ID is valid
        if (oldId.Length == 0 || oldId == "SPDXRef-DOCUMENT")
        {
            throw new CommandUsageException("Old ID must not be empty or 'SPDXRef-DOCUMENT'");
        }

        // Verify the new ID is valid
        if (newId.Length == 0 || newId == "SPDXRef-DOCUMENT")
        {
            throw new CommandUsageException("New ID must not be empty or 'SPDXRef-DOCUMENT'");
        }

        // Verify ID is not in use
        if (Array.Exists(doc.Packages, p => p.Id == newId) ||
            Array.Exists(doc.Files, f => f.Id == newId) ||
            Array.Exists(doc.Snippets, s => s.Id == newId))
        {
            throw new CommandErrorException($"Element ID {newId} is already used");
        }

        // Update packages
        foreach (var package in doc.Packages)
        {
            // Rename the package name if necessary
            package.Id = UpdateId(package.Id, oldId, newId);

            // Rename files in package
            for (var i = 0; i < package.HasFiles.Length; ++i)
            {
                package.HasFiles[i] = UpdateId(package.HasFiles[i], oldId, newId);
            }
        }

        // Update files
        foreach (var file in doc.Files)
        {
            file.Id = UpdateId(file.Id, oldId, newId);
        }

        // Update snippets
        foreach (var snippet in doc.Snippets)
        {
            snippet.Id = UpdateId(snippet.Id, oldId, newId);
            snippet.SnippetFromFile = UpdateId(snippet.SnippetFromFile, oldId, newId);
        }

        // Update relationships
        foreach (var relationship in doc.Relationships)
        {
            // Update the from-element id
            relationship.Id = UpdateId(relationship.Id, oldId, newId);

            // Update the to-element id
            relationship.RelatedSpdxElement = UpdateId(relationship.RelatedSpdxElement, oldId, newId);
        }

        // Update describes
        for (var i = 0; i < doc.Describes.Length; ++i)
        {
            doc.Describes[i] = UpdateId(doc.Describes[i], oldId, newId);
        }
    }

    /// <summary>
    ///     Updates an element ID by replacing the old ID with the new ID if they match.
    /// </summary>
    /// <remarks>
    ///     This is the single consistent replacement primitive called by all collection-update loops
    ///     in <see cref="Rename(SpdxDocument, string, string)"/>. Centralising the comparison here
    ///     ensures that every collection (packages, files, snippets, relationships, Describes) applies
    ///     identical comparison semantics and reduces the risk of divergent behaviour across loops.
    ///     Stateless and thread-safe.
    /// </remarks>
    /// <param name="id">The current ID to be checked and potentially updated.</param>
    /// <param name="oldId">The old ID to be replaced.</param>
    /// <param name="newId">The new ID to replace the old ID.</param>
    /// <returns>The updated ID if the current ID matches the old ID; otherwise the original ID.</returns>
    private static string UpdateId(string id, string oldId, string newId)
    {
        return id == oldId ? newId : id;
    }
}
