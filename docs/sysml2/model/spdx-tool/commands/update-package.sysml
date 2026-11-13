package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def UpdatePackage {
        doc /* Workflow-only command that updates metadata fields of an existing package in an SPDX document. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/UpdatePackage.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/UpdatePackageTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/update-package.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/update-package.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/update-package.yaml");
        }
    }
}
