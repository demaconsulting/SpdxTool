namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Exception thrown when a command is used incorrectly
/// </summary>
public class CommandUsageException : Exception
{
    /// <summary>
    /// Initializes a new instance of the CommandUsageException class
    /// </summary>
    /// <param name="message">Error message</param>
    public CommandUsageException(string message) : base(message)
    {
    }
}