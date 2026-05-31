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

using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Abstract base class for all SpdxTool commands.
/// </summary>
/// <remarks>
///     Each concrete command subclass implements two overloads of <c>Run</c>:
///     one for CLI invocation (receives a <c>string[]</c> arguments array) and one for
///     workflow step invocation (receives a <see cref="YamlDotNet.RepresentationModel.YamlMappingNode"/>
///     and a variables dictionary). Helper methods such as <see cref="Expand"/>,
///     <see cref="GetMapString"/>, <see cref="GetMapMap"/>, <see cref="GetMapSequence"/>,
///     and <see cref="GetSequenceString"/> are provided for consistent YAML node extraction
///     and variable substitution across all commands.
/// </remarks>
public abstract class Command
{
    /// <summary>
    ///     Executes the command from the CLI entry point.
    /// </summary>
    /// <remarks>
    ///     Called by <c>Program</c> after resolving the command name in
    ///     <see cref="CommandsRegistry"/>. Subclasses parse <paramref name="args"/> directly
    ///     and perform the requested SPDX operation. Workflow-only commands throw
    ///     <see cref="CommandUsageException"/> immediately from this overload.
    /// </remarks>
    /// <param name="context">Execution context providing output and logging services. Must not be null.</param>
    /// <param name="args">CLI argument tokens following the command name. May be empty.</param>
    /// <exception cref="CommandUsageException">
    ///     Thrown when <paramref name="args"/> has the wrong count, contains invalid options,
    ///     or the command is workflow-only and cannot be invoked from the CLI.
    /// </exception>
    /// <exception cref="CommandErrorException">
    ///     Thrown when the command encounters a runtime failure (e.g., file not found,
    ///     invalid SPDX content).
    /// </exception>
    public abstract void Run(Context context, string[] args);

    /// <summary>
    ///     Executes the command as a step in a YAML workflow file.
    /// </summary>
    /// <remarks>
    ///     Called by <c>RunWorkflow</c> for each step in the workflow YAML document. Subclasses
    ///     read their parameters from <paramref name="step"/> using the helper methods on this
    ///     base class (<see cref="GetMapString"/>, <see cref="GetMapMap"/>, etc.), apply variable
    ///     expansion via <see cref="Expand"/>, and perform the requested SPDX operation.
    ///     Commands that mutate <paramref name="variables"/> (such as <c>SetVariable</c>) do so
    ///     through this overload.
    /// </remarks>
    /// <param name="context">Execution context providing output and logging services. Must not be null.</param>
    /// <param name="step">YAML mapping node for this workflow step. Must not be null.</param>
    /// <param name="variables">Current workflow variable dictionary; may be mutated by the command.</param>
    /// <exception cref="YamlDotNet.Core.YamlException">
    ///     Thrown when required YAML keys are missing or have the wrong node type.
    /// </exception>
    /// <exception cref="CommandErrorException">
    ///     Thrown when the command encounters a runtime failure.
    /// </exception>
    public abstract void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables);

    /// <summary>
    ///     Expands all <c>${{ variable }}</c> references in a text string.
    /// </summary>
    /// <remarks>
    ///     Uses a stack to support nested expansions — an inner <c>${{ ... }}</c> token can produce
    ///     the key used by the outer token, enabling indirect variable references. Environment variables
    ///     are supported via the <c>environment.</c> prefix: <c>${{ environment.NAME }}</c> resolves
    ///     to <c>Environment.GetEnvironmentVariable("NAME")</c>. Stateless and thread-safe as long as
    ///     the caller does not mutate <paramref name="variables"/> concurrently.
    /// </remarks>
    /// <param name="text">Input text that may contain <c>${{ variable }}</c> tokens. Must not be null.</param>
    /// <param name="variables">
    ///     Variable name-to-value map used for token resolution. Must not be null. Keys are compared
    ///     case-sensitively. The dictionary is read but never modified.
    /// </param>
    /// <returns>The input text with all <c>${{ variable }}</c> references replaced by their values.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when a referenced variable is not present in <paramref name="variables"/>
    ///     and does not resolve as an environment variable, when a variable name is empty,
    ///     when <c>}}</c> appears without a matching <c>${{</c>, or when a <c>${{</c> is
    ///     not closed by <c>}}</c>.
    /// </exception>
    public static string Expand(string text, Dictionary<string, string> variables)
    {
        // Use a StringBuilder to assemble the expanded string
        var builder = new System.Text.StringBuilder(text.Length);

        // Use a Stack to track macro-body-start-index positions
        var macroStack = new Stack<int>();

        // Scan through the input text
        var i = 0;
        while (i < text.Length)
        {
            // Check for macro start "${{" 
            // Note: "${{" is NOT appended to the builder - the macro body 
            // content gets built character-by-character in the else branch below
            if (i + 2 < text.Length && text[i] == '$' && text[i + 1] == '{' && text[i + 2] == '{')
            {
                // Push the macro-body-start-index onto the stack (current builder position)
                macroStack.Push(builder.Length);
                i += 3; // Skip "${{" 
            }
            // Check for macro end "}}"
            else if (i + 1 < text.Length && text[i] == '}' && text[i + 1] == '}')
            {
                // Verify we have a matching macro start
                if (macroStack.Count == 0)
                {
                    throw new InvalidOperationException("Unmatched '}}' in variable expansion");
                }

                // Pop the macro-body-start-index
                var macroBodyStart = macroStack.Pop();

                // Extract the macro body from the StringBuilder
                var macroLength = builder.Length - macroBodyStart;
                var name = builder.ToString(macroBodyStart, macroLength).Trim();

                // Check for empty variable name
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new InvalidOperationException("Empty variable name in macro expansion");
                }

                // Look up the value
                string? value;
                if (name.StartsWith("environment."))
                {
                    value = Environment.GetEnvironmentVariable(name["environment.".Length..]); // Skip "environment." prefix
                }
                else
                {
                    variables.TryGetValue(name, out value);
                }

                // Fail if the lookup failed
                if (value == null)
                {
                    throw new InvalidOperationException($"Undefined variable {name}");
                }

                // Replace the macro body with the value
                builder.Remove(macroBodyStart, macroLength);
                builder.Append(value);

                i += 2; // Skip "}}"
            }
            else
            {
                // Normal text - just append to the StringBuilder
                builder.Append(text[i]);
                i++;
            }
        }

        // Verify all macros were closed
        if (macroStack.Count > 0)
        {
            throw new InvalidOperationException("Unmatched '${{' in variable expansion");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Retrieves a nested mapping node from a parent YAML mapping, returning null when absent.
    /// </summary>
    /// <remarks>
    ///     Used by command subclasses to safely read optional or required sub-maps from a
    ///     workflow step node without throwing on missing keys.
    /// </remarks>
    /// <param name="map">Parent map node. Null is accepted and produces a null return.</param>
    /// <param name="name">Key to look up in <paramref name="map"/>.</param>
    /// <returns>
    ///     The child <see cref="YamlMappingNode"/> if the key exists and its value is a mapping;
    ///     otherwise null.
    /// </returns>
    public static YamlMappingNode? GetMapMap(YamlMappingNode? map, string name)
    {
        // Handle null map
        if (map == null)
        {
            return null;
        }

        // Get the entry
        return map.Children.TryGetValue(name, out var value) ? value as YamlMappingNode : null;
    }

    /// <summary>
    ///     Retrieves a nested sequence node from a parent YAML mapping, returning null when absent.
    /// </summary>
    /// <remarks>
    ///     Used by command subclasses to safely read optional or required list values from a
    ///     workflow step node without throwing on missing keys.
    /// </remarks>
    /// <param name="map">Parent map node. Null is accepted and produces a null return.</param>
    /// <param name="name">Key to look up in <paramref name="map"/>.</param>
    /// <returns>
    ///     The child <see cref="YamlSequenceNode"/> if the key exists and its value is a sequence;
    ///     otherwise null.
    /// </returns>
    public static YamlSequenceNode? GetMapSequence(YamlMappingNode? map, string name)
    {
        // Handle null map
        if (map == null)
        {
            return null;
        }

        // Get the entry
        return map.Children.TryGetValue(name, out var value) ? value as YamlSequenceNode : null;
    }

    /// <summary>
    ///     Retrieves a string value from a YAML mapping node with variable expansion applied.
    /// </summary>
    /// <remarks>
    ///     Used by command subclasses to read named parameters from a workflow step YAML node.
    ///     Returns null for absent keys so callers can distinguish "not provided" from an
    ///     empty string value.
    /// </remarks>
    /// <param name="map">Map node to query. Null is accepted and produces a null return.</param>
    /// <param name="key">Key to look up in <paramref name="map"/>.</param>
    /// <param name="variables">Variable dictionary passed to <see cref="Expand"/>.</param>
    /// <returns>
    ///     The expanded string value when the key exists; null when the key is absent or
    ///     <paramref name="map"/> is null.
    /// </returns>
    public static string? GetMapString(YamlMappingNode? map, string key, Dictionary<string, string> variables)
    {
        // Handle null map
        if (map == null)
        {
            return null;
        }

        // Get the parameter
        return map.Children.TryGetValue(key, out var value) ? Expand(value.ToString(), variables) : null;
    }

    /// <summary>
    ///     Retrieves a string element from a YAML sequence node by index with variable expansion applied.
    /// </summary>
    /// <remarks>
    ///     Used by command subclasses to read positional parameters from a workflow step sequence.
    ///     Returns null when the sequence is shorter than the requested index, when the sequence is
    ///     null, or when the index is negative — so callers can distinguish "not provided" from an
    ///     empty string value. A negative <paramref name="index"/> always returns null rather than
    ///     throwing, because the null-conditional operator alone does not guard against negative indices
    ///     on non-null sequences.
    /// </remarks>
    /// <param name="sequence">Sequence node to query. Null is accepted and produces a null return.</param>
    /// <param name="index">Zero-based index of the element to retrieve. Negative values produce a null return.</param>
    /// <param name="variables">Variable dictionary passed to <see cref="Expand"/>.</param>
    /// <returns>
    ///     The expanded string element when the index is non-negative and in range; null when
    ///     <paramref name="sequence"/> is null, the index is negative, or the index is out of range.
    /// </returns>
    public static string? GetSequenceString(YamlSequenceNode? sequence, int index, Dictionary<string, string> variables)
    {
        // Reject null sequence or negative index before checking bounds
        return sequence != null && index >= 0 && sequence.Children.Count > index
            ? Expand(sequence.Children[index].ToString(), variables)
            : null;
    }
}
