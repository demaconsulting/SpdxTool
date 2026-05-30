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
///     Prints one or more lines of text to the console.
/// </summary>
/// <remarks>
///     In CLI mode each argument is printed as a separate line. In workflow mode the
///     <c>text</c> YAML sequence provides the lines, with variable expansion applied.
///     This command is available from both the CLI and workflow YAML steps.
///     Thread-safe: all public methods on this singleton operate only on method-local state and the shared <see cref="Context"/>.
/// </remarks>
public sealed class Print : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "print";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly Print Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "print <text>",
        "Print text to the console",
        [
            "This command prints text to the console.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool print <text>",
            "",
            "From a YAML file this can be used as:",
            "  - command: print",
            "    inputs:",
            "      text:",
            "      - Some text to print",
            "      - The value of variable is ${{ variable }}"
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private Print()
    {
    }

    /// <summary>
    ///     Runs the print command from the CLI.
    /// </summary>
    /// <remarks>
    ///     An empty <paramref name="args"/> array is valid and produces no output; this is not
    ///     treated as an error because printing nothing is a well-defined no-op.
    /// </remarks>
    /// <param name="context">Program context used for output.</param>
    /// <param name="args">Command-line arguments; each element is printed as a separate line. May be empty.</param>
    public override void Run(Context context, string[] args)
    {
        foreach (var arg in args)
        {
            context.WriteLine(arg);
        }
    }

    /// <summary>
    ///     Runs the print command from a YAML workflow step.
    /// </summary>
    /// <remarks>
    ///     Variable expansion is applied to each text line via <c>GetSequenceString</c> before the
    ///     line is written to output. This method is safe to call concurrently provided the caller
    ///     does not mutate <paramref name="variables"/> while the method is executing.
    /// </remarks>
    /// <param name="context">Program context used for output.</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">Workflow variable map; variable references in each text line are expanded before printing.</param>
    /// <exception cref="YamlException">Thrown when the <c>text</c> sequence input is absent from the step.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a text line contains an undefined variable reference, an empty variable name, or an unmatched macro delimiter.</exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'text' input
        var text = GetMapSequence(inputs, "text") ??
                   throw new YamlException(step.Start, step.End, "'print' command missing 'text' input");

        // Write all text
        for (var i = 0; i < text.Children.Count; i++)
        {
            var line = GetSequenceString(text, i, variables) ?? string.Empty;
            context.WriteLine(line);
        }
    }
}
