package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Diagram {
        doc /* Command that generates a Mermaid entity-relationship diagram from the package relationships in an SPDX document. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/Diagram.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/DiagramTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/diagram.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/diagram.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/diagram.yaml");
        }
    }
}
