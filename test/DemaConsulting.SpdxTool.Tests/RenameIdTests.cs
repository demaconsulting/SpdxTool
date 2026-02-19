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

using DemaConsulting.SpdxModel.IO;

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Tests for the 'rename-id' command.
/// </summary>
[TestClass]
public class RenameIdTests
{
    /// <summary>
    ///     Test the 'rename-id' command with missing arguments.
    /// </summary>
    [TestMethod]
    public void RenameId_MissingArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "rename-id");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'rename-id' command missing arguments", output);
    }

    /// <summary>
    ///     Test the 'rename-id' command with missing SPDX file.
    /// </summary>
    [TestMethod]
    public void RenameId_MissingFile()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "rename-id",
            "missing.spdx.json",
            "SPDXRef-Package-1",
            "SPDXRef-Package-2");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test the 'rename-id' command.
    /// </summary>
    [TestMethod]
    public void RenameId()
    {
        const string spdxContents =
            """
            {
              "files": [],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package-1",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-1",
                  "relationshipType": "DESCRIBES"
                }
              ],
              "spdxVersion": "SPDX-2.2",
              "dataLicense": "CC0-1.0",
              "SPDXID": "SPDXRef-DOCUMENT",
              "name": "Test Document",
              "documentNamespace": "https://sbom.spdx.org",
              "creationInfo": {
                "created": "2021-10-01T00:00:00Z",
                "creators": [ "Person: Malcolm Nixon" ]
              },
              "documentDescribes": [ "SPDXRef-Package-1" ]
            }
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("test.spdx.json", spdxContents);

            // Act: Run the tool
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "rename-id",
                "test.spdx.json",
                "SPDXRef-Package-1",
                "SPDXRef-Package-2");

            // Assert: Verify the conversion succeeded
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("test.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("test.spdx.json"));

            // Assert: Verify the SPDX ID was updated
            Assert.AreEqual("SPDXRef-Package-2", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.AreEqual("SPDXRef-Package-2", doc.Describes[0]);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }
}
