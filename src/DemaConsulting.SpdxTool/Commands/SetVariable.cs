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
///     Set Variable Command
/// </summary>
/// <remarks>
///     <para>
///         This class follows the singleton pattern: a single instance is registered with
///         <see cref="CommandsRegistry"/> and shared across all workflow executions.
///     </para>
///     <para>
///         The command is workflow-only by design: direct CLI invocation is rejected with a
///         <see cref="CommandUsageException"/> so that users receive a clear diagnostic rather
///         than a silent no-op. Variable assignment is only meaningful within a running workflow
///         context where a variable map already exists.
///     </para>
///     <para>
///         The class is stateless and thread-safe: all mutable state is passed through the
///         <c>variables</c> parameter and never stored as instance data.
///     </para>
/// </remarks>
public sealed class SetVariable : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "set-variable";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    /// <remarks>
    ///     The singleton is registered with <see cref="CommandsRegistry"/> at startup so that
    ///     workflow YAML dispatch routes to the same instance.
    /// </remarks>
    public static readonly SetVariable Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    /// <remarks>
    ///     The entry record associates the command name, usage string, help lines, and singleton
    ///     instance for registration with <see cref="CommandsRegistry"/>.
    /// </remarks>
    public static readonly CommandEntry Entry = new(
        Command,
        "set-variable",
        "Set workflow variable (workflow only).",
        [
            "This command sets a workflow variable.",
            "",
            "  - command: set-variable",
            "    inputs:",
            "      value: <value>                # New value",
            "      output: <variable>            # Variable to set"
        ],
        Instance);

    /// <summary>
    ///     Initializes the singleton instance. Use <see cref="Instance"/> to access the singleton.
    /// </summary>
    private SetVariable()
    {
    }

    /// <summary>
    ///     Rejects CLI invocation of the set-variable command.
    /// </summary>
    /// <param name="context">Program context (unused).</param>
    /// <param name="args">Command-line arguments (unused).</param>
    /// <exception cref="CommandUsageException">
    ///     Always thrown, because set-variable is only valid within a workflow context.
    /// </exception>
    public override void Run(Context context, string[] args)
    {
        throw new CommandUsageException("'set-variable' command is only valid in a workflow");
    }

    /// <summary>
    ///     Runs the set-variable command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context (unused).</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">
    ///     Workflow variable map; the value specified by the <c>value</c> input is stored under
    ///     the key given by the <c>output</c> input.
    /// </param>
    /// <exception cref="YamlException">
    ///     Thrown when the <c>value</c> or <c>output</c> input is absent from the step.
    /// </exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'value' input - GetMapString handles a null inputs map gracefully by returning
        // null, so no explicit null check is needed here
        var value = GetMapString(inputs, "value", variables) ??
                    throw new YamlException(step.Start, step.End, "'set-variable' command missing 'value' input");

        // Get the 'output' input (not expanded - used literally as the variable key).
        // GetMapString cannot be used here because it applies variable expansion; instead we
        // access inputs.Children directly so the key is stored literally. If inputs were null,
        // the GetMapString call above would have returned null and thrown before reaching here.
        string output;
        if (inputs!.Children.TryGetValue("output", out var rawOutput))
        {
            output = rawOutput.ToString();
        }
        else
        {
            throw new YamlException(step.Start, step.End, "'set-variable' command missing 'output' input");
        }

        // Save the value to the variables
        variables[output] = value;
    }
}
