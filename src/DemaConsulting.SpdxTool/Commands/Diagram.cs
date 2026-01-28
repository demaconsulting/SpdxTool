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

using System.Text;
using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxTool.Spdx;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Command to generate a diagram of an SPDX document
/// </summary>
public sealed class Diagram : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "diagram";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly Diagram Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "diagram <spdx.json> <mermaid.txt> [tools]",
        "Generate mermaid diagram.",
        [
            "This command generates a mermaid diagram from an SPDX document.",
            "",
            "  - command: diagram",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      mermaid: <mermaid.txt>        # Mermaid file name",
            "      tools: true                   # Optionally include tools"
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private Diagram()
    {
    }

    /// <inheritdoc />
    public override void Run(Context context, string[] args)
    {
        // Report an error if the number of arguments is less than 2
        if (args.Length < 2)
            throw new CommandUsageException("'diagram' command invalid arguments");

        // Check for options
        var tools = false;
        foreach (var option in args.Skip(2))
            tools = option switch
            {
                "tools" => true,
                _ => throw new CommandUsageException($"'diagram' command invalid option {option}")
            };

        // Generate the diagram
        GenerateDiagram(args[0], args[1], tools);
    }

    /// <inheritdoc />
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'diagram' command missing 'spdx' input");

        // Get the 'mermaid' input
        var mermaidFile = GetMapString(inputs, "mermaid", variables) ??
                          throw new YamlException(step.Start, step.End, "'diagram' command missing 'mermaid' input");

        // Get the 'tools' input
        var toolsText = GetMapString(inputs, "tools", variables) ?? "false";
        if (!bool.TryParse(toolsText, out var tools))
            throw new YamlException(step.Start, step.End, "'diagram' invalid 'tools' input");

        // Generate the diagram
        GenerateDiagram(spdxFile, mermaidFile, tools);
    }

    /// <summary>
    ///     Generate mermaid entity-relationship diagram from SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX document file name</param>
    /// <param name="mermaidFile">Mermaid diagram file name</param>
    /// <param name="tools">True to include tools</param>
    public static void GenerateDiagram(string spdxFile, string mermaidFile, bool tools = false)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Generate the mermaid diagram
        var diagram = new StringBuilder();
        diagram.AppendLine("erDiagram");

        // Process all relationships
        var filteredRelationships = doc.Relationships
            .Where(relationship => tools || relationship.RelationshipType is not
                (SpdxRelationshipType.BuildToolOf or
                 SpdxRelationshipType.DevToolOf or
                 SpdxRelationshipType.TestToolOf))
            .Where(relationship =>
                doc.GetElement<SpdxPackage>(relationship.Id) != null &&
                doc.GetElement<SpdxPackage>(relationship.RelatedSpdxElement) != null);

        foreach (var relationship in filteredRelationships)
        {
            // Get the packages
            var a = doc.GetElement<SpdxPackage>(relationship.Id)!;
            var b = doc.GetElement<SpdxPackage>(relationship.RelatedSpdxElement)!;

            // Get the relationship direction
            var direction = relationship.RelationshipType.GetDirection();
            var from = direction switch
            {
                RelationshipDirection.Parent => a,
                RelationshipDirection.Child => b,
                RelationshipDirection.Sibling => a,
                _ => throw new InvalidDataException()
            };
            var to = direction switch
            {
                RelationshipDirection.Parent => b,
                RelationshipDirection.Child => a,
                RelationshipDirection.Sibling => b,
                _ => throw new InvalidDataException()
            };

            // Write the relationship to the diagram
            var type = relationship.RelationshipType.ToText();
            diagram.AppendLine($"  \"{from.Name} / {from.Version}\" ||--|| \"{to.Name} / {to.Version}\" : \"{type}\"");
        }

        // Write the diagram to the file
        File.WriteAllText(mermaidFile, diagram.ToString());
    }
}