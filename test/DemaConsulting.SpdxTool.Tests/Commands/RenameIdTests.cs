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

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'rename-id' command.
/// </summary>
[Collection("CommandSequential")]
public class RenameIdTests
{
    /// <summary>
    ///     Test that rename-id command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void RenameId_Run_MissingArguments_ReportsError()
    {
        // Arrange: no setup required

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "rename-id");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'rename-id' command missing arguments", output);
    }

    /// <summary>
    ///     Test that rename-id command with missing file reports an error
    /// </summary>
    [Fact]
    public void RenameId_Run_MissingFile_ReportsError()
    {
        // Arrange: no setup required

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
        Assert.Equal(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test that rename-id command with valid SPDX file renames the ID
    /// </summary>
    [Fact]
    public void RenameId_Run_ValidSpdxFile_RenamesId()
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
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("test.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("test.spdx.json"));

            // Assert: Verify the SPDX ID was updated
            Assert.Equal("SPDXRef-Package-2", doc.Packages[0].Id);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.Equal("SPDXRef-Package-2", doc.Describes[0]);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that rename-id command with valid SPDX file renames all collections
    /// </summary>
    [Fact]
    public void RenameId_Run_ValidSpdxFile_RenamesAllCollections()
    {
        const string spdxContents =
            """
            {
              "files": [
                {
                  "SPDXID": "SPDXRef-File-1",
                  "fileName": "./test-file.txt",
                  "licenseConcluded": "MIT",
                  "copyrightText": "NOASSERTION"
                }
              ],
              "snippets": [
                {
                  "SPDXID": "SPDXRef-Snippet-1",
                  "snippetFromFile": "SPDXRef-File-1",
                  "ranges": [
                    {
                      "startPointer": { "offset": 0, "reference": "SPDXRef-File-1" },
                      "endPointer": { "offset": 10, "reference": "SPDXRef-File-1" }
                    }
                  ],
                  "licenseConcluded": "MIT",
                  "copyrightText": "NOASSERTION"
                }
              ],
              "packages": [    {
                  "SPDXID": "SPDXRef-Package-1",
                  "name": "Test Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT",
                  "hasFiles": ["SPDXRef-File-1"]
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-1",
                  "relationshipType": "DESCRIBES"
                },
                {
                  "spdxElementId": "SPDXRef-File-1",
                  "relatedSpdxElement": "SPDXRef-Package-1",
                  "relationshipType": "CONTAINED_BY"
                },
                {
                  "spdxElementId": "SPDXRef-Package-1",
                  "relatedSpdxElement": "SPDXRef-File-1",
                  "relationshipType": "CONTAINS"
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
                "SPDXRef-File-1",
                "SPDXRef-File-2");

            // Assert: Verify the conversion succeeded
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("test.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("test.spdx.json"));

            // Assert: Verify the file ID was updated
            Assert.Equal("SPDXRef-File-2", doc.Files[0].Id);

            // Assert: Verify the snippet from-file reference was updated
            Assert.Equal("SPDXRef-File-2", doc.Snippets[0].SnippetFromFile);

            // Assert: Verify the package hasFiles entry was updated
            Assert.Equal("SPDXRef-File-2", doc.Packages[0].HasFiles[0]);

            // Assert: Verify the relationship from-element was updated
            Assert.Equal("SPDXRef-File-2", doc.Relationships[1].Id);

            // Assert: Verify the relationship to-element was updated
            Assert.Equal("SPDXRef-File-2", doc.Relationships[2].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("test.spdx.json");
        }
    }

    /// <summary>
    ///     Test that Rename throws a usage exception when the old ID is empty
    /// </summary>
    [Fact]
    public void RenameId_Rename_EmptyOldId_ThrowsException()
    {
        // Arrange: empty document (exception raised before any document access)
        var doc = new SpdxDocument
        {
            Packages = [],
            Files = [],
            Snippets = [],
            Relationships = [],
            Describes = []
        };

        // Act / Assert: empty old ID is invalid
        Assert.Throws<CommandUsageException>(() => RenameId.Rename(doc, "", "SPDXRef-New"));
    }

    /// <summary>
    ///     Test that Rename throws a usage exception when the old ID is SPDXRef-DOCUMENT
    /// </summary>
    [Fact]
    public void RenameId_Rename_OldIdIsDocument_ThrowsException()
    {
        // Arrange: empty document (exception raised before any document access)
        var doc = new SpdxDocument
        {
            Packages = [],
            Files = [],
            Snippets = [],
            Relationships = [],
            Describes = []
        };

        // Act / Assert: SPDXRef-DOCUMENT is a reserved ID and cannot be renamed
        Assert.Throws<CommandUsageException>(() => RenameId.Rename(doc, "SPDXRef-DOCUMENT", "SPDXRef-New"));
    }

    /// <summary>
    ///     Test that Rename throws a usage exception when the new ID is empty
    /// </summary>
    [Fact]
    public void RenameId_Rename_EmptyNewId_ThrowsException()
    {
        // Arrange: empty document (exception raised before any document access)
        var doc = new SpdxDocument
        {
            Packages = [],
            Files = [],
            Snippets = [],
            Relationships = [],
            Describes = []
        };

        // Act / Assert: empty new ID is invalid
        Assert.Throws<CommandUsageException>(() => RenameId.Rename(doc, "SPDXRef-Old", ""));
    }

    /// <summary>
    ///     Test that Rename throws a usage exception when the new ID is SPDXRef-DOCUMENT
    /// </summary>
    [Fact]
    public void RenameId_Rename_NewIdIsDocument_ThrowsException()
    {
        // Arrange: empty document (exception raised before any document access)
        var doc = new SpdxDocument
        {
            Packages = [],
            Files = [],
            Snippets = [],
            Relationships = [],
            Describes = []
        };

        // Act / Assert: SPDXRef-DOCUMENT is a reserved ID and cannot be used as a target
        Assert.Throws<CommandUsageException>(() => RenameId.Rename(doc, "SPDXRef-Old", "SPDXRef-DOCUMENT"));
    }

    /// <summary>
    ///     Test that Rename throws a command-error exception when the new ID is already in use
    /// </summary>
    [Fact]
    public void RenameId_Rename_NewIdAlreadyInUse_ThrowsException()
    {
        // Arrange: document with two packages
        var doc = new SpdxDocument
        {
            Packages =
            [
                new SpdxPackage { Id = "SPDXRef-Package-1" },
                new SpdxPackage { Id = "SPDXRef-Package-2" }
            ],
            Files = [],
            Snippets = [],
            Relationships = [],
            Describes = []
        };

        // Act / Assert: renaming to an ID already used by another element is an error
        Assert.Throws<CommandErrorException>(() => RenameId.Rename(doc, "SPDXRef-Package-1", "SPDXRef-Package-2"));
    }

    /// <summary>
    ///     Test that Rename performs no operation when the old and new IDs are the same
    /// </summary>
    [Fact]
    public void RenameId_Rename_SameId_NoOp()
    {
        // Arrange: document with a single package
        var doc = new SpdxDocument
        {
            Packages = [new SpdxPackage { Id = "SPDXRef-Package-1" }],
            Files = [],
            Snippets = [],
            Relationships = [],
            Describes = []
        };

        // Act: rename to the same ID is a documented no-op
        RenameId.Rename(doc, "SPDXRef-Package-1", "SPDXRef-Package-1");

        // Assert: document is unchanged
        Assert.Equal("SPDXRef-Package-1", doc.Packages[0].Id);
    }

    /// <summary>
    ///     Test that rename-id command can be invoked from a YAML workflow step
    /// </summary>
    [Fact]
    public void RenameId_Run_WorkflowInvocation_RenamesId()
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

        const string workflowContents =
            """
            steps:
            - command: rename-id
              inputs:
                spdx: rename-workflow-test.spdx.json
                old: SPDXRef-Package-1
                new: SPDXRef-Package-2
            """;

        try
        {
            // Arrange: Write the SPDX and workflow files
            File.WriteAllText("rename-workflow-test.spdx.json", spdxContents);
            File.WriteAllText("rename-workflow-test.yaml", workflowContents);

            // Act: Run the workflow
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "rename-workflow-test.yaml");

            // Assert: Verify the workflow succeeded
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("rename-workflow-test.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("rename-workflow-test.spdx.json"));

            // Assert: Verify the SPDX ID was updated
            Assert.Equal("SPDXRef-Package-2", doc.Packages[0].Id);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.Equal("SPDXRef-Package-2", doc.Describes[0]);
        }
        finally
        {
            File.Delete("rename-workflow-test.spdx.json");
            File.Delete("rename-workflow-test.yaml");
        }
    }

    /// <summary>
    ///     Test that renaming a file ID also updates snippet range pointer references
    ///     (which the serializer derives from SnippetFromFile)
    /// </summary>
    [Fact]
    public void RenameId_Rename_SnippetPointerReferences_UpdatesReferences()
    {
        const string spdxContents =
            """
            {
              "files": [
                {
                  "SPDXID": "SPDXRef-File-1",
                  "fileName": "./test-file.txt",
                  "licenseConcluded": "MIT",
                  "copyrightText": "NOASSERTION"
                }
              ],
              "snippets": [
                {
                  "SPDXID": "SPDXRef-Snippet-1",
                  "snippetFromFile": "SPDXRef-File-1",
                  "ranges": [
                    {
                      "startPointer": { "offset": 10, "reference": "SPDXRef-File-1" },
                      "endPointer": { "offset": 20, "reference": "SPDXRef-File-1" }
                    }
                  ],
                  "licenseConcluded": "MIT",
                  "copyrightText": "NOASSERTION"
                }
              ],
              "packages": [],
              "relationships": [],
              "spdxVersion": "SPDX-2.2",
              "dataLicense": "CC0-1.0",
              "SPDXID": "SPDXRef-DOCUMENT",
              "name": "Test Document",
              "documentNamespace": "https://sbom.spdx.org",
              "creationInfo": {
                "created": "2021-10-01T00:00:00Z",
                "creators": [ "Person: Malcolm Nixon" ]
              },
              "documentDescribes": []
            }
            """;

        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("rename-pointer-test.spdx.json", spdxContents);

            // Act: Run the tool to rename the file ID
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "rename-id",
                "rename-pointer-test.spdx.json",
                "SPDXRef-File-1",
                "SPDXRef-File-2");

            // Assert: Verify the rename succeeded
            Assert.Equal(0, exitCode);

            // Read the serialized JSON to verify pointer reference fields are updated
            Assert.True(File.Exists("rename-pointer-test.spdx.json"));
            var json = File.ReadAllText("rename-pointer-test.spdx.json");

            // Assert: Verify the pointer reference values are updated in the JSON output
            Assert.Contains("\"reference\": \"SPDXRef-File-2\"", json);
            Assert.DoesNotContain("\"reference\": \"SPDXRef-File-1\"", json);

            // Also verify the SnippetFromFile model field is updated
            var doc = Spdx2JsonDeserializer.Deserialize(json);
            Assert.Equal("SPDXRef-File-2", doc.Snippets[0].SnippetFromFile);
        }
        finally
        {
            File.Delete("rename-pointer-test.spdx.json");
        }
    }
}
