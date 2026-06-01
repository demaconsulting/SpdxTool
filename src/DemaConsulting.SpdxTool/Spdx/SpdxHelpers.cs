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
///     Provides centralized file I/O operations for loading and saving SPDX 2.x JSON documents.
/// </summary>
/// <remarks>
///     Provides centralized file-system operations for loading and saving SPDX 2.x JSON
///     documents. All commands that read or write SPDX files use these helpers to ensure
///     consistent error handling and document stamping behavior. Not thread-safe for
///     concurrent access to the same file path. <see cref="SaveJsonDocument"/> temporarily
///     mutates the passed-in document's <c>CreationInformation.Creators</c> array during
///     serialization; the original array is always restored in a <c>finally</c> block.
/// </remarks>
public static class SpdxHelpers
{
    /// <summary>
    ///     Loads an SPDX 2.x JSON document from disk, throwing <see cref="Commands.CommandUsageException"/> when the file does not exist.
    /// </summary>
    /// <remarks>
    ///     Centralizing the file-existence check here ensures that every command that loads an
    ///     SPDX file reports the same exception type and message, simplifying caller error handling.
    ///     Reads the entire file synchronously and delegates deserialization to
    ///     <see cref="DemaConsulting.SpdxModel.IO.Spdx2JsonDeserializer"/>. Not thread-safe for
    ///     concurrent access to the same file path.
    /// </remarks>
    /// <param name="spdxFile">Path to the SPDX JSON file. Must not be null.</param>
    /// <returns>Fully deserialized SPDX document.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spdxFile"/> is null.</exception>
    /// <exception cref="Commands.CommandUsageException">Thrown when the specified file does not exist.</exception>
    public static SpdxDocument LoadJsonDocument(string spdxFile)
    {
        // Validate arguments
        ArgumentNullException.ThrowIfNull(spdxFile);

        // Verify the file exists
        if (!File.Exists(spdxFile))
        {
            throw new CommandUsageException($"File not found: {spdxFile}");
        }

        // Load the SPDX document
        var fileContent = File.ReadAllText(spdxFile);
        return Spdx2JsonDeserializer.Deserialize(fileContent);
    }

    /// <summary>
    ///     Stamps the tool creator entry and serializes an SPDX document to a JSON file, overwriting any existing file.
    /// </summary>
    /// <remarks>
    ///     Every command that writes an SPDX file calls this method to ensure the tool creator
    ///     entry is consistently stamped on every written document. The creator entry is appended
    ///     only if not already present, so re-saving a document does not produce duplicate entries.
    ///     Serialization is performed in memory before writing; I/O errors from
    ///     <see cref="System.IO.File.WriteAllText(string, string)"/> are not caught and propagate
    ///     to the caller.
    /// </remarks>
    /// <param name="doc">The SPDX document to serialize. Must not be null.</param>
    /// <param name="spdxFile">
    ///     Path to the output JSON file. Must not be null. Any existing file at this path is
    ///     overwritten.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="doc"/> or <paramref name="spdxFile"/> is null.</exception>
    public static void SaveJsonDocument(SpdxDocument doc, string spdxFile)
    {
        // Validate arguments
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(spdxFile);

        // Construct the tool name
        var toolName = $"Tool: DemaConsulting.SpdxTool-{Program.Version}";

        // Build a local modified creators list for serialization, leaving the original doc unchanged
        var originalCreators = doc.CreationInformation.Creators;
        var creators = originalCreators.Contains(toolName)
            ? originalCreators
            : [.. originalCreators.Append(toolName)];

        // Temporarily apply the serialization creators, serialize, then restore the originals
        doc.CreationInformation.Creators = creators;
        try
        {
            var serializedContent = Spdx2JsonSerializer.Serialize(doc);
            File.WriteAllText(spdxFile, serializedContent);
        }
        finally
        {
            doc.CreationInformation.Creators = originalCreators;
        }
    }
}
