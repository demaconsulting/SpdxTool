namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command Entry record
/// </summary>
/// <param name="Name">Command name</param>
/// <param name="CommandLine">Command line example</param>
/// <param name="Summary">Command summary</param>
/// <param name="Description">Command detailed description</param>
/// <param name="Instance">Command instance</param>
public record CommandEntry(string Name, string CommandLine, string Summary, string Description, Command Instance);