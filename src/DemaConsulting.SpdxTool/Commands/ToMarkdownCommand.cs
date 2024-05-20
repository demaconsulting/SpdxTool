using System.Text;
using DemaConsulting.SpdxModel.IO;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command to generate a Markdown summary of an SPDX document
/// </summary>
public class ToMarkdownCommand : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly ToMarkdownCommand Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "to-markdown",
        "to-markdown <spdx.yaml> <out.md>",
        "Create Markdown summary for SPDX document",
        new[]
        {
            "This command produces a Markdown summary of an SPDX document.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool to-markdown <spdx.yaml> <out.md>",
            "",
            "From a YAML file this can be used as:",
            "  - command: to-markdown",
            "    inputs:",
            "      spdx: <spdx.yaml>",
            "      markdown: <out.md>"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private ToMarkdownCommand()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is not 2
        if (args.Length != 2)
            throw new CommandUsageException("'to-markdown' command missing arguments");

        // Generate the markdown
        Generate(args[0], args[1]);
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

        // Generate the markdown
        Generate(spdxFile, markdownFile);
    }

    /// <summary>
    /// Generate the markdown description for an SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX file</param>
    /// <param name="markdownFile">Markdown file</param>
    /// <param name="depth">Depth of the Markdown headers</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    public static void Generate(string spdxFile, string markdownFile, int depth = 2)
    {
        // Verify the SPDX file exists
        if (!File.Exists(spdxFile))
            throw new CommandUsageException($"File not found: {spdxFile}");

        // Construct the Markdown text
        var markdown = new StringBuilder();

        // Load the SPDX document
        var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText(spdxFile));

        // Add the document information
        markdown.AppendLine($"{new string('#', depth)} SPDX Document");
        markdown.AppendLine();
        markdown.AppendLine("| Item | Details |");
        markdown.AppendLine("| :--- | :-------- |");
        markdown.AppendLine($"| File Name | {Path.GetFileName(spdxFile)} |");
        markdown.AppendLine($"| Name | {doc.Name} |");
        markdown.AppendLine($"| Created | {doc.CreationInformation.Created} |");
        markdown.AppendLine($"| Files | {doc.Files.Length} |");
        markdown.AppendLine($"| Packages | {doc.Packages.Length} |");
        markdown.AppendLine($"| Relationships | {doc.Relationships.Length} |");
        markdown.AppendLine();
        markdown.AppendLine();

        // Print the packages
        markdown.AppendLine($"{new string('#', depth + 1)} Package Summary");
        markdown.AppendLine();
        markdown.AppendLine("| Name | Version | | License |");
        markdown.AppendLine("| :-------- | :--- | :--- | ");
        foreach (var package in doc.Packages)
            markdown.AppendLine(
                $"| {package.Name} | {package.Version ?? string.Empty} | {package.ConcludedLicense ?? string.Empty} |");
        markdown.AppendLine();
        markdown.AppendLine();

        // Save the Markdown text to file
        File.WriteAllText(markdownFile, markdown.ToString());
    }
}