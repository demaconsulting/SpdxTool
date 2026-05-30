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

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Retrieves the version string of a named package from an SPDX document; available from both the CLI and workflow
///     YAML steps.
/// </summary>
/// <remarks>
///     CLI mode matches a package by caller-supplied criteria and writes the version string to standard output.
///     Workflow mode stores the version in a named variable for use in downstream steps. The command delegates
///     package lookup to <see cref="FindPackage"/> so all supported criteria are handled uniformly.
///     Thread-safe: all public methods are stateless and operate only on method-local state.
/// </remarks>
public sealed class GetVersion : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "get-version";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly GetVersion Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "get-version <spdx.json> <criteria>",
        "Get the version of an SPDX package.",
        [
            "This command gets the version of an SPDX package.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool get-version <spdx.json> <criteria>",
            "",
            "  The supported criteria are:",
            "    id=<id>                         # Optional package ID header",
            "    name=<name>                     # Optional package name header",
            "    version=<version>               # Optional package version header",
            "    filename=<filename>             # Optional package filename header",
            "    download=<url>                  # Optional package download URL header",
            "",
            "From a YAML file this can be used as:",
            "  - command: get-version",
            "    inputs:",
            "      output: <variable>            # Output variable",
            "      spdx: <spdx.json>             # SPDX file name",
            "      id: <id>                      # Optional package ID header",
            "      name: <name>                  # Optional package name header",
            "      version: <version>            # Optional package version header",
            "      filename: <filename>          # Optional package filename header",
            "      download: <url>               # Optional package download URL header"
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private GetVersion()
    {
    }

    /// <summary>
    ///     Runs the get-version command from the CLI.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="args">
    ///     Command-line arguments. Must contain at least two elements: the SPDX file path followed by one or
    ///     more package criteria in <c>key=value</c> form.
    /// </param>
    /// <exception cref="CommandUsageException">Thrown when fewer than two arguments are supplied.</exception>
    /// <exception cref="CommandErrorException">
    ///     Thrown when no package matches the supplied criteria, or when multiple packages match.
    /// </exception>
    public override void Run(Context context, string[] args)
    {
        // Report an error if insufficient arguments
        if (args.Length < 2)
        {
            throw new CommandUsageException("'get-version' command missing arguments");
        }

        // Parse the arguments
        var spdxFile = args[0];
        var criteria = new Dictionary<string, string>();
        FindPackage.ParseCriteria(args.Skip(1), criteria);

        // Find the package version
        var packageVersion = FindPackage.FindPackageByCriteria(spdxFile, criteria).Version;

        // Print the version
        context.WriteLine(packageVersion ?? "");
    }

    /// <summary>
    ///     Runs the get-version command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">Workflow variable map; the retrieved version is stored under the key given by the <c>output</c> input.</param>
    /// <exception cref="YamlException">Thrown when the <c>spdx</c> or <c>output</c> input is absent from the step.</exception>
    /// <exception cref="CommandErrorException">
    ///     Thrown when no package matches the supplied criteria, or when multiple packages match.
    /// </exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'spdx' input
        var spdxFile = GetMapString(inputs, "spdx", variables) ??
                       throw new YamlException(step.Start, step.End, "'get-version' command missing 'spdx' input");

        // Get the 'output' input
        var output = GetMapString(inputs, "output", variables) ??
                     throw new YamlException(step.Start, step.End, "'get-version' command missing 'output' input");

        // Get the criteria
        var criteria = new Dictionary<string, string>();
        FindPackage.ParseCriteria(inputs, variables, criteria);

        // Find the package version
        var packageVersion = FindPackage.FindPackageByCriteria(spdxFile, criteria).Version;

        // Save the version
        variables[output] = packageVersion ?? string.Empty;
    }
}
