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
/// Hash command
/// </summary>
public sealed class Hash : Command
{
    /// <summary>
    /// Command name
    /// </summary>
    private const string Command = "hash";

    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly Hash Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        Command,
        "hash <operation> <algorithm> <file>",
        "Generate or verify hashes of files",
        new[]
        {
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
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private Hash()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is not 3
        if (args.Length != 3)
            throw new CommandUsageException("'hash' command missing arguments");

        // Do the hash operation
        var operation = args[0];
        var algorithm = args[1];
        var file = args[2];
        DoHashOperation(operation, algorithm, file);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
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
        DoHashOperation(operation, algorithm, file);
    }

    /// <summary>
    /// Do the requested Sha256 operation
    /// </summary>
    /// <param name="operation">Operation to perform (generate or verify)</param>
    /// <param name="algorithm">Hash algorithm</param>
    /// <param name="file">File to perform operation on</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    public static void DoHashOperation(string operation, string algorithm, string file)
    {
        // Check the algorithm
        if (algorithm != "sha256")
            throw new CommandUsageException($"'hash' command invalid algorithm '{algorithm}'");

        // Process the operation
        switch (operation)
        {
            case "generate":
                GenerateSha256(file);
                break;

            case "verify":
                VerifySha256(file);
                break;

            default:
                throw new CommandUsageException($"'hash' command invalid operation '{operation}'");
        }
    }

    /// <summary>
    /// Generate a Sha256 hash for a file
    /// </summary>
    /// <param name="file">File to generate hash for</param>
    public static void GenerateSha256(string file)
    {
        // Calculate the digest
        var digest = CalculateSha256(file);

        // Write the digest
        File.WriteAllText(file + ".sha256", digest);
    }

    /// <summary>
    /// Verify a Sha256 hash for a file
    /// </summary>
    /// <param name="file"></param>
    /// <exception cref="CommandErrorException"></exception>
    public static void VerifySha256(string file)
    {
        // Check the hash file exists
        var hashFile = file + ".sha256";
        if (!File.Exists(hashFile))
            throw new CommandErrorException($"Error: Could not find file '{hashFile}'");

        // Read the digest
        var digest = File.ReadAllText(hashFile).Trim();

        // Calculate the digest
        var calculated = CalculateSha256(file);

        // Verify the digest
        if (digest != calculated)
            throw new CommandErrorException($"Sha256 hash mismatch for '{file}'");

        // Report the digest is OK
        Console.WriteLine($"Sha256 Digest OK for '{file}'");
    }

    /// <summary>
    /// Calculate the Sha256 hash of a file
    /// </summary>
    /// <param name="file">File to hash</param>
    /// <returns>Sh256 hash</returns>
    /// <exception cref="CommandErrorException">On error</exception>
    public static string CalculateSha256(string file)
    {
        // Check the hash file exists
        if (!File.Exists(file))
            throw new CommandErrorException($"Error: Could not find file '{file}'");

        try
        {
            // Calculate the Sha256 digest of the file
            using var stream = new FileStream(file, FileMode.Open);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            throw new CommandErrorException($"Error calculating sha256 hash for '{file}': {ex.Message}");
        }
    }
}