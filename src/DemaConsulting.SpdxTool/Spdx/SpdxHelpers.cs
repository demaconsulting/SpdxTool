using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;
using DemaConsulting.SpdxTool.Commands;

namespace DemaConsulting.SpdxTool.Spdx;

/// <summary>
/// SPDX Helpers Class
/// </summary>
public static class SpdxHelpers
{
    /// <summary>
    /// Load an SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX document file name</param>
    /// <returns>SPDX document</returns>
    public static SpdxDocument LoadJsonDocument(string spdxFile)
    {
        // Verify to file exists
        if (!File.Exists(spdxFile))
            throw new CommandUsageException($"File not found: {spdxFile}");

        // Load the SPDX document
        return Spdx2JsonDeserializer.Deserialize(File.ReadAllText(spdxFile));
    }

    /// <summary>
    /// Save an SPDX document
    /// </summary>
    /// <param name="doc">SPDX document</param>
    /// <param name="spdxFile">SPDX document file name</param>
    public static void SaveJsonDocument(SpdxDocument doc, string spdxFile)
    {
        // Construct the tool name
        var toolName = $"Tool: DemaConsulting.SpdxTool-{Program.Version}";

        // Add this tool if missing
        if (!doc.CreationInformation.Creators.Contains(toolName))
            doc.CreationInformation.Creators = doc.CreationInformation.Creators.Append(toolName).ToArray();

        // Save the document
        File.WriteAllText(spdxFile, Spdx2JsonSerializer.Serialize(doc));
    }
}