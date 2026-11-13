package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateRenameId {
        doc /* Self-test step exercising the rename-id command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateRenameId.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateRenameIdTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-rename-id.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-rename-id.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-rename-id.yaml");
        }
    }
}
