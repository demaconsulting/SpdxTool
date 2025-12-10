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

namespace DemaConsulting.SpdxTool.Tests;

/// <summary>
///     Tests for the 'copy-package' command.
/// </summary>
[TestClass]
public class CopyPackageTests
{
    /// <summary>
    ///     Test the 'copy-package' command with missing arguments.
    /// </summary>
    [TestMethod]
    public void CopyPackage_MissingArguments()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "copy-package");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("'copy-package' command missing arguments", output);
    }

    /// <summary>
    ///     Test the 'copy-package' command with missing file.
    /// </summary>
    [TestMethod]
    public void CopyPackage_MissingFile()
    {
        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "copy-package",
            "missing.spdx.json",
            "missing.spdx.json",
            "some-package");

        // Assert: Verify error reported
        Assert.AreEqual(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test the 'copy-package' command from the command-line.
    /// </summary>
    [TestMethod]
    public void CopyPackage_CommandLine()
    {
        const string toSpdxContents =
            """
            {
              "files": [],
              "packages": [
                {
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

        const string fromSpdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package-2",
                  "name": "Another Package",
                  "versionInfo": "1.2.3",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-2",
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
              "documentDescribes": [ "SPDXRef-Package-2" ]
            }
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("to.spdx.json", toSpdxContents);
            File.WriteAllText("from.spdx.json", fromSpdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "copy-package",
                "from.spdx.json",
                "to.spdx.json",
                "SPDXRef-Package-2");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("to.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("to.spdx.json"));

            // Assert: Verify both packages present
            Assert.HasCount(2, doc.Packages);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-2", doc.Packages[1].Id);
        }
        finally
        {
            File.Delete("to.spdx.json");
            File.Delete("from.spdx.json");
        }
    }

    /// <summary>
    ///     Test the 'copy-package' command from a workflow.
    /// </summary>
    [TestMethod]
    public void CopyPackage_Workflow()
    {
        const string toSpdxContents =
            """
            {
              "files": [],
              "packages": [
                {
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

        const string fromSpdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Package-2",
                  "name": "Another Package",
                  "versionInfo": "1.2.3",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Package-2",
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
              "documentDescribes": [ "SPDXRef-Package-2" ]
            }
            """;

        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: copy-package
              inputs:
                from: from.spdx.json
                to: to.spdx.json
                package: SPDXRef-Package-2
                relationships:
                  - type: CONTAINED_BY
                    element: SPDXRef-Package-1
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("to.spdx.json", toSpdxContents);
            File.WriteAllText("from.spdx.json", fromSpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("to.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("to.spdx.json"));

            // Assert: Verify both packages present
            Assert.HasCount(2, doc.Packages);
            Assert.AreEqual("SPDXRef-Package-1", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Package-2", doc.Packages[1].Id);

            // Assert: Verify the relationship
            Assert.HasCount(2, doc.Relationships);
            Assert.AreEqual("SPDXRef-Package-2", doc.Relationships[1].Id);
            Assert.AreEqual(SpdxRelationshipType.ContainedBy, doc.Relationships[1].RelationshipType);
            Assert.AreEqual("SPDXRef-Package-1", doc.Relationships[1].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("to.spdx.json");
            File.Delete("from.spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test the 'copy-package' command with recursive package copying.
    /// </summary>
    [TestMethod]
    public void CopyPackage_Recursive()
    {
        const string toSpdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-MainPackage",
                  "name": "Main Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-MainPackage",
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
              "documentDescribes": [ "SPDXRef-MainPackage" ]
            }
            """;

        const string fromSpdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Application",
                  "name": "Test Application",
                  "versionInfo": "1.2.3",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Library",
                  "name": "Test Library",
                  "versionInfo": "2.3.4",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Compiler",
                  "name": "Compiler",
                  "versionInfo": "3.4.5",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT"
                },
                {
                  "SPDXID": "SPDXRef-Unrelated-Application",
                  "name": "Unrelated Application",
                  "versionInfo": "4.5.6",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "DESCRIBES"
                },
                {
                  "spdxElementId": "SPDXRef-Application",
                  "relatedSpdxElement": "SPDXRef-Library",
                  "relationshipType": "CONTAINS"
                },
                {
                  "spdxElementId": "SPDXRef-Compiler",
                  "relatedSpdxElement": "SPDXRef-Application",
                  "relationshipType": "BUILD_TOOL_OF"
                },
                {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Unrelated-Application",
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
              "documentDescribes": [ "SPDXRef-Application", "SPDXRef-Unrelated-Application" ]
            }
            """;

        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: copy-package
              inputs:
                from: from.spdx.json
                to: to.spdx.json
                package: SPDXRef-Application
                recursive: true
                relationships:
                  - type: CONTAINED_BY
                    element: SPDXRef-MainPackage
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("to.spdx.json", toSpdxContents);
            File.WriteAllText("from.spdx.json", fromSpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("to.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("to.spdx.json"));

            // Assert: Verify expected packages
            Assert.HasCount(4, doc.Packages);
            Assert.AreEqual("SPDXRef-MainPackage", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Application", doc.Packages[1].Id);
            Assert.AreEqual("SPDXRef-Library", doc.Packages[2].Id);
            Assert.AreEqual("SPDXRef-Compiler", doc.Packages[3].Id);

            // Assert: Verify expected relationships
            Assert.HasCount(4, doc.Packages);
            Assert.AreEqual("SPDXRef-DOCUMENT", doc.Relationships[0].Id);
            Assert.AreEqual(SpdxRelationshipType.Describes, doc.Relationships[0].RelationshipType);
            Assert.AreEqual("SPDXRef-MainPackage", doc.Relationships[0].RelatedSpdxElement);
            Assert.AreEqual("SPDXRef-Application", doc.Relationships[1].Id);
            Assert.AreEqual(SpdxRelationshipType.ContainedBy, doc.Relationships[1].RelationshipType);
            Assert.AreEqual("SPDXRef-MainPackage", doc.Relationships[1].RelatedSpdxElement);
            Assert.AreEqual("SPDXRef-Application", doc.Relationships[2].Id);
            Assert.AreEqual(SpdxRelationshipType.Contains, doc.Relationships[2].RelationshipType);
            Assert.AreEqual("SPDXRef-Library", doc.Relationships[2].RelatedSpdxElement);
            Assert.AreEqual("SPDXRef-Compiler", doc.Relationships[3].Id);
            Assert.AreEqual(SpdxRelationshipType.BuildToolOf, doc.Relationships[3].RelationshipType);
            Assert.AreEqual("SPDXRef-Application", doc.Relationships[3].RelatedSpdxElement);
        }
        finally
        {
            File.Delete("to.spdx.json");
            File.Delete("from.spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test the 'copy-package' command with copying of package file information.
    /// </summary>
    [TestMethod]
    public void CopyPackage_Files()
    {
        const string toSpdxContents =
            """
            {
              "files": [],
              "packages": [
                {
                  "SPDXID": "SPDXRef-MainPackage",
                  "name": "Main Package",
                  "versionInfo": "1.0.0",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
                  "licenseConcluded": "MIT"
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-MainPackage",
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
              "documentDescribes": [ "SPDXRef-MainPackage" ]
            }
            """;

        const string fromSpdxContents =
            """
            {
              "files": [
                {
                  "SPDXID" : "SPDXRef-File1",
                  "checksums" : [ {
                    "algorithm" : "SHA1",
                    "checksumValue" : "d6a770ba38583ed4bb4525bd96e50461655d2758"
                  } ],
                  "fileName" : "file1"
                },
                {
                  "SPDXID" : "SPDXRef-File2",
                  "checksums" : [ {
                    "algorithm" : "SHA1",
                    "checksumValue" : "d6a770ba38583ed4bb4525bd96e50461655d2758"
                  } ],
                  "fileName" : "file2"
                }
              ],
              "packages": [
                {
                  "SPDXID": "SPDXRef-Application",
                  "name": "Test Application",
                  "versionInfo": "1.2.3",
                  "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
                  "licenseConcluded": "MIT",
                  "filesAnalyzed" : true,
                  "hasFiles" : [ "SPDXRef-File1", "SPDXRef-File2" ]
                }
              ],
              "relationships": [    {
                  "spdxElementId": "SPDXRef-DOCUMENT",
                  "relatedSpdxElement": "SPDXRef-Application",
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
              "documentDescribes": [ "SPDXRef-Application", "SPDXRef-Unrelated-Application" ]
            }
            """;

        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: copy-package
              inputs:
                from: from.spdx.json
                to: to.spdx.json
                package: SPDXRef-Application
                files: true
                relationships:
                  - type: CONTAINED_BY
                    element: SPDXRef-MainPackage
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("to.spdx.json", toSpdxContents);
            File.WriteAllText("from.spdx.json", fromSpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify success
            Assert.AreEqual(0, exitCode);

            // Read the SPDX document
            Assert.IsTrue(File.Exists("to.spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("to.spdx.json"));

            // Assert: Verify expected packages
            Assert.HasCount(2, doc.Packages);
            Assert.AreEqual("SPDXRef-MainPackage", doc.Packages[0].Id);
            Assert.AreEqual("SPDXRef-Application", doc.Packages[1].Id);

            // Assert: Verify expected files
            Assert.HasCount(2, doc.Files);
            Assert.AreEqual("SPDXRef-File1", doc.Files[0].Id);
            Assert.AreEqual("SPDXRef-File2", doc.Files[1].Id);
        }
        finally
        {
            File.Delete("to.spdx.json");
            File.Delete("from.spdx.json");
            File.Delete("workflow.yaml");
        }
    }
}