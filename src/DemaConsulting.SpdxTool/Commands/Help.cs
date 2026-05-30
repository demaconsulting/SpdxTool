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
///     Command to display extended help about a command
/// </summary>
/// <remarks>
///     Delegates to <see cref="ShowUsage"/> for both CLI and workflow invocations. The CLI overload requires
///     exactly one argument (the target command name); the workflow overload reads the command name from the
///     <c>about</c> input. Both paths throw on an unknown command so callers receive a clear error message.
///     Thread-safe: the instance <c>Run</c> methods delegate exclusively to the static
///     <see cref="ShowUsage"/> helper and carry no mutable instance state.
/// </remarks>
public sealed class Help : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "help";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly Help Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "help <command>",
        "Display extended help about a command",
        [
            "This command displays extended help information about the specified command.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool help <command>",
            "",
            "From a YAML file this can be used as:",
            "  - command: help",
            "    inputs:",
            "      about: <command>"
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private Help()
    {
    }

    /// <summary>
    ///     Runs the help command from the CLI.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="args">Command-line arguments; must contain exactly one element (the command name).</param>
    /// <exception cref="CommandUsageException">Thrown when args does not contain exactly one argument.</exception>
    public override void Run(Context context, string[] args)
    {
        // Report an error if the number of arguments is not exactly 1
        if (args.Length != 1)
        {
            throw new CommandUsageException("'help' command requires exactly one argument");
        }

        // Display the command's extended help text
        ShowUsage(context, args[0]);
    }

    /// <summary>
    ///     Runs the help command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">Workflow variable map for input expansion.</param>
    /// <exception cref="YamlException">Thrown when the about input is absent from the step inputs.</exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'about' input
        var about = GetMapString(inputs, "about", variables) ??
                    throw new YamlException(step.Start, step.End, "'help' command missing 'about' input");

        // Display the command's extended help text
        ShowUsage(context, about);
    }

    /// <summary>
    ///     Shows the usage for the requested command.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="command">Command to get help on.</param>
    /// <exception cref="CommandUsageException">Thrown when the command name is not registered in <c>CommandsRegistry.Commands</c>.</exception>
    public static void ShowUsage(Context context, string command)
    {
        // Get the entry for the command
        if (!CommandsRegistry.Commands.TryGetValue(command, out var entry))
        {
            throw new CommandUsageException($"Unknown command: '{command}'");
        }

        // Display the command entry
        foreach (var line in entry.Details)
        {
            context.WriteLine(line);
        }
    }
}
