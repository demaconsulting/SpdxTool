package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateDiagram {
        doc /* Self-test step exercising the diagram command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateDiagram.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateDiagramTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-diagram.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-diagram.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-diagram.yaml");
        }
    }
}
