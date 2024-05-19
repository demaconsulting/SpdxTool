namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Commands Registry
/// </summary>
public static class CommandsRegistry
{
    /// <summary>
    /// Dictionary of known commands
    /// </summary>
    private static readonly Dictionary<string, CommandEntry> InternalCommands = new()
    {
        { RunWorkflowCommand.Entry.Name, RunWorkflowCommand.Entry },
        { ToMarkdownCommand.Entry.Name, ToMarkdownCommand.Entry }
    };

    /// <summary>
    /// Gets the commands
    /// </summary>
    public static IReadOnlyDictionary<string, CommandEntry> Commands => InternalCommands;
}