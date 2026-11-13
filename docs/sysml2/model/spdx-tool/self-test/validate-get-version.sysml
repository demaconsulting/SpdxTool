package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateGetVersion {
        doc /* Self-test step exercising the get-version command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateGetVersion.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateGetVersionTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-get-version.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-get-version.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-get-version.yaml");
        }
    }
}
