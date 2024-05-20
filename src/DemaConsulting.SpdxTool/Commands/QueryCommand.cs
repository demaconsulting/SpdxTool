using System.Diagnostics;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Query a program output for a value
/// </summary>
public class QueryCommand : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly QueryCommand Instance = new();

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
            "      program: <program>",
            "      arguments:",
            "      - <argument>",
            "      - <argument>"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private QueryCommand()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is not 1
        if (args.Length < 2)
            throw new CommandUsageException("'query' command missing arguments");

        // Generate the markdown
        var found = Query(args[0], args[1], args.Skip(2).ToArray());

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

        // Get the 'program' input
        var program = GetMapString(inputs, "program", variables) ??
                      throw new YamlException(step.Start, step.End, "'query' command missing 'program' input");

        // Get the arguments
        var argumentsSequence = GetMapSequence(inputs, "arguments");
        var arguments = argumentsSequence?.Children.Select(c => Expand(c.ToString(), variables)).ToArray() ??
                        Array.Empty<string>();

        // Generate the markdown
        var found = Query(pattern, program, arguments);

        // Save the output to the variables
        variables[output] = found;
    }

    /// <summary>
    /// Run a program and query the output for a value
    /// </summary>
    /// <param name="pattern">Regular expression pattern to capture 'value'</param>
    /// <param name="program">Program to execute</param>
    /// <param name="arguments">Program arguments</param>
    /// <returns>Captured value</returns>
    /// <exception cref="CommandUsageException">On bad usage</exception>
    /// <exception cref="CommandErrorException">On error</exception>
    public static string Query(string pattern, string program, string[] arguments)
    {
        // Construct the regular expression
        var regex = new Regex(pattern);
        if (!regex.GetGroupNames().Contains("value"))
            throw new CommandUsageException("Pattern must contain a 'value' capture group");

        // Construct the start information
        var startInfo = new ProcessStartInfo(program)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Add the arguments
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        // Start the process
        Process process;
        try
        {
            process = Process.Start(startInfo) ??
                      throw new CommandErrorException($"Unable to start program '{program}'");
        }
        catch
        {
            throw new CommandErrorException($"Unable to start program '{program}'");
        }

        // Wait for the process to exit
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new CommandErrorException($"Program '{program}' exited with code {process.ExitCode}");

        // Save the output and return the exit code
        var output = process.StandardOutput.ReadToEnd().Trim();

        // Find the match
        var match = regex.Match(output);
        if (match == null)
            throw new CommandErrorException($"Pattern '{pattern}' not found in program output");

        // Return the captured value
        return match.Groups["value"].Value;
    }
}