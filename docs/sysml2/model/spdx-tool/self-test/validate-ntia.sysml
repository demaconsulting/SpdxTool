package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateNtia {
        doc /* Self-test step exercising NTIA minimum-elements validation. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateNtia.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateNtiaTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-ntia.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-ntia.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-ntia.yaml");
        }
    }
}
