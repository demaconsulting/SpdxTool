package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def GetVersion {
        doc /* Command that retrieves the version of a package matching search criteria. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/GetVersion.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/GetVersionTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/get-version.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/get-version.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/get-version.yaml");
        }
    }
}
