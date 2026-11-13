package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateUpdatePackage {
        doc /* Self-test step exercising the update-package command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateUpdatePackage.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateUpdatePackageTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-update-package.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-update-package.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-update-package.yaml");
        }
    }
}
