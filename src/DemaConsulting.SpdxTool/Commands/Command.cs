using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command base class
/// </summary>
public abstract class Command
{
    /// <summary>
    /// Run the command
    /// </summary>
    /// <param name="args">Command arguments</param>
    public abstract void Run(string[] args);

    /// <summary>
    /// Run the command
    /// </summary>
    /// <param name="step">Command step</param>
    /// <param name="variables">Workflow variables</param>
    public abstract void Run(YamlMappingNode step, Dictionary<string, string> variables);

    /// <summary>
    /// Expand variables in text
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

            // Replace the variable
            if (!variables.TryGetValue(name, out var value))
                throw new InvalidOperationException($"Undefined variable {name}");

            // Apply the replacement
            text = text[..start] + value + text[(end + 2)..];
        }
    }

    /// <summary>
    /// Get a map from a map
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
    /// Get a sequence from a map
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
    /// Get a map value from a map
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
}
