package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def CommandUsageException {
        doc /* Exception signaling an incorrectly invoked command (reported with usage information). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/CommandUsageException.cs");
            designFiles = ("docs/design/spdx-tool/commands/command-usage-exception.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/command-usage-exception.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/command-usage-exception.yaml");
        }
    }
}
