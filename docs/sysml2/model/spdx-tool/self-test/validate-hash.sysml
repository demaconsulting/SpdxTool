package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateHash {
        doc /* Self-test step exercising the hash command's generate and verify modes. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateHash.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateHashTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-hash.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-hash.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-hash.yaml");
        }
    }
}
