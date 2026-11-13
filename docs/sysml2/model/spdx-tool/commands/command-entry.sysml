package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def CommandEntry {
        doc /* Immutable record associating a command name and usage metadata with its Command singleton. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/CommandEntry.cs");
            designFiles = ("docs/design/spdx-tool/commands/command-entry.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/command-entry.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/command-entry.yaml");
        }
    }
}
