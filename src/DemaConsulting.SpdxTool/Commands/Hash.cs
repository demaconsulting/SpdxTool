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

using System.Security.Cryptography;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Generates or verifies a SHA-256 hash for a file using a sidecar file.
/// </summary>
/// <remarks>
///     In generate mode the digest is computed and persisted to a sidecar file
///     (file + ".sha256") so later runs can verify integrity without re-reading
///     the original source. In verify mode the sidecar is read and the freshly
///     computed digest is compared against the stored value. Using a sidecar file
///     avoids embedding hash data inside SPDX documents and keeps the command
///     independent of any particular SPDX schema version.
///     Thread-safe: all public methods are static and operate only on method-local
///     state; concurrent calls on different files are safe. Concurrent calls on the
///     same file are not recommended because the sidecar write is non-atomic.
/// </remarks>
public sealed class Hash : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "hash";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly Hash Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "hash <operation> <algorithm> <file>",
        "Generate or verify hashes of files",
        [
            "This command generates or verifies hashes.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool hash generate sha256 <file>",
            "  spdx-tool hash verify sha256 <file>",
            "",
            "From a YAML file this can be used as:",
            "  - command: hash",
            "    inputs:",
            "      operation: generate | verify",
            "      algorithm: sha256",
            "      file: <file>"
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private Hash()
    {
    }

    /// <summary>
    ///     Runs the hash command from the CLI.
    /// </summary>
    /// <remarks>
    ///     This overload is the command-line entry point; it validates the argument count before
    ///     delegating to <see cref="DoHashOperation"/>.
    /// </remarks>
    /// <param name="context">Program context used for output.</param>
    /// <param name="args">Command-line arguments; must contain exactly three elements: operation, algorithm, and file path.</param>
    /// <exception cref="CommandUsageException">Thrown when the argument count is not exactly three, the algorithm is unsupported, or the operation is unrecognized.</exception>
    /// <exception cref="CommandErrorException">Thrown when the target file does not exist or an I/O error occurs.</exception>
    public override void Run(Context context, string[] args)
    {
        // Report an error if the number of arguments is not 3
        if (args.Length != 3)
        {
            throw new CommandUsageException("'hash' command requires exactly 3 arguments");
        }

        // Do the hash operation
        var operation = args[0];
        var algorithm = args[1];
        var file = args[2];
        DoHashOperation(context, operation, algorithm, file);
    }

    /// <summary>
    ///     Runs the hash command from a YAML workflow step.
    /// </summary>
    /// <remarks>
    ///     This overload is the workflow entry point; it extracts <c>operation</c>, <c>algorithm</c>,
    ///     and <c>file</c> inputs from the YAML node before delegating to <see cref="DoHashOperation"/>.
    /// </remarks>
    /// <param name="context">Program context used for output.</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">Workflow variable map for input expansion.</param>
    /// <exception cref="YamlException">Thrown when the <c>operation</c>, <c>algorithm</c>, or <c>file</c> input is absent from the step.</exception>
    /// <exception cref="CommandUsageException">Thrown when the algorithm is unsupported or the operation is unrecognized.</exception>
    /// <exception cref="CommandErrorException">Thrown when the target file does not exist or an I/O error occurs.</exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'operation' input
        var operation = GetMapString(inputs, "operation", variables) ??
                        throw new YamlException(step.Start, step.End, "'hash' command missing 'operation' input");

        // Get the 'algorithm' input
        var algorithm = GetMapString(inputs, "algorithm", variables) ??
                        throw new YamlException(step.Start, step.End, "'hash' command missing 'algorithm' input");

        // Get the 'file' input
        var file = GetMapString(inputs, "file", variables) ??
                   throw new YamlException(step.Start, step.End, "'hash' command missing 'file' input");

        // Do the hash operation
        DoHashOperation(context, operation, algorithm, file);
    }

    /// <summary>
    ///     Validates the algorithm and dispatches the hash operation to <see cref="GenerateSha256"/> or <see cref="VerifySha256"/>.
    /// </summary>
    /// <remarks>
    ///     The algorithm check lives here rather than in each leaf method so that unsupported algorithms
    ///     are rejected before any file I/O is attempted. This keeps the leaf methods focused on a single
    ///     algorithm and avoids duplicating the validation logic.
    /// </remarks>
    /// <param name="context">Program context</param>
    /// <param name="operation">Operation to perform (generate or verify)</param>
    /// <param name="algorithm">Hash algorithm</param>
    /// <param name="file">File to perform operation on</param>
    /// <exception cref="CommandUsageException">Thrown when the algorithm is not "sha256", or when the operation is not "generate" or "verify".</exception>
    public static void DoHashOperation(Context context, string operation, string algorithm, string file)
    {
        // Check the algorithm
        if (algorithm != "sha256")
        {
            throw new CommandUsageException($"'hash' command invalid algorithm '{algorithm}'");
        }

        // Process the operation
        switch (operation)
        {
            case "generate":
                GenerateSha256(file);
                break;

            case "verify":
                VerifySha256(context, file);
                break;

            default:
                throw new CommandUsageException($"'hash' command invalid operation '{operation}'");
        }
    }

    /// <summary>
    ///     Generate a SHA-256 hash for a file
    /// </summary>
    /// <remarks>
    ///     This is the generate path: it computes the digest via <see cref="CalculateSha256"/> and
    ///     persists it to a sidecar file so a later verify run can compare without re-reading the
    ///     original source. The sidecar write is non-atomic; callers should not run concurrent
    ///     generate calls on the same file.
    /// </remarks>
    /// <param name="file">File to generate hash for</param>
    /// <exception cref="CommandErrorException">
    ///     Thrown when <paramref name="file"/> does not exist or an I/O error occurs during hashing.
    /// </exception>
    public static void GenerateSha256(string file)
    {
        // Calculate the digest
        var digest = CalculateSha256(file);

        // Write the digest
        File.WriteAllText(file + ".sha256", digest);
    }

    /// <summary>
    ///     Verify a SHA-256 hash for a file
    /// </summary>
    /// <remarks>
    ///     This is the verify path: it reads the stored digest from the sidecar file, normalizes it
    ///     to lowercase (so sidecar files written by external tools that use uppercase hex still
    ///     compare correctly), recomputes the digest via <see cref="CalculateSha256"/>, and compares
    ///     the two. A missing sidecar is treated as an error rather than a silent pass to prevent
    ///     undetected integrity gaps.
    /// </remarks>
    /// <param name="context">Program context</param>
    /// <param name="file">Name of the file to verify</param>
    /// <exception cref="CommandErrorException">
    ///     Thrown when the sidecar hash file does not exist, when the target file does not exist,
    ///     or when the computed digest does not match the stored digest.
    /// </exception>
    public static void VerifySha256(Context context, string file)
    {
        // Check the hash file exists
        var hashFile = file + ".sha256";
        if (!File.Exists(hashFile))
        {
            throw new CommandErrorException($"Error: Could not find file '{hashFile}'");
        }

        // Read the digest, normalizing to lowercase so that uppercase sidecar files
        // written by external tools compare correctly against the computed lowercase digest
        var digest = File.ReadAllText(hashFile).Trim().ToLowerInvariant();

        // Calculate the digest
        var calculated = CalculateSha256(file);

        // Verify the digest
        if (digest != calculated)
        {
            throw new CommandErrorException($"Sha256 hash mismatch for '{file}'");
        }

        // Report the digest is OK
        context.WriteLine($"Sha256 Digest OK for '{file}'");
    }

    /// <summary>
    ///     Calculate the SHA-256 hash of a file
    /// </summary>
    /// <remarks>
    ///     File existence is checked explicitly with <see cref="File.Exists"/> before opening the
    ///     stream so that the error message is a controlled <see cref="CommandErrorException"/>
    ///     rather than an unhandled <see cref="FileNotFoundException"/>. The stream is opened with
    ///     <see cref="FileAccess.Read"/> to avoid acquiring an unnecessary write lock, which
    ///     allows concurrent readers and reduces the risk of permission errors on read-only files.
    /// </remarks>
    /// <param name="file">File to hash</param>
    /// <returns>SHA-256 hash as a lowercase hexadecimal string</returns>
    /// <exception cref="CommandErrorException">
    ///     Thrown when <paramref name="file"/> does not exist or an I/O exception occurs while
    ///     reading the file stream.
    /// </exception>
    public static string CalculateSha256(string file)
    {
        // Check the hash file exists
        if (!File.Exists(file))
        {
            throw new CommandErrorException($"Error: Could not find file '{file}'");
        }

        try
        {
            // Calculate the Sha256 digest of the file
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            throw new CommandErrorException($"Error calculating sha256 hash for '{file}': {ex.Message}");
        }
    }
}
