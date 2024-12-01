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

using DemaConsulting.SpdxTool.Spdx;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command to validate SPDX documents
/// </summary>
public sealed class Validate : Command
{
    /// <summary>
    /// Command name
    /// </summary>
    private const string Command = "validate";

    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly Validate Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "validate <spdx.json> [ntia]",
        "Validate SPDX document for issues",
        [
            "This command validates an SPDX document for issues.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool validate <spdx.json> [ntia]",
            "",
            "From a YAML file this can be used as:",
            "  - command: validate",
            "    inputs:",
            "      spdx: <spdx.json>             # SPDX file name",
            "      ntia: true                    # Optional NTIA checking"
        ],
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private Validate()
    {
    }

    /// <inheritdoc />
    public override void Run(Context context, string[] args)
    {
        // Report an error if for missing arguments
        if (args.Length == 0)
            throw new CommandUsageException("'validate' command missing arguments");

        // Process the arguments
        var spdxFile = args[0];
        var ntia = args.Skip(1).Any(a => a == "ntia");
        
        // Perform validation
        DoValidate(context, spdxFile, ntia);
    }

    /// <inheritdoc />
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'to-markdown' command missing 'spdx' input");

        // Get the 'ntia' input
        var ntiaValue = GetMapString(inputs, "ntia", variables);
        var ntia = ntiaValue?.ToLowerInvariant() == "true";

        // Perform validation
        DoValidate(context, spdxFile, ntia);
    }

    /// <summary>
    /// Validate SPDX document for issues
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="spdxFile">SPDX document file name</param>
    /// <param name="ntia">NTIA flag</param>
    /// <exception cref="CommandErrorException">on issues</exception>
    public static void DoValidate(Context context, string spdxFile, bool ntia)
    {
        // Load the SPDX document
        var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

        // Get the issues
        var issues = new List<string>();
        doc.Validate(issues, ntia);

        // Skip if no issues detected
        if (issues.Count == 0)
            return;

        // Report issues
        foreach (var issue in issues)
            context.WriteWarning(issue);
        context.WriteLine("");
        
        // Throw error
        throw new CommandErrorException($"Found {issues.Count} Issues in {spdxFile}");
    }
}