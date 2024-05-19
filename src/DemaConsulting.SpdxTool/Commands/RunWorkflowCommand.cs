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
            "  spdx-tool run-workflow <workflow.yaml>",
            "",
            "From a YAML file this can be used as:",
            "  - command: run-workflow",
            "    file: <workflow.yaml>"
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
        // Report an error if the number of arguments is not 1
        if (args.Length != 1)
            throw new CommandUsageException("'run-workflow' command missing arguments");

        // Execute the workflow
        Execute(args[0]);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step)
    {
        // Get the workflow filename
        if (!step.Children.TryGetValue("file", out var file))
            throw new YamlException(step.Start, step.End, "'run-workflow' command missing 'file' parameter");

        // Execute the workflow
        Execute(file.ToString());
    }

    /// <summary>
    /// Execute the workflow
    /// </summary>
    /// <param name="workflowFile">Workflow file</param>
    /// <exception cref="CommandUsageException">On usage error</exception>
    /// <exception cref="YamlException">On workflow error</exception>
    public static void Execute(string workflowFile)
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

            // Get the steps
            var steps = root["steps"] as YamlSequenceNode ??
                        throw new CommandErrorException(
                            $"Workflow {workflowFile} missing steps");
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
                entry.Instance.Run(step);
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