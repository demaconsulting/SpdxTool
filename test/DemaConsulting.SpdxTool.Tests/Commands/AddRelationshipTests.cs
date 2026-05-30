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

namespace DemaConsulting.SpdxTool.Tests.Commands;

/// <summary>
///     Tests for the 'add-relationship' command
/// </summary>
[Collection("CommandSequential")]
public class AddRelationshipTests
{
    /// <summary>
    ///     SPDX test fixture containing two packages and no relationships
    /// </summary>
    private const string SpdxContents =
        """
        {
          "files": [],
          "packages": [    {
              "SPDXID": "SPDXRef-Package-1",
              "name": "Test Package",
              "versionInfo": "1.0.0",
              "packageFileName": "package1.zip",
              "downloadLocation": "https://github.com/demaconsulting/SpdxTool",
              "licenseConcluded": "MIT"
            },
            {
              "SPDXID": "SPDXRef-Package-2",
              "name": "Another Test Package",
              "versionInfo": "2.0.0",
              "packageFileName": "package2.tar",
              "downloadLocation": "https://github.com/demaconsulting/SpdxModel",
              "licenseConcluded": "MIT"
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

    /// <summary>
    ///     Test that add-relationship command with missing arguments reports an error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_MissingArguments_ReportsError()
    {
        // Arrange: No setup required - testing argument validation only

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "add-relationship");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("'add-relationship' command missing arguments", output);
    }

    /// <summary>
    ///     Test that add-relationship command with missing file reports an error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_MissingFile_ReportsError()
    {
        // Arrange: No setup required - testing missing file error with non-existent path

        // Act: Run the command
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            "DemaConsulting.SpdxTool.dll",
            "add-relationship",
            "missing.spdx.json",
            "from-package",
            "CONTAINS",
            "to-package");

        // Assert: Verify error reported
        Assert.Equal(1, exitCode);
        Assert.Contains("File not found: missing.spdx.json", output);
    }

    /// <summary>
    ///     Test that add-relationship command on command line without a comment adds a relationship
    /// </summary>
    [Fact]
    public void AddRelationship_Run_OnCommandLine_WithoutComment_AddsRelationship()
    {
        try
        {
            // Arrange: Write the SPDX file
            File.WriteAllText("spdx.json", SpdxContents);

            // Act: Run the command using the four-argument form (no comment)
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "add-relationship",
                "spdx.json",
                "SPDXRef-Package-1",
                "CONTAINS",
                "SPDXRef-Package-2");

            // Assert: Verify relationship added successfully
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify the relationship was added and the comment is absent
            Assert.Single(doc.Relationships);
            Assert.Equal("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.Equal(SpdxRelationshipType.Contains, doc.Relationships[0].RelationshipType);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.Null(doc.Relationships[0].Comment);
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    /// <summary>
    ///     Test that add-relationship command on command line adds a relationship
    /// </summary>
    [Fact]
    public void AddRelationship_Run_OnCommandLine_AddsRelationship()
    {
        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "add-relationship",
                "spdx.json",
                "SPDXRef-Package-1",
                "CONTAINS",
                "SPDXRef-Package-2",
                "Package 1 contains Package 2");

            // Assert: Verify relationship added successfully
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify the relationship added
            Assert.Single(doc.Relationships);
            Assert.Equal("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.Equal(SpdxRelationshipType.Contains, doc.Relationships[0].RelationshipType);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.Equal("Package 1 contains Package 2", doc.Relationships[0].Comment);
        }
        finally
        {
            File.Delete("spdx.json");
        }
    }

    /// <summary>
    ///     Test that add-relationship command in workflow adds the relationship
    /// </summary>
    [Fact]
    public void AddRelationship_Run_InWorkflow_AddsRelationship()
    {
        // Workflow contents
        const string workflowContents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                relationships:
                - type: CONTAINS
                  element: SPDXRef-Package-2
                  comment: Package 1 contains Package 2
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify relationship added successfully
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify the relationship added
            Assert.Single(doc.Relationships);
            Assert.Equal("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.Equal(SpdxRelationshipType.Contains, doc.Relationships[0].RelationshipType);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.Equal("Package 1 contains Package 2", doc.Relationships[0].Comment);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-relationship command with replace mode replaces existing relationships
    /// </summary>
    [Fact]
    public void AddRelationship_Run_ReplaceMode_ReplacesExistingRelationship()
    {
        // Workflow1 contents
        const string workflow1Contents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                relationships:
                - type: CONTAINS
                  element: SPDXRef-Package-2
                  comment: Package 1 contains Package 2
                - type: DESCRIBES
                  element: SPDXRef-Package-2
                  comment: Package 1 describes Package 2
            """;

        // Workflow2 contents
        const string workflow2Contents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                replace: true
                relationships:
                - type: BUILD_TOOL_OF
                  element: SPDXRef-Package-2
                  comment: Package 1 builds Package 2
            """;

        try
        {
            // Arrange: Write the SPDX files
            File.WriteAllText("spdx.json", SpdxContents);
            File.WriteAllText("workflow1.yaml", workflow1Contents);
            File.WriteAllText("workflow2.yaml", workflow2Contents);

            // Act: Run the first workflow
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow1.yaml");

            // Assert: Verify success
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            var doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify the relationships added
            Assert.Equal(2, doc.Relationships.Count());
            Assert.Equal("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.Equal(SpdxRelationshipType.Contains, doc.Relationships[0].RelationshipType);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.Equal("Package 1 contains Package 2", doc.Relationships[0].Comment);
            Assert.Equal("SPDXRef-Package-1", doc.Relationships[1].Id);
            Assert.Equal(SpdxRelationshipType.Describes, doc.Relationships[1].RelationshipType);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[1].RelatedSpdxElement);
            Assert.Equal("Package 1 describes Package 2", doc.Relationships[1].Comment);

            // Act: Run the second workflow
            exitCode = Runner.Run(
                out _,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow2.yaml");

            // Assert: Verify success
            Assert.Equal(0, exitCode);

            // Read the SPDX document
            Assert.True(File.Exists("spdx.json"));
            doc = Spdx2JsonDeserializer.Deserialize(File.ReadAllText("spdx.json"));

            // Assert: Verify the relationship replaced
            Assert.Single(doc.Relationships);
            Assert.Equal("SPDXRef-Package-1", doc.Relationships[0].Id);
            Assert.Equal(SpdxRelationshipType.BuildToolOf, doc.Relationships[0].RelationshipType);
            Assert.Equal("SPDXRef-Package-2", doc.Relationships[0].RelatedSpdxElement);
            Assert.Equal("Package 1 builds Package 2", doc.Relationships[0].Comment);
        }
        finally
        {
            File.Delete("spdx.json");
            File.Delete("workflow1.yaml");
            File.Delete("workflow2.yaml");
        }
    }

    /// <summary>
    ///     Test that add-relationship command in workflow with missing spdx input reports error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_InWorkflowMissingSpdxInput_ReportsError()
    {
        // Workflow contents - missing 'spdx' input
        const string workflowContents =
            """
            steps:
            - command: add-relationship
              inputs:
                id: SPDXRef-Package-1
                relationships:
                - type: CONTAINS
                  element: SPDXRef-Package-2
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-relationship' command missing 'spdx' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-relationship command in workflow with missing id input reports error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_InWorkflowMissingIdInput_ReportsError()
    {
        // Workflow contents - missing 'id' input
        const string workflowContents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                relationships:
                - type: CONTAINS
                  element: SPDXRef-Package-2
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-relationship' command missing 'id' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-relationship command in workflow with missing relationships input reports error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_InWorkflowMissingRelationshipsInput_ReportsError()
    {
        // Workflow contents - missing 'relationships' input
        const string workflowContents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-relationship' missing 'relationships' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-relationship command in workflow with invalid replace value reports error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_InWorkflowInvalidReplaceValue_ReportsError()
    {
        // Workflow contents - invalid 'replace' value
        const string workflowContents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                replace: not-a-bool
                relationships:
                - type: CONTAINS
                  element: SPDXRef-Package-2
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-relationship' invalid 'replace' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-relationship command in workflow with non-mapping relationship node reports error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_InWorkflowNonMappingRelationshipNode_ReportsError()
    {
        // Workflow contents - relationship is a scalar, not a mapping
        const string workflowContents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                relationships:
                - not-a-mapping
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-relationship' relationship must be a mapping", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-relationship command in workflow with missing relationship type reports error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_InWorkflowMissingRelationshipType_ReportsError()
    {
        // Workflow contents - relationship missing 'type'
        const string workflowContents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                relationships:
                - element: SPDXRef-Package-2
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-relationship' missing relationship 'type' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }

    /// <summary>
    ///     Test that add-relationship command in workflow with missing relationship element reports error
    /// </summary>
    [Fact]
    public void AddRelationship_Run_InWorkflowMissingRelationshipElement_ReportsError()
    {
        // Workflow contents - relationship missing 'element'
        const string workflowContents =
            """
            steps:
            - command: add-relationship
              inputs:
                spdx: spdx.json
                id: SPDXRef-Package-1
                relationships:
                - type: CONTAINS
            """;

        try
        {
            // Arrange: Write the workflow file
            File.WriteAllText("workflow.yaml", workflowContents);

            // Act: Run the command
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                "DemaConsulting.SpdxTool.dll",
                "run-workflow",
                "workflow.yaml");

            // Assert: Verify error reported
            Assert.Equal(1, exitCode);
            Assert.Contains("'add-relationship' missing relationship 'element' input", output);
        }
        finally
        {
            File.Delete("workflow.yaml");
        }
    }
}
