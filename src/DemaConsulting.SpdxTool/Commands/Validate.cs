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
///     Command to validate SPDX documents
/// </summary>
public sealed class Validate : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "validate";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly Validate Instance = new();

    /// <summary>
    ///     Entry information for this command
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
    ///     Private constructor - this is a singleton
    /// </summary>
    private Validate()
    {
    }

    /// <summary>
    ///     Run the validate command from CLI arguments.
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="args">CLI arguments: args[0] is the SPDX file path; optional subsequent args may include "ntia".</param>
    /// <remarks>
    ///     Parses the SPDX file path from the first argument and detects the case-sensitive literal
    ///     "ntia" in any subsequent argument to enable NTIA minimum-elements checking.
    /// </remarks>
    /// <exception cref="CommandUsageException">Thrown when no arguments are provided.</exception>
    public override void Run(Context context, string[] args)
    {
        // Report an error for missing arguments
        if (args.Length == 0)
        {
            throw new CommandUsageException("'validate' command missing arguments");
        }

        // Process the arguments
        var spdxFile = args[0];
        var ntia = args.Skip(1).Any(a => a == "ntia");

        // Perform validation
        DoValidate(context, spdxFile, ntia);
    }

    /// <summary>
    ///     Run the validate command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="step">The YAML mapping node representing the workflow step.</param>
    /// <param name="variables">Variable map for substitution in input values.</param>
    /// <remarks>
    ///     Reads the required <c>spdx</c> input and the optional <c>ntia</c> input from the YAML step.
    ///     The <c>ntia</c> input is evaluated case-insensitively (via <c>ToLowerInvariant()</c>),
    ///     so "true", "True", and "TRUE" all enable NTIA checking.
    /// </remarks>
    /// <exception cref="YamlException">Thrown when the required <c>spdx</c> input is missing.</exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'validate' command missing 'spdx' input");

        // Get the 'ntia' input
        var ntiaValue = GetMapString(inputs, "ntia", variables);
        var ntia = ntiaValue?.ToLowerInvariant() == "true";

        // Perform validation
        DoValidate(context, spdxFile, ntia);
    }

    /// <summary>
    ///     Validate SPDX document for issues
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="spdxFile">SPDX document file name</param>
    /// <param name="ntia">When <c>true</c>, NTIA minimum-elements checks are applied in addition to SPDX specification validation.</param>
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
        {
            return;
        }

        // Report issues
        foreach (var issue in issues)
        {
            context.WriteWarning(issue);
        }

        // Write a blank line to visually separate the warning list from the error summary in user output
        context.WriteLine("");

        // Throw error
        throw new CommandErrorException($"Found {issues.Count} Issues in {spdxFile}");
    }
}
