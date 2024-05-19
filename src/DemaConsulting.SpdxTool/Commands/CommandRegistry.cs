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
        { HelpCommand.Entry.Name, HelpCommand.Entry },
        { RunWorkflowCommand.Entry.Name, RunWorkflowCommand.Entry },
        { ToMarkdownCommand.Entry.Name, ToMarkdownCommand.Entry },
        { RenameIdCommand.Entry.Name, RenameIdCommand.Entry },
        { CopyPackageCommand.Entry.Name, CopyPackageCommand.Entry }
    };

    /// <summary>
    /// Gets the commands
    /// </summary>
    public static IReadOnlyDictionary<string, CommandEntry> Commands => InternalCommands;
}