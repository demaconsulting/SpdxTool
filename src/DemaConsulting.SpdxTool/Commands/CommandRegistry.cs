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
        { AddPackageCommand.Entry.Name, AddPackageCommand.Entry },
        { CopyPackageCommand.Entry.Name, CopyPackageCommand.Entry },
        { QueryCommand.Entry.Name, QueryCommand.Entry },
        { RenameIdCommand.Entry.Name, RenameIdCommand.Entry },
        { RunWorkflowCommand.Entry.Name, RunWorkflowCommand.Entry },
        { Sha256Command.Entry.Name, Sha256Command.Entry },
        { ToMarkdownCommand.Entry.Name, ToMarkdownCommand.Entry }
    };

    /// <summary>
    /// Gets the commands
    /// </summary>
    public static IReadOnlyDictionary<string, CommandEntry> Commands => InternalCommands;
}