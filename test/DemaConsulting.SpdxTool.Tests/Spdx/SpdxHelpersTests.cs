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
using DemaConsulting.SpdxTool.Spdx;

namespace DemaConsulting.SpdxTool.Tests.Spdx;

/// <summary>
///     Tests for the <see cref="DemaConsulting.SpdxTool.Spdx.SpdxHelpers"/> class.
/// </summary>
public class SpdxHelpersTests
{
    /// <summary>
    ///     Minimal SPDX 2.3 JSON document for testing.
    /// </summary>
    private static readonly string MinimalSpdxJson = """
        {
          "SPDXID": "SPDXRef-DOCUMENT",
          "spdxVersion": "SPDX-2.3",
          "creationInfo": {
            "created": "2024-01-01T00:00:00Z",
            "creators": ["Tool: test"]
          },
          "name": "test-doc",
          "dataLicense": "CC0-1.0",
          "documentNamespace": "https://example.org/test-1",
          "documentDescribes": [],
          "packages": [],
          "relationships": []
        }
        """;

    /// <summary>
    ///     Test that SpdxHelpers.LoadJsonDocument with a missing file throws CommandUsageException
    /// </summary>
    [Fact]
    public void SpdxHelpers_LoadJsonDocument_MissingFile_ThrowsCommandUsageException()
    {
        // Arrange: path to a file that does not exist
        const string missingFile = "does-not-exist.spdx.json";

        // Act/Assert: loading a missing file throws CommandUsageException
        Assert.Throws<CommandUsageException>(() => SpdxHelpers.LoadJsonDocument(missingFile));
    }

    /// <summary>
    ///     Test that SpdxHelpers.LoadJsonDocument with a valid file returns a document
    /// </summary>
    [Fact]
    public void SpdxHelpers_LoadJsonDocument_ValidFile_ReturnsDocument()
    {
        // Arrange: write a minimal SPDX JSON file to a temporary path
        var spdxFile = Path.GetTempFileName() + ".spdx.json";
        File.WriteAllText(spdxFile, MinimalSpdxJson);

        try
        {
            // Act: load the document
            var doc = SpdxHelpers.LoadJsonDocument(spdxFile);

            // Assert: document was loaded with expected content
            Assert.NotNull(doc);
            Assert.Equal("test-doc", doc.Name);
        }
        finally
        {
            // Cleanup
            if (File.Exists(spdxFile))
            {
                File.Delete(spdxFile);
            }
        }
    }

    /// <summary>
    ///     Test that SpdxHelpers.SaveJsonDocument stamps the creator entry
    /// </summary>
    [Fact]
    public void SpdxHelpers_SaveJsonDocument_ValidDocument_StampsCreator()
    {
        // Arrange: deserialize a minimal document and prepare output path
        var doc = Spdx2JsonDeserializer.Deserialize(MinimalSpdxJson);
        var outputFile = Path.GetTempFileName() + ".spdx.json";

        try
        {
            // Act: save the document
            SpdxHelpers.SaveJsonDocument(doc, outputFile);

            // Assert: file was created and contains the tool creator entry
            Assert.True(File.Exists(outputFile));
            var content = File.ReadAllText(outputFile);
            Assert.Contains("DemaConsulting.SpdxTool", content);
        }
        finally
        {
            // Cleanup
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }
}
