// Copyright (c) 2024 DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.SpdxModel;
using DemaConsulting.SpdxModel.IO;
using DemaConsulting.SpdxTool.Commands;

namespace DemaConsulting.SpdxTool.Spdx;

/// <summary>
///     SPDX Helpers Class
/// </summary>
public static class SpdxHelpers
{
    /// <summary>
    ///     Load an SPDX document
    /// </summary>
    /// <param name="spdxFile">SPDX document file name</param>
    /// <returns>SPDX document</returns>
    public static SpdxDocument LoadJsonDocument(string spdxFile)
    {
        // Verify to file exists
        if (!File.Exists(spdxFile))
            throw new CommandUsageException($"File not found: {spdxFile}");

        // Load the SPDX document
        var fileContent = File.ReadAllText(spdxFile);
        return Spdx2JsonDeserializer.Deserialize(fileContent);
    }

    /// <summary>
    ///     Save an SPDX document
    /// </summary>
    /// <param name="doc">SPDX document</param>
    /// <param name="spdxFile">SPDX document file name</param>
    public static void SaveJsonDocument(SpdxDocument doc, string spdxFile)
    {
        // Construct the tool name
        var toolName = $"Tool: DemaConsulting.SpdxTool-{Program.Version}";

        // Add this tool if missing
        if (!doc.CreationInformation.Creators.Contains(toolName))
            doc.CreationInformation.Creators = [.. doc.CreationInformation.Creators.Append(toolName)];

        // Save the document
        var serializedContent = Spdx2JsonSerializer.Serialize(doc);
        File.WriteAllText(spdxFile, serializedContent);
    }
}
