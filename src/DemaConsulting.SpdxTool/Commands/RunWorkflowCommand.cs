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
        "This command runs the steps specified in the workflow.yaml file.",
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
            throw new CommandUsageException("Missing workflow filename");

        // Execute the workflow
        Execute(args[0]);
    }

    /// <inheritdoc />
    public override void Run(YamlMappingNode step)
    {
        // Get the workflow file
        var file = step["file"]?.ToString() ?? throw new YamlException("Step missing file");

        // Execute the workflow
        Execute(file);
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
                       throw new YamlException(
                           "Invalid workflow file");

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
                var command = step["command"]?.ToString() ??
                              throw new YamlException(
                                  $"Workflow {workflowFile} step missing command");

                // Execute the step
                if (!CommandsRegistry.Commands.TryGetValue(command, out var entry))
                    throw new CommandUsageException(
                        $"Unknown command: {command}");

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
                $"Workflow {workflowFile} invalid at {ex.Start}", ex);
        }
    }
}