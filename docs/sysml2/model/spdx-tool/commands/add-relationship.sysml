package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def AddRelationship {
        doc /* Command that adds relationships between SPDX elements in a document. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/AddRelationship.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/AddRelationshipTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/add-relationship.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/add-relationship.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/add-relationship.yaml");
        }
    }
}
