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
///     Add a relationship between SPDX elements
/// </summary>
public sealed class AddRelationship : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "add-relationship";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly AddRelationship Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "add-relationship <spdx.json> <args>",
        "Add relationship between elements.",
        [
            "This command adds a relationship between SPDX elements.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool add-relationship <spdx.json> <id> <type> <element> [comment]",
            "",
            "From a YAML file this can be used as:",
            "  - command: add-relationship",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      id: <id>                      # Element ID",
            "      replace: false                # Replace existing relationships (default: true)",
            "      relationships:",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
            "        comment: <comment>          # Optional comment",
            "      - type: <relationship>        # Relationship type",
            "        element: <element>          # Related element",
            "        comment: <comment>          # Optional comment",
            "",
            "The <relationship> argument describes the <id> relationship to <element>.",
            "",
            "The <relationship> is defined by the SPDX specification, and is usually one of:",
            "  DESCRIBES, DESCRIBED_BY, CONTAINS, BUILD_TOOL_OF, ..."
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private AddRelationship()
    {
    }

    /// <inheritdoc />
    public override void Run(Context context, string[] args)
    {
        // Report an error if the number of arguments is less than 4
        if (args.Length < 4)
            throw new CommandUsageException("'add-relationship' command missing arguments");

        var spdxFile = args[0];
        var relationship = new SpdxRelationship
        {
            Id = args[1],
            RelationshipType = SpdxRelationshipTypeExtensions.FromText(args[2]),
            RelatedSpdxElement = args[3],
            Comment = args.Length > 4 ? args[4] : null
        };

        // Add the relationship
        Add(spdxFile, [relationship]);
    }

    /// <inheritdoc />
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'add-relationship' command missing 'spdx' input");

        // Get the 'id' input
        var id = GetMapString(inputs, "id", variables) ??
                 throw new YamlException(step.Start, step.End, "'add-relationship' command missing 'id' input");

        // Get the 'replace' input
        var replaceText = GetMapString(inputs, "replace", variables) ?? "true";
        if (!bool.TryParse(replaceText, out var replace))
            throw new YamlException(step.Start, step.End, "'add-relationship' invalid 'replace' input");

        // Parse the relationships
        var relationshipsSequence = GetMapSequence(inputs, "relationships") ??
                                    throw new YamlException(step.Start, step.End,
                                        "'add-relationship' missing 'relationships' input");
        var relationships = Parse(Command, id, relationshipsSequence, variables);

        // Add the relationship
        Add(spdxFile, relationships, replace);
    }

    /// <summary>
    ///     Add the SPDX relationship to the SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX document file name</param>
    /// <param name="relationships">SPDX relationships</param>
    /// <param name="replace">Replace existing relationships</param>
    public static void Add(string spdxFile, SpdxRelationship[] relationships, bool replace = false)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        Add(doc, relationships, replace);

        // Save the SPDX document
        SpdxHelpers.SaveJsonDocument(doc, spdxFile);
    }

    /// <summary>
    ///     Add the SPDX relationships to the SPDX document
    /// </summary>
    /// <param name="doc">SPDX document</param>
    /// <param name="relationships">SPDX relationships</param>
    /// <param name="replace">Replace existing relationships</param>
    public static void Add(SpdxDocument doc, SpdxRelationship[] relationships, bool replace = false)
    {
        try
        {
            SpdxRelationships.Add(doc, relationships, replace);
        }
        catch (Exception ex)
        {
            throw new CommandErrorException(ex.Message, ex);
        }
    }

    /// <summary>
    ///     Parse SPDX relationships from a YAML sequence node
    /// </summary>
    /// <param name="command">Command to blame for errors</param>
    /// <param name="packageId">Package ID</param>
    /// <param name="relationships">Relationships YAML sequence node</param>
    /// <param name="variables">Variables for expansion</param>
    /// <returns>Array of SPDX relationships</returns>
    /// <exception cref="YamlException">On error</exception>
    public static SpdxRelationship[] Parse(
        string command,
        string packageId,
        YamlSequenceNode? relationships,
        Dictionary<string, string> variables)
    {
        // Handle no relationships
        if (relationships == null)
            return [];

        // Parse each relationship
        return
        [
            ..relationships.Children.Select(node =>
            {
                // Get the relationship map
                if (node is not YamlMappingNode relationshipMap)
                    throw new YamlException(node.Start, node.End, $"'{command}' relationship must be a mapping");

                // Parse the relationship
                return Parse(command, packageId, relationshipMap, variables);
            })
        ];
    }

    /// <summary>
    ///     Parse an SPDX relationship from a YAML mapping node
    /// </summary>
    /// <param name="command">Command to blame for errors</param>
    /// <param name="packageId">Package ID</param>
    /// <param name="relationshipMap">Relationship YAML mapping node</param>
    /// <param name="variables">Variables for expansion</param>
    /// <returns>SPDX relationship</returns>
    /// <exception cref="YamlException">On error</exception>
    public static SpdxRelationship Parse(
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
                                     $"'{command}' missing relationship 'element' input"),

            // Get the comment
            Comment = GetMapString(relationshipMap, "comment", variables)
        };

        // Return the relationship
        return relationship;
    }
}