package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateFindPackage {
        doc /* Self-test step exercising the find-package command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateFindPackage.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateFindPackageTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-find-package.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-find-package.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-find-package.yaml");
        }
    }
}
