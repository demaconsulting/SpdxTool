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
        { AddRelationship.Entry.Name, AddRelationship.Entry },
        { CopyPackage.Entry.Name, CopyPackage.Entry },
        { FindPackage.Entry.Name, FindPackage.Entry },
        { Hash.Entry.Name, Hash.Entry },
        { Print.Entry.Name, Print.Entry },
        { Query.Entry.Name, Query.Entry },
        { RenameId.Entry.Name, RenameId.Entry },
        { RunWorkflow.Entry.Name, RunWorkflow.Entry },
        { ToMarkdown.Entry.Name, ToMarkdown.Entry },
        { UpdatePackage.Entry.Name, UpdatePackage.Entry },
        { Validate.Entry.Name, Validate.Entry }
    };

    /// <summary>
    /// Gets the commands
    /// </summary>
    public static IReadOnlyDictionary<string, CommandEntry> Commands => InternalCommands;
}