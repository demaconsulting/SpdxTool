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

namespace DemaConsulting.SpdxTool;

/// <summary>
///     Single mutable execution-state holder for one SpdxTool invocation.
/// </summary>
/// <remarks>
///     Created once per invocation by <see cref="Create"/>, passed to every command and
///     subsystem, and disposed by <see cref="Program"/> after the command completes.
///     Encapsulates the parsed global flag values, the remaining command arguments,
///     an optional log-file writer, and an error counter. Implements
///     <see cref="IDisposable"/> to close the log file when the invocation ends.
/// </remarks>
public sealed class Context : IDisposable
{
    /// <summary>
    ///     Output log-file writer (when logging output to file)
    /// </summary>
    private readonly StreamWriter? _log;

    /// <summary>
    ///     Initializes a new instance of the Context class
    /// </summary>
    /// <param name="log">Optional log-file writer</param>
    /// <param name="args">Program arguments</param>
    private Context(StreamWriter? log, IReadOnlyCollection<string> args)
    {
        _log = log;
        Arguments = args;
    }

    /// <summary>
    ///     Gets a value indicating the version has been requested
    /// </summary>
    /// <remarks>Set to <see langword="true"/> when <c>-v</c> or <c>--version</c> appears on the command line.</remarks>
    public bool Version { get; private init; }

    /// <summary>
    ///     Gets a value indicating help has been requested
    /// </summary>
    /// <remarks>Set to <see langword="true"/> when <c>-h</c>, <c>-?</c>, or <c>--help</c> appears on the command line.</remarks>
    public bool Help { get; private init; }

    /// <summary>
    ///     Gets a value indicating silent-output has been requested
    /// </summary>
    /// <remarks>When <see langword="true"/>, <see cref="WriteLine"/>, <see cref="WriteWarning"/>, and <see cref="WriteError"/> suppress console output but still write to the log file.</remarks>
    public bool Silent { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether to perform self-validation
    /// </summary>
    /// <remarks>Set to <see langword="true"/> when <c>--validate</c> appears on the command line. Program routes to the SelfTest subsystem when this is true.</remarks>
    public bool Validate { get; private init; }

    /// <summary>
    ///     Gets the name of the validation results file
    /// </summary>
    /// <remarks>
    ///     Set by the <c>-r</c>/<c>--result</c> command-line flag. Empty string when
    ///     <c>--result</c> is not specified. Used by the SelfTest subsystem when
    ///     <see cref="Validate"/> is <see langword="true"/> to write the TRX-style
    ///     self-test results to a file.
    /// </remarks>
    public string ValidationFile { get; private init; } = "";

    /// <summary>
    ///     Gets the depth of the validation report
    /// </summary>
    /// <remarks>
    ///     Set by the <c>--depth</c> command-line flag. Defaults to 1 when <c>--depth</c> is
    ///     not specified. Must be a non-negative integer; non-integer or negative values cause
    ///     <see cref="Create"/> to throw <see cref="InvalidOperationException"/>.
    /// </remarks>
    public int Depth { get; private init; }

    /// <summary>
    ///     Gets the positional command-line arguments
    /// </summary>
    /// <remarks>
    ///     Contains the arguments remaining after all global flags have been consumed
    ///     by <see cref="Create"/>. The first element is typically the command name
    ///     (e.g., <c>validate</c>, <c>query</c>) followed by command-specific operands.
    ///     Empty when no positional arguments appear after the global flags.
    /// </remarks>
    public IReadOnlyCollection<string> Arguments { get; private init; }

    /// <summary>
    ///     Gets the number of errors reported
    /// </summary>
    /// <remarks>Incremented by each call to <see cref="WriteError"/>. Read by <see cref="ExitCode"/> to determine the process exit code.</remarks>
    public int Errors { get; private set; }

    /// <summary>
    ///     Gets the proposed exit code
    /// </summary>
    /// <value>
    ///     0 when no errors have been recorded (<see cref="Errors"/> is zero);
    ///     1 when one or more errors have been recorded.
    /// </value>
    public int ExitCode => Errors > 0 ? 1 : 0;

    /// <summary>
    ///     Dispose of this context
    /// </summary>
    /// <remarks>
    ///     Closes and disposes the log-file writer if one was opened. After disposal,
    ///     calls to <see cref="WriteLine"/>, <see cref="WriteWarning"/>, and
    ///     <see cref="WriteError"/> continue to write to the console (if
    ///     <see cref="Silent"/> is <see langword="false"/>) but no longer write to
    ///     the log file. <c>Dispose</c> must be the final operation on the
    ///     <c>Context</c> instance; calling it concurrently with active output
    ///     calls from another thread is not supported.
    /// </remarks>
    public void Dispose()
    {
        _log?.Dispose();
    }

    /// <summary>
    ///     Writes a line of text to the console (when not silent) and to the log file (when configured).
    /// </summary>
    /// <param name="text">Text to write</param>
    /// <remarks>
    ///     Not thread-safe; do not call concurrently from multiple threads.
    /// </remarks>
    public void WriteLine(string text)
    {
        // Write to the console unless silent
        if (!Silent)
        {
            Console.WriteLine(text);
        }

        // Write to the log if specified
        _log?.WriteLine(text);
    }

    /// <summary>
    ///     Writes a warning message in dark yellow to the console (when not silent) and to the log file (when configured).
    /// </summary>
    /// <param name="message">Warning message to write</param>
    /// <remarks>
    ///     Not thread-safe; do not call concurrently from multiple threads.
    /// </remarks>
    public void WriteWarning(string message)
    {
        // Write to the console unless silent
        if (!Silent)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        // Write to the log if specified
        _log?.WriteLine(message);
    }

    /// <summary>
    ///     Writes an error message in red to the console (when not silent) and to the log file (when configured), and increments <see cref="Errors"/>, causing <see cref="ExitCode"/> to return 1.
    /// </summary>
    /// <param name="message">Error message to write</param>
    /// <remarks>
    ///     Each call to this method increments the <see cref="Errors"/> counter by one.
    ///     Because <see cref="ExitCode"/> returns 1 whenever <see cref="Errors"/> is greater
    ///     than zero, a single call to <see cref="WriteError"/> is sufficient to cause the
    ///     process to exit with a non-zero code.
    /// </remarks>
    public void WriteError(string message)
    {
        // Write to the console unless silent
        if (!Silent)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        // Write to the log if specified
        _log?.WriteLine(message);

        // Increment the number of errors
        Errors++;
    }

    /// <summary>
    ///     Create a program context
    /// </summary>
    /// <param name="args">Program arguments. Must not be null.</param>
    /// <returns>Program context</returns>
    /// <exception cref="ArgumentNullException">Thrown when args is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when a flag is missing its required value argument; when <c>--depth</c> is
    ///     followed by a non-integer string; when <c>--depth</c> is followed by a negative
    ///     integer; or when the log file path is inaccessible or invalid.
    /// </exception>
    /// <remarks>
    ///     Creates or opens a log file (I/O side effect) when the <c>--log</c> flag is present.
    ///     Not thread-safe; do not call concurrently from multiple threads.
    /// </remarks>
    public static Context Create(string[] args)
    {
        // Validate arguments
        ArgumentNullException.ThrowIfNull(args);

        // Process arguments
        var version = false;
        var help = false;
        var silent = false;
        var validate = false;
        var validationFile = "";
        var depth = 1;
        string? logFile = null;
        var extra = new List<string>();
        using var arg = args.AsEnumerable().GetEnumerator();
        while (arg.MoveNext())
        {
            switch (arg.Current)
            {
                case "-v":
                case "--version":
                    // Handle version query
                    version = true;
                    break;

                case "-h":
                case "-?":
                case "--help":
                    // Handle help query
                    help = true;
                    break;

                case "-s":
                case "--silent":
                    // Handle silent flag
                    silent = true;
                    break;

                case "--validate":
                    // Handle self-validation
                    validate = true;
                    break;

                case "-r":
                case "--result":
                    // Handle validation result
                    validationFile = ParseArgument(arg, "Missing result argument");
                    break;

                case "--depth":
                    // Handle depth argument
                    var depthStr = ParseArgument(arg, "Missing depth argument");
                    if (!int.TryParse(depthStr, out depth))
                    {
                        throw new InvalidOperationException($"Invalid depth value '{depthStr}': must be an integer");
                    }

                    if (depth < 0)
                    {
                        throw new InvalidOperationException($"Invalid depth value '{depth}': must be a non-negative integer");
                    }

                    break;

                case "-l":
                case "--log":
                    // Handle logging output
                    logFile = ParseArgument(arg, "Missing log output filename");
                    break;

                default:
                    // Handle unknown argument as start of extra parameters
                    do
                    {
                        extra.Add(arg.Current);
                    } while (arg.MoveNext());

                    break;
            }
        }

        // Return the new context
        StreamWriter? logWriter = null;
        if (logFile != null)
        {
            try
            {
                logWriter = new StreamWriter(logFile);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new InvalidOperationException($"Access denied creating log file '{logFile}': {e.Message}", e);
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException($"Invalid log file path '{logFile}': {e.Message}", e);
            }
            catch (NotSupportedException e)
            {
                throw new InvalidOperationException($"Unsupported log file path '{logFile}': {e.Message}", e);
            }
            catch (IOException e)
            {
                throw new InvalidOperationException($"Cannot create log file '{logFile}': {e.Message}", e);
            }
        }

        return new Context(logWriter, extra.AsReadOnly())
        {
            Version = version,
            Help = help,
            Silent = silent,
            Validate = validate,
            ValidationFile = validationFile,
            Depth = depth
        };
    }

    /// <summary>
    ///     Parse the command line argument from the enumerator
    /// </summary>
    /// <param name="arg">Argument enumerator</param>
    /// <param name="missingMessage">Error message if missing</param>
    /// <returns>Command line argument</returns>
    /// <exception cref="InvalidOperationException">Thrown if argument missing</exception>
    private static string ParseArgument(IEnumerator<string> arg, string missingMessage)
    {
        // Move to the argument
        if (!arg.MoveNext())
        {
            throw new InvalidOperationException(missingMessage);
        }

        // Return the argument
        return arg.Current;
    }
}
