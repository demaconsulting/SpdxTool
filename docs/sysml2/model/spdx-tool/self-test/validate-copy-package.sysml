package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateCopyPackage {
        doc /* Self-test step exercising the copy-package command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateCopyPackage.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateCopyPackageTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-copy-package.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-copy-package.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-copy-package.yaml");
        }
    }
}
