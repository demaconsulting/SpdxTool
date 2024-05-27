using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Query a program output for a value
/// </summary>
public class Query : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly Query Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "query",
        "query <pattern> <command> [arguments]",
        "Query program output for value",
        new[]
        {
            "This command executes a program and inspects the output for a value.",
            "When executed in a workflow this can be used to set a variable.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool query <pattern> <command> [arguments]",
            "",
            "From a YAML file this can be used as:",
            "  - command: query",
            "    inputs:",
            "      output: <variable>",
            "      pattern: <regex with 'value' capture>",
            "      command: <command> [arguments]"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private Query()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is not 1
        if (args.Length < 2)
            throw new CommandUsageException("'query' command missing arguments");

        // Assemble the arguments into a command
        var command = string.Join(' ', args.Skip(1));

        // Generate the markdown
        var found = QueryProgramOutput(args[0], command);

        // Write the found value to the console
        Console.WriteLine(found);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'output' input
        var output = GetMapString(inputs, "output", variables) ??
                     throw new YamlException(step.Start, step.End, "'query' command missing 'output' input");

        // Get the 'pattern' input
        var pattern = GetMapString(inputs, "pattern", variables) ??
                      throw new YamlException(step.Start, step.End, "'query' command missing 'pattern' input");

        // Get the 'command' input
        var command = GetMapString(inputs, "command", variables) ??
                      throw new YamlException(step.Start, step.End, "'query' command missing 'command' input");

        // Generate the markdown
        var found = QueryProgramOutput(pattern, command);

        // Save the output to the variables
        variables[output] = found;
    }

    /// <summary>
    /// Run a program and query the output for a value
    /// </summary>
    /// <param name="pattern">Regular expression pattern to capture 'value'</param>
    /// <param name="command">Command line to execute</param>
    /// <returns>Captured value</returns>
    /// <exception cref="CommandUsageException">On bad usage</exception>
    /// <exception cref="CommandErrorException">On error</exception>
    public static string QueryProgramOutput(string pattern, string command)
    {
        // Construct the regular expression
        var regex = new Regex(pattern);
        if (!regex.GetGroupNames().Contains("value"))
            throw new CommandUsageException("Pattern must contain a 'value' capture group");

        // Select the filename and arguments
        string fileName;
        string arguments;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName = Environment.ExpandEnvironmentVariables("%COMSPEC%");
            arguments = "/c " + command;
        }
        else
        {
            fileName = Environment.ExpandEnvironmentVariables("%SHELL%");
            arguments = "-c " + command;
        }

        // Construct the process start information
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        // Start the process
        Console.WriteLine($"Executing '{fileName}' with arguments '{arguments}'");
        var process = new Process { StartInfo = startInfo };
        process.Start();

        // Save the output
        var output = process.StandardOutput.ReadToEnd().Trim();

        // Wait for the process to exit
        process.WaitForExit();

        // Process the output line-by-line
        var outputLines = output.Split('\n').Select(l => l.Trim()).ToArray();
        foreach (var line in outputLines)
        {
            // Test if this line contains a match
            var match = regex.Match(line);
            if (!match.Success)
                continue;
            
            // Test if the match value is valid
            var value = match.Groups["value"].Value;
            if (string.IsNullOrEmpty(value))
                continue;

            // Return the match value
            return value;
        }

        // Match not found in program output
        throw new CommandErrorException($"Pattern '{pattern}' not found in program output");
    }
}