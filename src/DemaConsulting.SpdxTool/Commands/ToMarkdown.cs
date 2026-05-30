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
///     Command to generate a Markdown summary of an SPDX document
/// </summary>
/// <remarks>
///     ToMarkdown is a stateless singleton that implements the to-markdown command. It reads an SPDX
///     document and writes a Markdown summary grouping packages into Root Packages, Packages, and
///     Tools sections. Both CLI and workflow YAML invocation paths are supported. This class is
///     thread-safe for concurrent calls on different files; concurrent calls writing to the same
///     output file are not recommended.
/// </remarks>
public sealed class ToMarkdown : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "to-markdown";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    /// <remarks>
    ///     The singleton is registered with <see cref="CommandsRegistry"/> at startup so that both
    ///     CLI dispatch and workflow YAML dispatch route to the same instance.
    /// </remarks>
    public static readonly ToMarkdown Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    /// <remarks>
    ///     The entry record associates the command name, usage string, help lines, and singleton
    ///     instance for registration with <see cref="CommandsRegistry"/>.
    /// </remarks>
    public static readonly CommandEntry Entry = new(
        Command,
        "to-markdown <spdx.json> <out.md> [args]",
        "Create Markdown summary for SPDX document",
        [
            "This command produces a Markdown summary of an SPDX document.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool to-markdown <spdx.json> <out.md> [title] [depth]",
            "",
            "From a YAML file this can be used as:",
            "  - command: to-markdown",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      markdown: <out.md>            # Output markdown file",
            "      title: <title>                # Optional title",
            "      depth: <depth>                # Optional heading depth"
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private ToMarkdown()
    {
    }

    /// <summary>
    ///     Runs the to-markdown command from the CLI.
    /// </summary>
    /// <param name="context">Program context (unused).</param>
    /// <param name="args">
    ///     Command-line arguments. Must contain at least two elements: the SPDX file path and the
    ///     output Markdown file path. An optional third element is the document title and an
    ///     optional fourth element is the heading depth.
    /// </param>
    /// <exception cref="CommandUsageException">
    ///     Thrown when fewer than two arguments are supplied, when the title argument is empty or
    ///     contains only whitespace, or when the depth argument is not a positive integer.
    /// </exception>
    public override void Run(Context context, string[] args)
    {
        // Report an error if the number of arguments is less than 2
        if (args.Length < 2)
        {
            throw new CommandUsageException("'to-markdown' command missing arguments");
        }

        // Get the file names
        var spdxFile = args[0];
        var markdownFile = args[1];

        // Get the title
        var title = args.Length > 2 ? args[2] : "SPDX Document";
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new CommandUsageException("'to-markdown' command invalid 'title' argument");
        }

        // Get the depth
        var depthText = args.Length > 3 ? args[3] : "2";
        if (!int.TryParse(depthText, out var depth) || depth < 1)
        {
            throw new CommandUsageException("'to-markdown' command invalid 'depth' argument");
        }

        // Generate the markdown
        GenerateSummaryMarkdown(spdxFile, markdownFile, title, depth);
    }

    /// <summary>
    ///     Runs the to-markdown command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context (unused).</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">Workflow variable map used to expand input values.</param>
    /// <exception cref="YamlException">
    ///     Thrown when the <c>spdx</c> or <c>markdown</c> input is absent from the step, when
    ///     the <c>title</c> input is empty or contains only whitespace, or when the <c>depth</c>
    ///     input is not a positive integer.
    /// </exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'to-markdown' command missing 'spdx' input");

        // Get the 'markdown' input
        var markdownFile = GetMapString(inputs, "markdown", variables) ??
                           throw new YamlException(step.Start, step.End,
                               "'to-markdown' command missing 'markdown' input");

        // Get the 'title' input
        var title = GetMapString(inputs, "title", variables) ?? "SPDX Document";
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new YamlException(step.Start, step.End, "'to-markdown' command invalid 'title' input");
        }

        // Get the 'depth' input
        var depthText = GetMapString(inputs, "depth", variables) ?? "2";
        if (!int.TryParse(depthText, out var depth) || depth < 1)
        {
            throw new YamlException(step.Start, step.End, "'to-markdown' command invalid 'depth' input");
        }

        // Generate the markdown
        GenerateSummaryMarkdown(spdxFile, markdownFile, title, depth);
    }

    /// <summary>
    ///     Generate the markdown description for an SPDX document
    /// </summary>
    /// <remarks>
    ///     Loads the SPDX document, classifies packages into root packages, dependency packages, and
    ///     tool packages, then writes a Markdown summary to <paramref name="markdownFile"/>. Root
    ///     packages are those directly described by the document. Tool packages are identified by
    ///     BUILD_TOOL_OF, DEV_TOOL_OF, or TEST_TOOL_OF relationships. All remaining packages are
    ///     rendered in the Packages section. Concluded license takes priority over declared license
    ///     in each row; "NOASSERTION" is used when neither is set.
    /// </remarks>
    /// <param name="spdxFile">Path to the SPDX JSON file to load. Must not be null; the file must exist on disk.</param>
    /// <param name="markdownFile">Path to the output Markdown file. Must not be null; any existing file is overwritten.</param>
    /// <param name="title">Title text for the top-level Markdown heading. Must not be null. Defaults to <c>"SPDX Document"</c>.</param>
    /// <param name="depth">Heading depth for the Markdown output (number of <c>#</c> characters). Must be a positive integer. Defaults to <c>2</c>.</param>
    /// <exception cref="CommandUsageException">
    ///     Propagated from <see cref="Spdx.SpdxHelpers.LoadJsonDocument"/> when
    ///     <paramref name="spdxFile"/> does not exist on disk.
    /// </exception>
    /// <exception cref="System.IO.IOException">
    ///     Propagated from <see cref="System.IO.File.WriteAllText(string,string)"/> when
    ///     the output file cannot be written.
    /// </exception>
    public static void GenerateSummaryMarkdown(string spdxFile, string markdownFile, string title = "SPDX Document",
        int depth = 2)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Construct the Markdown text
        var markdown = new StringBuilder();

        // Header indent
        var header = new string('#', depth);

        // Add the document information
        markdown.AppendLine($"{header} {title}");
        markdown.AppendLine();
        markdown.AppendLine("| Item | Details |");
        markdown.AppendLine("| :--- | :-------- |");
        markdown.AppendLine($"| File Name | {Path.GetFileName(spdxFile)} |");
        markdown.AppendLine($"| Name | {doc.Name} |");
        markdown.AppendLine($"| Files | {doc.Files.Length} |");
        markdown.AppendLine($"| Packages | {doc.Packages.Length} |");
        markdown.AppendLine($"| Relationships | {doc.Relationships.Length} |");
        markdown.AppendLine($"| Created | {doc.CreationInformation.Created} |");
        foreach (var creator in doc.CreationInformation.Creators)
        {
            markdown.AppendLine($"| Creator | {creator} |");
        }

        markdown.AppendLine();
        markdown.AppendLine();

        // Find tool package IDs
        var toolIds =
            doc
                .Relationships
                .Where(r => r.RelationshipType is SpdxRelationshipType.BuildToolOf or SpdxRelationshipType.DevToolOf
                    or SpdxRelationshipType.TestToolOf)
                .Select(r => r.Id)
                .ToHashSet();

        // Classify the packages
        var rootPackages = doc.GetRootPackages().OrderBy(p => p.Name).ToArray();
        var packages = doc.Packages.Except(rootPackages).OrderBy(p => p.Name).ToArray();
        var tools = packages.Where(p => toolIds.Contains(p.Id)).ToArray();
        packages = [.. packages.Except(tools)];

        // Print the root packages
        if (rootPackages.Length > 0)
        {
            // Sub-section headings are intentionally one level below the title heading (depth+1 hashes)
            markdown.AppendLine($"{header}# Root Packages");
            markdown.AppendLine();
            markdown.AppendLine("| Name | Version | License |");
            markdown.AppendLine("| :-------- | :--- | :--- |");
            foreach (var package in rootPackages)
            {
                markdown.AppendLine(
                    $"| {package.Name} | {package.Version ?? string.Empty} | {License(package)} |");
            }

            markdown.AppendLine();
            markdown.AppendLine();
        }

        // Print the packages
        if (packages.Length > 0)
        {
            markdown.AppendLine($"{header}# Packages");
            markdown.AppendLine();
            markdown.AppendLine("| Name | Version | License |");
            markdown.AppendLine("| :-------- | :--- | :--- |");
            foreach (var package in packages)
            {
                markdown.AppendLine(
                    $"| {package.Name} | {package.Version ?? string.Empty} | {License(package)} |");
            }

            markdown.AppendLine();
            markdown.AppendLine();
        }

        // Print the tools
        if (tools.Length > 0)
        {
            markdown.AppendLine($"{header}# Tools");
            markdown.AppendLine();
            markdown.AppendLine("| Name | Version | License |");
            markdown.AppendLine("| :-------- | :--- | :--- |");
            foreach (var package in tools)
            {
                markdown.AppendLine(
                    $"| {package.Name} | {package.Version ?? string.Empty} | {License(package)} |");
            }

            markdown.AppendLine();
            markdown.AppendLine();
        }

        // Save the Markdown text to file
        File.WriteAllText(markdownFile, markdown.ToString());
    }

    /// <summary>
    ///     Get a license for a package
    /// </summary>
    /// <remarks>
    ///     Concluded license represents the authoritative determination after analysis; declared
    ///     license is the upstream assertion before review. Concluded license therefore takes
    ///     priority. "NOASSERTION" is treated as absent for both fields so the fallback chain
    ///     always produces a meaningful value where one exists.
    /// </remarks>
    /// <param name="package">SPDX package</param>
    /// <returns>License</returns>
    private static string License(SpdxPackage package)
    {
        // Use the concluded license if available
        if (!string.IsNullOrEmpty(package.ConcludedLicense) && package.ConcludedLicense != "NOASSERTION")
        {
            return package.ConcludedLicense;
        }

        // Use the declared license if available
        if (!string.IsNullOrEmpty(package.DeclaredLicense) && package.DeclaredLicense != "NOASSERTION")
        {
            return package.DeclaredLicense;
        }

        // Could not find license
        return "NOASSERTION";
    }
}
