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
        { Help.Entry.Name, Help.Entry },
        { AddPackage.Entry.Name, AddPackage.Entry },
        { CopyPackage.Entry.Name, CopyPackage.Entry },
        { FindPackage.Entry.Name, FindPackage.Entry },
        { Print.Entry.Name, Print.Entry },
        { Query.Entry.Name, Query.Entry },
        { RenameIdCommand.Entry.Name, RenameIdCommand.Entry },
        { RunWorkflow.Entry.Name, RunWorkflow.Entry },
        { Sha256Command.Entry.Name, Sha256Command.Entry },
        { ToMarkdown.Entry.Name, ToMarkdown.Entry }
    };

    /// <summary>
    /// Gets the commands
    /// </summary>
    public static IReadOnlyDictionary<string, CommandEntry> Commands => InternalCommands;
}