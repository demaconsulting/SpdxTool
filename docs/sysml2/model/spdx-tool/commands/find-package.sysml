package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def FindPackage {
        doc /* Command that finds the ID of a package matching wildcard search criteria. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/FindPackage.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/FindPackageTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/find-package.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/find-package.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/find-package.yaml");
        }
    }
}
