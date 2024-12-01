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
///     Program Context class
/// </summary>
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
    public bool Version { get; private init; }

    /// <summary>
    ///     Gets a value indicating help has been requested
    /// </summary>
    public bool Help { get; private init; }

    /// <summary>
    ///     Gets a value indicating silent-output has been requested
    /// </summary>
    public bool Silent { get; private init; }

    /// <summary>
    ///     Gets the arguments
    /// </summary>
    public IReadOnlyCollection<string> Arguments { get; private init; }

    /// <summary>
    ///     Gets the number of errors reported
    /// </summary>
    public int Errors { get; private set; }

    /// <summary>
    ///     Dispose of this context
    /// </summary>
    public void Dispose()
    {
        _log?.Dispose();
    }

    /// <summary>
    ///     Write text to output
    /// </summary>
    /// <param name="text">Text to write</param>
    public void WriteLine(string text)
    {
        // Write to the console unless silent
        if (!Silent)
            Console.WriteLine(text);

        // Write to the log if specified
        _log?.WriteLine(text);
    }

    /// <summary>
    ///     Write warning message to output
    /// </summary>
    /// <param name="message">Warning message to write</param>
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
    ///     Write an error message to output
    /// </summary>
    /// <param name="message">Error message to write</param>
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
    /// <param name="args">Program arguments</param>
    /// <returns>Program context</returns>
    /// <exception cref="InvalidOperationException">Thrown on invalid arguments</exception>
    public static Context Create(string[] args)
    {
        // Process arguments
        var version = false;
        var help = false;
        var silent = false;
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
                    silent = true;
                    break;

                case "-l":
                case "--log":
                    // Handle logging output
                    if (!arg.MoveNext())
                        throw new InvalidOperationException("Missing log output filename");
                    logFile = arg.Current;
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
        return new Context(logFile != null ? new StreamWriter(logFile) : null, extra.AsReadOnly())
        {
            Version = version,
            Help = help,
            Silent = silent
        };
    }
}