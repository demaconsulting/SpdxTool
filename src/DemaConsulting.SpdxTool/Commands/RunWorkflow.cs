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
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
///     Command to run a workflow YAML file
/// </summary>
public sealed class RunWorkflow : Command
{
    /// <summary>
    ///     Command name
    /// </summary>
    private const string Command = "run-workflow";

    /// <summary>
    ///     Singleton instance of this command
    /// </summary>
    public static readonly RunWorkflow Instance = new();

    /// <summary>
    ///     Entry information for this command
    /// </summary>
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

    /// <inheritdoc />
    public override void Run(Context context, string[] args)
    {
        // Report an error if the number of arguments is less than 1
        if (args.Length < 1)
            throw new CommandUsageException("'run-workflow' command missing arguments");

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
                throw new CommandUsageException($"Invalid argument: {arg}");

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
            return;

        // Print the outputs
        context.WriteLine("Outputs:");
        foreach (var (key, value) in outputs)
            context.WriteLine($"  {key} = {value}");
    }

    /// <inheritdoc />
    public override void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "inputs");

        // Get the 'integrity' input
        var integrity = GetMapString(inputs, "integrity", variables);

        // Get the 'file' and 'url' inputs
        var file = GetMapString(inputs, "file", variables);
        var url = GetMapString(inputs, "url", variables);

        // Get the parameters
        var parameters = new Dictionary<string, string>();
        if (GetMapMap(inputs, "parameters") is { } parametersMap)
            // Process all the parameters
            foreach (var (keyNode, valueNode) in parametersMap.Children)
            {
                var key = keyNode.ToString();
                var value = valueNode.ToString();
                parameters[key] = Expand(value, variables);
            }

        // Run the workflow
        var outputs = Run(context, step, file, url, integrity, parameters);

        // Save any outputs
        if (GetMapMap(inputs, "outputs") is { } outputsMap)
            // Process all the outputs
            foreach (var (keyNode, valueNode) in outputsMap.Children)
            {
                var key = keyNode.ToString();
                var value = valueNode.ToString();
                if (!outputs.TryGetValue(key, out var output))
                    throw new CommandUsageException($"Workflow did not produce {key} output");

                variables[value] = output;
            }
    }

    /// <summary>
    ///     Execute the workflow
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="step">Step for reporting errors</param>
    /// <param name="file">Optional file</param>
    /// <param name="url">Optional URL</param>
    /// <param name="integrity">Optional integrity</param>
    /// <param name="parameters">Workflow parameters</param>
    /// <returns>Workflow outputs</returns>
    /// <exception cref="YamlException">on error</exception>
    public static Dictionary<string, string> Run(Context context, YamlMappingNode step, string? file, string? url,
        string? integrity, Dictionary<string, string> parameters)
    {
        // Fail if no source
        if (file != null && url != null)
            throw new YamlException(step.Start, step.End,
                "'run-workflow' command cannot specify both 'file' and 'url' inputs");

        // Run the file if specified
        if (file != null)
            return RunFile(context, file, integrity, parameters);

        // Run the URL if specified
        if (url != null)
            return RunUrl(context, url, integrity, parameters);

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
    /// <exception cref="CommandUsageException">On usage error</exception>
    /// <exception cref="YamlException">On workflow error</exception>
    public static Dictionary<string, string> RunFile(Context context, string workflowFile, string? integrity,
        Dictionary<string, string> parameters)
    {
        // Verify the file exists
        if (!File.Exists(workflowFile))
            throw new CommandUsageException(
                $"File not found: {workflowFile}");

        // Get the file bytes
        var bytes = File.ReadAllBytes(workflowFile);

        // Run the workflow
        return RunBytes(context, workflowFile, bytes, integrity, parameters);
    }

    /// <summary>
    ///     Run workflow from URL
    /// </summary>
    /// <param name="context">Program context</param>
    /// <param name="url">Workflow URL</param>
    /// <param name="integrity">Optional integrity hash</param>
    /// <param name="parameters">Workflow parameters</param>
    /// <returns>Workflow outputs</returns>
    /// <exception cref="CommandErrorException">on error</exception>
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
            throw new CommandErrorException($"Error {responseMessage.StatusCode} fetching {url}");

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
    public static Dictionary<string, string> RunBytes(Context context, string source, byte[] bytes, string? integrity,
        Dictionary<string, string> parameters)
    {
        // Optionally check the integrity before running
        if (integrity != null)
        {
            var hashBytes = SHA256.HashData(bytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            if (hash != integrity)
                throw new CommandErrorException($"Integrity check of {source} failed");
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
                // Process all the parameters
                foreach (var (keyNode, valueNode) in parametersMap.Children)
                {
                    var key = keyNode.ToString();
                    var value = Expand(valueNode.ToString(), variables);
                    variables[key] = Expand(value, parameters);
                }

            // Apply the provided parameters to our variables
            foreach (var (key, value) in parameters)
            {
                if (!variables.ContainsKey(key))
                    throw new CommandErrorException(
                        $"Workflow {source} parameter {key} not defined");

                variables[key] = Expand(value, variables);
            }

            // Get the steps
            var steps = GetMapSequence(root, "steps") ??
                        throw new CommandErrorException(
                            $"Workflow {source} missing steps");

            // Execute the steps
            foreach (var stepNode in steps)
            {
                // Get the step
                var step = stepNode as YamlMappingNode ??
                           throw new CommandErrorException(
                               $"Workflow {source} step is not a map");

                // Get the command
                var command = GetMapString(step, "command", variables) ??
                              throw new CommandErrorException(
                                  $"Workflow {source} step missing command");

                // Check for a displayName
                var displayName = GetMapString(step, "displayName", variables);
                if (displayName != null)
                    context.WriteLine(displayName);

                // Execute the step
                if (!CommandsRegistry.Commands.TryGetValue(command, out var entry))
                    throw new CommandUsageException(
                        $"Unknown command: '{command}'");

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
}