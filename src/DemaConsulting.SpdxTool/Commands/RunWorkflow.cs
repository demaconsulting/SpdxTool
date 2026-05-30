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

using System.Net;
using System.Security.Cryptography;
using DemaConsulting.NuGet.Caching;
using DemaConsulting.SpdxTool.Utility;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Command to run a workflow YAML file, URL, or NuGet package workflow
/// </summary>
/// <remarks>
///     RunWorkflow is a stateless singleton that implements the run-workflow command. It supports
///     three workflow sources: a local file path, an HTTP/HTTPS URL, and a NuGet package. The
///     optional <c>integrity</c> input performs a SHA-256 hash check before execution. Workflow
///     parameters flow in via the <c>parameters</c> map and outputs are captured from the returned
///     variable dictionary. This class is thread-safe for concurrent calls on independent workflow
///     files or URLs; concurrent calls sharing a mutable <see cref="Context"/> are not recommended.
/// </remarks>
public sealed class RunWorkflow : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "run-workflow";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    /// <remarks>
    ///     The singleton is registered with <see cref="CommandsRegistry"/> at startup so that both
    ///     CLI dispatch and workflow YAML dispatch route to the same instance.
    /// </remarks>
    public static readonly RunWorkflow Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
    /// <remarks>
    ///     The entry record associates the command name, usage string, help lines, and singleton
    ///     instance for registration with <see cref="CommandsRegistry"/>.
    /// </remarks>
    public static readonly CommandEntry Entry = new(
        Command,
        "run-workflow <workflow.yaml>",
        "Runs the workflow file/url",
        [
            "This command runs the steps specified in the workflow file/url.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool run-workflow <workflow.yaml> [parameter=value] [parameter=value]...",
            "",
            "From a YAML file this can be used as:",
            "  - command: run-workflow",
            "    inputs:",
            "      file: <workflow.yaml>         # Optional workflow file",
            "      url: <url>                    # Optional workflow url",
            "      nuget: <package:version>      # Optional NuGet package",
            "      integrity: <sha256>           # Optional workflow integrity check",
            "      parameters:",
            "        name: <value>               # Optional workflow parameter",
            "        name: <value>               # Optional workflow parameter",
            "      outputs:",
            "        name: <variable>            # Optional output to save to variable",
            "        name: <variable>            # Optional output to save to variable"
        ],
        Instance);

    /// <summary>
    ///     Private constructor - this is a singleton
    /// </summary>
    private RunWorkflow()
    {
    }

    /// <summary>
    ///     Runs the run-workflow command from the CLI.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="args">
    ///     Command-line arguments. Must contain at least one element: the workflow file path or
    ///     URL. Remaining elements may be <c>key=value</c> parameter pairs or the <c>--verbose</c>
    ///     flag.
    /// </param>
    /// <exception cref="CommandUsageException">
    ///     Thrown when no arguments are supplied, or when a parameter argument does not contain
    ///     the <c>=</c> separator.
    /// </exception>
    public override void Run(Context context, string[] args)
    {
        // Report an error if the number of arguments is less than 1
        if (args.Length < 1)
        {
            throw new CommandUsageException("'run-workflow' command missing arguments");
        }

        var name = args[0];

        // Parse the parameters
        var verbose = false;
        var parameters = new Dictionary<string, string>();
        foreach (var arg in args.Skip(1))
        {
            // Check for verbose flag
            if (arg == "--verbose")
            {
                verbose = true;
                continue;
            }

            // Verify the parameter is in the form key=value
            var sep = arg.IndexOf('=');
            if (sep < 0)
            {
                throw new CommandUsageException($"Invalid argument: {arg}");
            }

            // Add the parameter
            var key = arg[..sep];
            var value = arg[(sep + 1)..];
            parameters[key] = value;
        }

        // Execute the workflow
        var outputs = name.StartsWith("http")
            ? RunUrl(context, name, null, parameters)
            : RunFile(context, name, null, parameters);

        // Skip if not verbose
        if (!verbose)
        {
            return;
        }

        // Print the outputs
        context.WriteLine("Outputs:");
        foreach (var (key, value) in outputs)
        {
            context.WriteLine($"  {key} = {value}");
        }
    }

    /// <summary>
    ///     Runs the run-workflow command from a YAML workflow step.
    /// </summary>
    /// <param name="context">Program context used for output.</param>
    /// <param name="step">YAML step node containing the inputs.</param>
    /// <param name="variables">
    ///     Caller's workflow variable map; any declared output variables are written back into
    ///     this dictionary after execution.
    /// </param>
    /// <exception cref="CommandUsageException">
    ///     Thrown when a declared output variable is not present in the workflow's output map
    ///     after execution.
    /// </exception>
    /// <exception cref="YamlException">
    ///     Thrown when both <c>file</c> and <c>url</c> inputs are specified, when neither is
    ///     specified, when both <c>nuget</c> and <c>url</c> are specified, when <c>nuget</c> is
    ///     used without a <c>file</c> input, or when the <c>nuget</c> value is not in
    ///     <c>PackageName:version</c> format.
    /// </exception>
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'integrity' input
        var integrity = GetMapString(inputs, "integrity", variables);

        // Get the 'file' and 'url' inputs
        var file = GetMapString(inputs, "file", variables);
        var url = GetMapString(inputs, "url", variables);

        // Get the 'nuget' input
        var nuget = GetMapString(inputs, "nuget", variables);

        // If nuget is specified, resolve the workflow file from the NuGet package
        if (nuget != null)
        {
            file = ResolveNuGetFile(step, nuget, file, url);
            url = null;
        }

        // Get the parameters
        var parameters = new Dictionary<string, string>();
        if (GetMapMap(inputs, "parameters") is { } parametersMap)
        {
            // Process all the parameters
            foreach (var (keyNode, valueNode) in parametersMap.Children)
            {
                var key = keyNode.ToString();
                var value = valueNode.ToString();
                parameters[key] = Expand(value, variables);
            }
        }

        // Run the workflow
        var outputs = Run(context, step, file, url, integrity, parameters);

        // Save any outputs
        if (GetMapMap(inputs, "outputs") is { } outputsMap)
        {
            // Process all the outputs
            foreach (var (keyNode, valueNode) in outputsMap.Children)
            {
                var key = keyNode.ToString();
                var value = valueNode.ToString();
                if (!outputs.TryGetValue(key, out var output))
                {
                    throw new CommandUsageException($"Workflow did not produce {key} output");
                }

                variables[value] = output;
            }
        }
    }

    /// <summary>
    ///     Execute the workflow
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="step">Step for reporting errors</param>
    /// <param name="file">
    ///     Local file path of the workflow to execute. Exactly one of <paramref name="file"/> or
    ///     <paramref name="url"/> must be non-null, unless the caller has already resolved a NuGet
    ///     source into a file path (in which case <paramref name="url"/> is null and this provides
    ///     the resolved path). Pass <see langword="null"/> when specifying a URL source.
    /// </param>
    /// <param name="url">
    ///     HTTP or HTTPS URL of the workflow to execute. Exactly one of <paramref name="file"/> or
    ///     <paramref name="url"/> must be non-null. Pass <see langword="null"/> when specifying a
    ///     local file source. Providing both a non-null <paramref name="file"/> and a non-null
    ///     <paramref name="url"/> is an error.
    /// </param>
    /// <param name="integrity">Optional integrity</param>
    /// <param name="parameters">
    ///     Workflow parameter values to pass into the sub-workflow. May be an empty dictionary
    ///     but must not be <see langword="null"/>. Each key must match a parameter name declared
    ///     in the target workflow's <c>parameters</c> section; undeclared keys cause a
    ///     <see cref="CommandErrorException"/>.
    /// </param>
    /// <returns>Workflow outputs</returns>
    /// <exception cref="YamlException">
    ///     Thrown when both <paramref name="file"/> and <paramref name="url"/> are non-null
    ///     (ambiguous source), or when both are null (no source provided).
    /// </exception>
    public static Dictionary<string, string> Run(Context context, YamlMappingNode step, string? file, string? url,
        string? integrity, Dictionary<string, string> parameters)
    {
        // Fail if no source
        if (file != null && url != null)
        {
            throw new YamlException(step.Start, step.End,
                "'run-workflow' command cannot specify both 'file' and 'url' inputs");
        }

        // Run the file if specified
        if (file != null)
        {
            return RunFile(context, file, integrity, parameters);
        }

        // Run the URL if specified
        if (url != null)
        {
            return RunUrl(context, url, integrity, parameters);
        }

        // No source provided
        throw new YamlException(step.Start, step.End,
            "'run-workflow' command must specify either 'file' or 'url' input");
    }

    /// <summary>
    ///     Execute the workflow
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="workflowFile">Workflow file</param>
    /// <param name="integrity">Optional integrity hash</param>
    /// <param name="parameters">Workflow parameters</param>
    /// <returns>Workflow outputs</returns>
    /// <exception cref="CommandUsageException">
    ///     Thrown when the file specified by <paramref name="workflowFile"/> does not exist on disk.
    /// </exception>
    /// <exception cref="CommandErrorException">
    ///     Propagated from <see cref="RunBytes"/> when the integrity check fails, the YAML
    ///     structure is invalid, or a workflow step references an unknown command.
    /// </exception>
    public static Dictionary<string, string> RunFile(Context context, string workflowFile, string? integrity,
        Dictionary<string, string> parameters)
    {
        // Verify the file exists
        if (!File.Exists(workflowFile))
        {
            throw new CommandUsageException(
                $"File not found: {workflowFile}");
        }

        // Get the file bytes
        var bytes = File.ReadAllBytes(workflowFile);

        // Run the workflow
        return RunBytes(context, workflowFile, bytes, integrity, parameters);
    }

    /// <summary>
    ///     Run workflow from URL
    /// </summary>
    /// <remarks>
    ///     Blocks on the async HTTP operations using <c>.Result</c>. This is safe because
    ///     SpdxTool runs as a console application without a synchronization context that could
    ///     cause a deadlock.
    /// </remarks>
    /// <param name="context">Program context</param>
    /// <param name="url">Workflow URL</param>
    /// <param name="integrity">Optional integrity hash</param>
    /// <param name="parameters">Workflow parameters</param>
    /// <returns>Workflow outputs</returns>
    /// <exception cref="CommandErrorException">
    ///     Thrown when the HTTP response for <paramref name="url"/> is not HTTP 200 OK; also
    ///     propagated from <see cref="RunBytes"/> when the integrity check fails or the workflow
    ///     structure is invalid.
    /// </exception>
    public static Dictionary<string, string> RunUrl(Context context, string url, string? integrity,
        Dictionary<string, string> parameters)
    {
        // Construct the client handler to use the system proxy
        var handler = new HttpClientHandler
        {
            DefaultProxyCredentials = CredentialCache.DefaultCredentials,
            Proxy = WebRequest.GetSystemWebProxy(),
            PreAuthenticate = true
        };

        // Construct the HTTP client
        using var client = new HttpClient(handler);

        // Execute the Get request on the server
        var getTask = client.GetAsync(url);

        // Get the result (blocks until result available)
        var responseMessage = getTask.Result;
        if (responseMessage.StatusCode != HttpStatusCode.OK)
        {
            throw new CommandErrorException($"Error {responseMessage.StatusCode} fetching {url}");
        }

        // Get the content bytes (blocks until data available)
        var bytesTask = responseMessage.Content.ReadAsByteArrayAsync();
        var bytes = bytesTask.Result;

        // Run the workflow
        return RunBytes(context, url, bytes, integrity, parameters);
    }

    /// <summary>
    ///     Execute the workflow from Yaml bytes (from file, url, etc.)
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="source">Yaml source</param>
    /// <param name="bytes">Yaml bytes</param>
    /// <param name="integrity">Optional integrity hash</param>
    /// <param name="parameters">Parameters</param>
    /// <returns>Workflow outputs</returns>
    /// <exception cref="CommandErrorException">
    ///     Thrown when the integrity hash does not match the computed SHA-256 hash of
    ///     <paramref name="bytes"/>, when the YAML root node is not a mapping node, when the
    ///     <c>steps</c> key is absent from the root mapping, when a step node is not a mapping
    ///     node, when a provided parameter name is not declared in the workflow's
    ///     <c>parameters</c> section, or when the YAML is structurally invalid.
    /// </exception>
    public static Dictionary<string, string> RunBytes(Context context, string source, byte[] bytes, string? integrity,
        Dictionary<string, string> parameters)
    {
        // Optionally check the integrity before running
        if (integrity != null)
        {
            var hashBytes = SHA256.HashData(bytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            if (hash != integrity)
            {
                throw new CommandErrorException($"Integrity check of {source} failed");
            }
        }

        try
        {
            // Load the document
            var yaml = new YamlStream();
            using var input = new StreamReader(new MemoryStream(bytes));
            yaml.Load(input);
            var root = yaml.Documents[0].RootNode as YamlMappingNode ??
                       throw new CommandErrorException(
                           $"Workflow {source} missing root mapping node");

            // Process the parameters definitions into local variables
            var variables = new Dictionary<string, string>();
            if (GetMapMap(root, "parameters") is { } parametersMap)
            {
                // Process all the parameters
                foreach (var (keyNode, valueNode) in parametersMap.Children)
                {
                    var key = keyNode.ToString();
                    var value = Expand(valueNode.ToString(), variables);
                    variables[key] = Expand(value, parameters);
                }
            }

            // Apply the provided parameters to our variables
            foreach (var (key, value) in parameters)
            {
                if (!variables.ContainsKey(key))
                {
                    throw new CommandErrorException(
                        $"Workflow {source} parameter {key} not defined");
                }

                variables[key] = Expand(value, variables);
            }

            // Get the steps
            var steps = GetMapSequence(root, "steps") ??
                        throw new CommandErrorException(
                            $"Workflow {source} missing steps");

            // Execute the steps
            var mappedSteps = steps.Select(stepNode => stepNode as YamlMappingNode ??
                                                        throw new CommandErrorException(
                                                            $"Workflow {source} step is not a map")).ToArray();

            foreach (var step in mappedSteps)
            {
                // Get the command
                var command = GetMapString(step, "command", variables) ??
                              throw new CommandErrorException(
                                  $"Workflow {source} step missing command");

                // Check for a displayName
                var displayName = GetMapString(step, "displayName", variables);
                if (displayName != null)
                {
                    context.WriteLine(displayName);
                }

                // Execute the step
                if (!CommandsRegistry.Commands.TryGetValue(command, out var entry))
                {
                    throw new CommandUsageException(
                        $"Unknown command: '{command}'");
                }

                // Run the command
                entry.Instance.Run(context, step, variables);
            }

            // Return our variables as the output
            return variables;
        }
        catch (KeyNotFoundException ex)
        {
            throw new CommandErrorException(
                $"Workflow {source} invalid", ex);
        }
        catch (YamlException ex)
        {
            throw new CommandErrorException(
                $"Workflow {source} invalid at {ex.Start} - {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Resolve a workflow file path from a NuGet package
    /// </summary>
    /// <param name="step">Step for reporting errors</param>
    /// <param name="nuget">NuGet package specification (PackageName:version)</param>
    /// <param name="file">File path within the NuGet package</param>
    /// <param name="url">URL (must be null when nuget is specified)</param>
    /// <returns>Resolved file path</returns>
    /// <exception cref="YamlException">
    ///     Thrown when <paramref name="url"/> is non-null while <paramref name="nuget"/> is
    ///     specified (the two source types are mutually exclusive); thrown when
    ///     <paramref name="file"/> is null while <paramref name="nuget"/> is specified (a relative
    ///     path within the package is required); thrown when <paramref name="nuget"/> does not
    ///     contain the <c>:</c> separator expected by the <c>PackageName:version</c> format.
    /// </exception>
    private static string ResolveNuGetFile(YamlMappingNode step, string nuget, string? file, string? url)
    {
        // Cannot specify both nuget and url
        if (url != null)
        {
            throw new YamlException(step.Start, step.End,
                "'run-workflow' command cannot specify both 'nuget' and 'url' inputs");
        }

        // File must be specified with nuget
        if (file == null)
        {
            throw new YamlException(step.Start, step.End,
                "'run-workflow' command requires 'file' input when 'nuget' is specified");
        }

        // Parse the nuget value "PackageName:version"
        var sep = nuget.IndexOf(':');
        if (sep < 0)
        {
            throw new YamlException(step.Start, step.End,
                "'run-workflow' nuget parameter must be in format 'PackageName:version'");
        }

        var packageId = nuget[..sep];
        var version = nuget[(sep + 1)..];

        // Get the package path from NuGet cache - blocking synchronously using
        // GetAwaiter().GetResult() to match the pattern used by RunUrl (which also
        // blocks on async HTTP operations). This is safe because SpdxTool runs as
        // a console application without a synchronization context that could deadlock.
        var packagePath = NuGetCache.EnsureCachedAsync(packageId, version).GetAwaiter().GetResult();

        // Construct the full file path using safe path combination to prevent path traversal
        return PathHelpers.SafePathCombine(packagePath, file);
    }
}
