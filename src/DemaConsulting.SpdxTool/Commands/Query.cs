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

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Query a program output for a value
/// </summary>
/// <remarks>
///     Executes an external program, captures its combined stdout and stderr, and matches each output line
///     against a caller-supplied regular expression containing a named <c>value</c> capture group. In CLI mode
///     the captured value is written to stdout; in workflow mode it is stored in a named variable. The regex
///     is compiled with a 100 ms match timeout to prevent catastrophic backtracking on untrusted patterns.
///     Thread-safe: all public methods on this singleton operate only on method-local state and the shared <see cref="Context"/>.
/// </remarks>
public sealed class Query : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "query";

    /// <summary>
    ///     Timeout in milliseconds for each regular expression match, guarding against catastrophic backtracking
    /// </summary>
    private const int RegexMatchTimeoutMs = 100;

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly Query Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "query <pattern> <program> [args]",
        "Query program output for value",
        [
            "This command executes a program and inspects the output for a value.",
            "When executed in a workflow this can be used to set a variable.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool query <pattern> <program> [args]",
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
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private Query()
    {
    }

    /// <summary>
    ///     Runs the query command from the CLI.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="args">
    ///     Command-line arguments. Must contain at least two elements: the regex pattern (with a <c>value</c>
    ///     capture group) followed by the program name; any additional elements are forwarded as program arguments.
    /// </param>
    /// <exception cref="CommandUsageException">Thrown when fewer than two arguments are supplied, the pattern is syntactically invalid, or it lacks a <c>value</c> capture group.</exception>
    /// <exception cref="CommandErrorException">Thrown when the external program cannot be started or the pattern is not matched in any output line.</exception>
    public override void Run(Context context, string[] args)
    {
        // Report an error if fewer than 2 arguments are provided
        if (args.Length < 2)
        {
            throw new CommandUsageException("'query' command missing arguments");
        }

        // Query the program output
        var found = QueryProgramOutput(args[0], args[1], [.. args.Skip(2)]);

        // Write the found value
        context.WriteLine(found);
    }

    /// <summary>
    ///     Runs the query command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">Workflow variable map; the captured value is stored under the key given by the <c>output</c> input.</param>
    /// <exception cref="YamlException">Thrown when the <c>output</c>, <c>pattern</c>, or <c>program</c> input is absent from the step.</exception>
    /// <exception cref="CommandUsageException">Thrown when the pattern is syntactically invalid or lacks a <c>value</c> capture group.</exception>
    /// <exception cref="CommandErrorException">Thrown when the external program cannot be started or the pattern is not matched in any output line.</exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
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
                        [];

        // Query the program output
        var found = QueryProgramOutput(pattern, program, arguments);

        // Save the output to the variables
        variables[output] = found;
    }

    /// <summary>
    ///     Run a program and query the output for a value
    /// </summary>
    /// <remarks>
    ///     Both stdout and stderr are read concurrently before waiting for process exit to prevent the
    ///     deadlock that occurs when a child process fills its output buffer and blocks waiting for the
    ///     reader, while the caller blocks in WaitForExit waiting for the process to terminate. The
    ///     regular expression is compiled with a 100 ms match timeout to guard against catastrophic
    ///     backtracking on untrusted patterns.
    /// </remarks>
    /// <param name="pattern">
    ///     Regular expression pattern used to capture the output value. Must be non-null and
    ///     syntactically valid; must contain a named <c>value</c> capture group. A syntactically
    ///     invalid pattern or a pattern without the <c>value</c> group throws
    ///     <see cref="CommandUsageException"/>.
    /// </param>
    /// <param name="program">
    ///     Name or full path of the program to execute. Must be non-null and non-empty. If the
    ///     program cannot be found or launched a <see cref="CommandErrorException"/> is thrown.
    /// </param>
    /// <param name="arguments">
    ///     Arguments forwarded to the program. Must be non-null; an empty array is valid and
    ///     results in the program being invoked with no arguments.
    /// </param>
    /// <returns>Captured value</returns>
    /// <exception cref="CommandUsageException">
    ///     Thrown when <paramref name="pattern"/> is syntactically invalid or does not contain a
    ///     named "value" capture group.
    /// </exception>
    /// <exception cref="CommandErrorException">
    ///     Thrown when <paramref name="program"/> cannot be started, or when the pattern does not
    ///     match any line of the combined program output.
    /// </exception>
    public static string QueryProgramOutput(string pattern, string program, string[] arguments)
    {
        // Construct the regular expression
        Regex regex;
        try
        {
            regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(RegexMatchTimeoutMs));
        }
        catch (RegexParseException ex)
        {
            throw new CommandUsageException($"Invalid regular expression pattern: {ex.Message}");
        }

        if (!regex.GetGroupNames().Contains("value"))
        {
            throw new CommandUsageException("Pattern must contain a 'value' capture group");
        }

        // Construct the process start information
        var startInfo = new ProcessStartInfo(program)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Add the arguments
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        // Start the process
        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Exception ex) when (ex is Win32Exception or IOException)
        {
            throw new CommandErrorException($"Unable to start program '{program}'");
        }

        // Save the output (read both streams concurrently to prevent deadlock)
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        Task.WaitAll(stdoutTask, stderrTask);
        var output = (stdoutTask.Result + "\n" + stderrTask.Result).Trim();

        // Wait for the process to exit
        process.WaitForExit();

        // Process the output line-by-line
        var outputLines = output.Split('\n').Select(l => l.Trim()).ToArray();
        var value = outputLines
            .Select(line => regex.Match(line))
            .Where(match => match.Success)
            .Select(match => match.Groups["value"].Value)
            .FirstOrDefault(val => !string.IsNullOrEmpty(val));

        if (value != null)
        {
            return value;
        }

        // Match not found in program output
        throw new CommandErrorException($"Pattern '{pattern}' not found in program output");
    }
}
