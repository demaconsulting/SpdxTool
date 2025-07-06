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
///     Command base class
/// </summary>
public abstract class Command
{
    /// <summary>
    ///     Run the command
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="args">Command arguments</param>
    public abstract void Run(Context context, string[] args);

    /// <summary>
    ///     Run the command
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="step">Command step</param>
    /// <param name="variables">Workflow variables</param>
    public abstract void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables);

    /// <summary>
    ///     Expand variables in text
    /// </summary>
    /// <param name="text">Text to expand</param>
    /// <param name="variables">Variables</param>
    /// <returns>Expanded text</returns>
    /// <exception cref="InvalidOperationException">on error</exception>
    public static string Expand(string text, Dictionary<string, string> variables)
    {
        while (true)
        {
            // Find the last macro to expand
            var start = text.LastIndexOf("${{", StringComparison.Ordinal);
            if (start < 0)
                return text;

            // Find the end of the macro
            var end = text.IndexOf("}}", start, StringComparison.Ordinal);
            if (end < 0)
                throw new InvalidOperationException("Unmatched '${{' in variable expansion");

            // Get the variable name
            var name = text[(start + 3)..end].Trim();

            // Look up the value
            string? value;
            if (name.StartsWith("environment."))
                value = Environment.GetEnvironmentVariable(name[12..]);
            else
                variables.TryGetValue(name, out value);

            // Fail if the lookup failed
            if (value == null)
                throw new InvalidOperationException($"Undefined variable {name}");

            // Apply the replacement
            text = text[..start] + value + text[(end + 2)..];
        }
    }

    /// <summary>
    ///     Get a map from a map
    /// </summary>
    /// <param name="map">Parent map node</param>
    /// <param name="name">Entry name</param>
    /// <returns>Child map node or null</returns>
    public static YamlMappingNode? GetMapMap(YamlMappingNode? map, string name)
    {
        // Handle null map
        if (map == null)
            return null;

        // Get the entry
        return map.Children.TryGetValue(name, out var value) ? value as YamlMappingNode : null;
    }

    /// <summary>
    ///     Get a sequence from a map
    /// </summary>
    /// <param name="map">Parent map node</param>
    /// <param name="name">Entry name</param>
    /// <returns>Child sequence node or null</returns>
    public static YamlSequenceNode? GetMapSequence(YamlMappingNode? map, string name)
    {
        // Handle null map
        if (map == null)
            return null;

        // Get the entry
        return map.Children.TryGetValue(name, out var value) ? value as YamlSequenceNode : null;
    }

    /// <summary>
    ///     Get a map value from a map
    /// </summary>
    /// <param name="map">Map node</param>
    /// <param name="key">Map key</param>
    /// <param name="variables">Variables for expansion</param>
    /// <returns>Map value or null</returns>
    public static string? GetMapString(YamlMappingNode? map, string key, Dictionary<string, string> variables)
    {
        // Handle null map
        if (map == null)
            return null;

        // Get the parameter
        return map.Children.TryGetValue(key, out var value) ? Expand(value.ToString(), variables) : null;
    }

    /// <summary>
    ///     Get a sequence value from a sequence
    /// </summary>
    /// <param name="sequence">Sequence node</param>
    /// <param name="index">Sequence index</param>
    /// <param name="variables">Variables for expansion</param>
    /// <returns>Sequence value or null</returns>
    public static string? GetSequenceString(YamlSequenceNode? sequence, int index, Dictionary<string, string> variables)
    {
        // Get the parameter
        return sequence?.Children.Count > index ? Expand(sequence.Children[index].ToString(), variables) : null;
    }
}