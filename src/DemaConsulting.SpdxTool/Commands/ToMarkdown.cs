using System.Text;
using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxTool.Spdx;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command to generate a Markdown summary of an SPDX document
/// </summary>
public class ToMarkdown : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly ToMarkdown Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "to-markdown",
        "to-markdown <spdx.json> <out.md> [args]",
        "Create Markdown summary for SPDX document",
        new[]
        {
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
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private ToMarkdown()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is less than 2
        if (args.Length < 2)
            throw new CommandUsageException("'to-markdown' command missing arguments");

        // Get the file names
        var spdxFile = args[0];
        var markdownFile = args[1];

        // Get the title
        var title = args.Length > 2 ? args[2] : "SPDX Document";
        if (string.IsNullOrWhiteSpace(title))
            throw new CommandUsageException("'to-markdown' command invalid 'title' argument");

        // Get the depth
        var depthText = args.Length > 3 ? args[3] : "2";
        if (!int.TryParse(depthText, out var depth) || depth < 1)
            throw new CommandUsageException("'to-markdown' command invalid 'depth' argument");

        // Generate the markdown
        GenerateSummaryMarkdown(spdxFile, markdownFile, title, depth);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'to-markdown' command missing 'spdx' input");

        // Get the 'markdown' input
        var markdownFile = GetMapString(inputs, "markdown", variables) ??
                           throw new YamlException(step.Start, step.End, "'to-markdown' command missing 'spdx' input");

        // Get the 'title' input
        var title = GetMapString(inputs, "title", variables) ?? "SPDX Document";
        if (string.IsNullOrWhiteSpace(title))
            throw new YamlException(step.Start, step.End, "'to-markdown' command invalid 'title' input");

        // Get the 'depth' input
        var depthText = GetMapString(inputs, "depth", variables) ?? "2";
        if (!int.TryParse(depthText, out var depth) || depth < 1)
            throw new YamlException(step.Start, step.End, "'to-markdown' command invalid 'depth' input");

        // Generate the markdown
        GenerateSummaryMarkdown(spdxFile, markdownFile, title, depth);
    }

    /// <summary>
    /// Generate the markdown description for an SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX file</param>
    /// <param name="markdownFile">Markdown file</param>
    /// <param name="title">Markdown title</param>
    /// <param name="depth">Depth of the Markdown headers</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    public static void GenerateSummaryMarkdown(string spdxFile, string markdownFile, string title = "SPDX Document", int depth = 2)
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
            markdown.AppendLine($"| Creator | {creator} |");
        markdown.AppendLine();
        markdown.AppendLine();

        // Find tool package IDs
        var toolIds = new HashSet<string>();
        foreach (var relationship in doc.Relationships)
            if (relationship.RelationshipType is SpdxRelationshipType.BuildToolOf or SpdxRelationshipType.DevToolOf or SpdxRelationshipType.TestToolOf)
                toolIds.Add(relationship.Id);

        // Classify the packages
        var rootPackages = doc.GetRootPackages().OrderBy(p => p.Name).ToArray();
        var packages = doc.Packages.Except(rootPackages).OrderBy(p => p.Name).ToArray();
        var tools = packages.Where(p => toolIds.Contains(p.Id)).ToArray();
        packages = packages.Except(tools).ToArray();

        // Print the root packages
        if (rootPackages.Length > 0)
        {
            markdown.AppendLine($"{header}# Root Packages");
            markdown.AppendLine();
            markdown.AppendLine("| Name | Version | License |");
            markdown.AppendLine("| :-------- | :--- | :--- |");
            foreach (var package in rootPackages)
                markdown.AppendLine(
                    $"| {package.Name} | {package.Version ?? string.Empty} | {package.ConcludedLicense} |");
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
                markdown.AppendLine(
                    $"| {package.Name} | {package.Version ?? string.Empty} | {package.ConcludedLicense} |");
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
                markdown.AppendLine(
                    $"| {package.Name} | {package.Version ?? string.Empty} | {package.ConcludedLicense} |");
            markdown.AppendLine();
            markdown.AppendLine();
        }

        // Save the Markdown text to file
        File.WriteAllText(markdownFile, markdown.ToString());
    }
}