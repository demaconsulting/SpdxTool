package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def AddPackage {
        doc /* Workflow-only command that adds or enhances a package in an SPDX document and optionally relates it to other elements. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/AddPackage.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/AddPackageTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/add-package.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/add-package.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/add-package.yaml");
        }
    }
}
