﻿using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxTool.Spdx;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Rename an element ID in an SPDX document
/// </summary>
public class RenameId : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly RenameId Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "rename-id",
        "rename-id <arguments>",
        "Rename an element ID in an SPDX document.",
        new[]
        {
            "This command renames an element ID in an SPDX document.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool rename-id <spdx.json> <old-id> <new-id>",
            "",
            "From a YAML file this can be used as:",
            "  - command: rename-id",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      old: <old-id>                 # Old element ID",
            "      new: <new-id>                 # New element ID"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private RenameId()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is not 3
        if (args.Length != 3)
            throw new CommandUsageException("'rename-id' command missing arguments");

        // Rename the ID
        Rename(args[0], args[1], args[2]);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
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
                    throw new YamlException(step.Start, step.End, "'rename-id' command missing 'spdx' input");

        // Rename the ID
        Rename(spdxFile, oldId, newId);
    }

    /// <summary>
    /// Rename an element ID in an SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX file name</param>
    /// <param name="oldId">Old element ID</param>
    /// <param name="newId">New element ID</param>
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
    /// Rename an element ID in an SPDX document
    /// </summary>
    /// <param name="doc">SPDX document</param>
    /// <param name="oldId">Old element ID</param>
    /// <param name="newId">New element ID</param>
    /// <exception cref="CommandUsageException">On invalid usage</exception>
    /// <exception cref="CommandErrorException">On error</exception>
    public static void Rename(SpdxDocument doc, string oldId, string newId)
    {
        // Skip if no rename
        if (oldId == newId)
            return;

        // Verify the old ID is valid
        if (oldId.Length == 0 || oldId == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid old ID");

        // Verify the new ID is valid
        if (newId.Length == 0 || newId == "SPDXRef-DOCUMENT")
            throw new CommandUsageException("Invalid new ID");

        // Verify the IDs are different
        if (oldId == newId)
            throw new CommandUsageException("Old and new IDs are the same");

        // Verify ID is not in use
        if (Array.Exists(doc.Packages, p => p.Id == newId) ||
            Array.Exists(doc.Files, f => f.Id == newId) ||
            Array.Exists(doc.Snippets, s => s.Id == newId))
            throw new CommandErrorException($"Element ID {newId} is already used");

        // Update packages
        foreach (var package in doc.Packages)
        {
            // Rename the package name if necessary
            if (package.Id == oldId)
                package.Id = newId;

            // Rename files in package
            for (var i = 0; i < package.HasFiles.Length; ++i)
                if (package.HasFiles[i] == oldId)
                    package.HasFiles[i] = newId;
        }

        // Update files
        foreach (var file in doc.Files)
            if (file.Id == oldId)
                file.Id = newId;

        // Update snippets
        foreach (var snippet in doc.Snippets)
            if (snippet.Id == oldId)
                snippet.Id = newId;

        // Update relationships
        foreach (var relationship in doc.Relationships)
        {
            // Update the from-element id
            if (relationship.Id == oldId)
                relationship.Id = newId;

            // Update the to-element id
            if (relationship.RelatedSpdxElement == oldId)
                relationship.RelatedSpdxElement = newId;
        }

        // Update describes
        for (var i = 0; i < doc.Describes.Length; ++i)
            if (doc.Describes[i] == oldId)
                doc.Describes[i] = newId;
    }
}