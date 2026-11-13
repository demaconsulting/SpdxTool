package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def RenameId {
        doc /* Command that renames an SPDX element ID throughout an SPDX document. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/RenameId.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/RenameIdTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/rename-id.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/rename-id.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/rename-id.yaml");
        }
    }
}
