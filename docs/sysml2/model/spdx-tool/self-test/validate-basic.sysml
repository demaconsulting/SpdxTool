package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateBasic {
        doc /* Self-test step exercising the validate command on well-formed and malformed SPDX documents. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateBasic.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateBasicTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-basic.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-basic.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-basic.yaml");
        }
    }
}
