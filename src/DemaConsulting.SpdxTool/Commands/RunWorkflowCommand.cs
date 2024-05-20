using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command to run a workflow YAML file
/// </summary>
public class RunWorkflowCommand : Command
{
    /// <summary>
    /// Singleton instance of this command
    /// </summary>
    public static readonly RunWorkflowCommand Instance = new();

    /// <summary>
    /// Entry information for this command
    /// </summary>
    public static readonly CommandEntry Entry = new(
        "run-workflow",
        "run-workflow <workflow.yaml>",
        "Runs the workflow file",
        new[]
        {
            "This command runs the steps specified in the workflow.yaml file.",
            "",
            "From the command-line this can be used as:",
            "  spdx-tool run-workflow <workflow.yaml> [parameter=value] [parameter=value]...",
            "",
            "From a YAML file this can be used as:",
            "  - command: run-workflow",
            "    inputs:",
            "      file: <workflow.yaml>",
            "      parameters:",
            "        name: value",
            "        name: value"
        },
        Instance);

    /// <summary>
    /// Private constructor - this is a singleton
    /// </summary>
    private RunWorkflowCommand()
    {
    }

    /// <inheritdoc />
    public override void Run(string[] args)
    {
        // Report an error if the number of arguments is less than 1
        if (args.Length < 1)
            throw new CommandUsageException("'run-workflow' command missing arguments");

        // Parse the parameters
        var parameters = new Dictionary<string, string>();
        foreach (var arg in args.Skip(1))
        {
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
        Execute(args[0], parameters);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step, Dictionary<string, string> variables)
    {
        // Get the step inputs
        var inputs = GetMapMap(step, "input");

        // Get the 'file' input
        var file = GetMapString(inputs, "file", variables) ?? 
                   throw new YamlException(step.Start, step.End, "'run-workflow' command missing 'file' input");

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

        // Execute the workflow
        Execute(file, parameters);
    }

    /// <summary>
    /// Execute the workflow
    /// </summary>
    /// <param name="workflowFile">Workflow file</param>
    /// <param name="parameters">Workflow parameters</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    /// <exception cref="YamlException">On workflow error</exception>
    public static void Execute(string workflowFile, Dictionary<string, string> parameters)
    {
        // Verify the file exists
        if (!File.Exists(workflowFile))
            throw new CommandUsageException(
                $"File not found: {workflowFile}");

        try
        {
            // Load the document
            var yaml = new YamlStream();
            using var input = new StreamReader(workflowFile);
            yaml.Load(input);
            var root = yaml.Documents[0].RootNode as YamlMappingNode ??
                       throw new CommandErrorException(
                           $"Workflow {workflowFile} missing root mapping node");

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
                    throw new CommandErrorException(
                        $"Workflow {workflowFile} parameter {key} not defined");

                variables[key] = Expand(value, variables);
            }

            // Get the steps
            var steps = GetMapSequence(root, "steps") ??
                        throw new CommandErrorException(
                            $"Workflow {workflowFile} missing steps");

            // Execute the steps
            foreach (var stepNode in steps)
            {
                // Get the step
                var step = stepNode as YamlMappingNode ??
                           throw new CommandErrorException(
                               $"Workflow {workflowFile} step is not a map");

                // Get the command
                if (!step.Children.TryGetValue("command", out var commandNode))
                    throw new CommandErrorException(
                        $"Workflow {workflowFile} step missing command");

                // Execute the step
                var command = commandNode.ToString();
                if (!CommandsRegistry.Commands.TryGetValue(command, out var entry))
                    throw new CommandUsageException(
                        $"Unknown command: '{command}'");

                // Run the command
                entry.Instance.Run(step, variables);
            }
        }
        catch (KeyNotFoundException ex)
        {
            throw new CommandErrorException(
                $"Workflow {workflowFile} invalid", ex);
        }
        catch (YamlException ex)
        {
            throw new CommandErrorException(
                $"Workflow {workflowFile} invalid at {ex.Start} - {ex.Message}", ex);
        }
    }
}