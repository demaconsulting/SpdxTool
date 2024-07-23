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
/// Set Variable Command
/// </summary>
public class SetVariable : Command
{
    /// <summary>
    /// Command name
    /// </summary>
    private const string Command = "set-variable";

    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly SetVariable Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "set-variable",
        "Set workflow variable (workflow only).",
        new[]
        {
            "This command sets a workflow variable.",
            "",
            "  - command: set-variable",
            "    inputs:",
            "      value: <value>                # New value",
            "      output: <variable>            # Variable to set"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private SetVariable()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        throw new CommandUsageException("'set-variable' command is only valid in a workflow");
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'value' input
        var value = GetMapString(inputs, "value", variables) ??
                    throw new YamlException(step.Start, step.End, "'set-variable' command missing 'value' input");

        // Get the 'output' input
        var output = GetMapString(inputs, "output", variables) ??
                     throw new YamlException(step.Start, step.End, "'set-variable' command missing 'output' input");

        // Save the value to the variables
        variables[output] = value;
    }
}