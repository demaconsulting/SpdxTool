package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Command {
        doc /* Abstract base class defining the CLI and workflow-step execution contract shared by every command. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/Command.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/CommandTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/command.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/command.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/command.yaml");
        }
    }
}
