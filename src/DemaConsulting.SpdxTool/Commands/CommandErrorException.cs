namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command error exception
/// </summary>
public class CommandErrorException : Exception
{
    /// <summary>
    /// Initialize a new instance of the CommandErrorException class
    /// </summary>
    /// <param name="message">Command error message</param>
    public CommandErrorException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initialize a new instance of the CommandErrorException class
    /// </summary>
    /// <param name="message">Command error message</param>
    /// <param name="innerException">Inner exception cause</param>
    public CommandErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}