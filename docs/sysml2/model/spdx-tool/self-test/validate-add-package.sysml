package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateAddPackage {
        doc /* Self-test step exercising the add-package command end to end. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateAddPackage.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateAddPackageTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-add-package.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-add-package.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-add-package.yaml");
        }
    }
}
