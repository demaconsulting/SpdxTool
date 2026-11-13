package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def CopyPackage {
        doc /* Command that copies a package (and optionally its children and files) from one SPDX document to another. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/CopyPackage.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/CopyPackageTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/copy-package.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/copy-package.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/copy-package.yaml");
        }
    }
}
