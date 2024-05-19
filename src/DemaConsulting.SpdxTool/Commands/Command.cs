using YamlDotNet.RepresentationModel;

namespace DemaConsulting.SpdxTool.Commands;

/// <summary>
/// Command base class
/// </summary>
public abstract class Command
{
    /// <summary>
    /// Run the command
    /// </summary>
    /// <param name="args">Command arguments</param>
    public abstract void Run(string[] args);

    /// <summary>
    /// Run the command
    /// </summary>
    /// <param name="step">Command step</param>
    public abstract void Run(YamlMappingNode step);
}