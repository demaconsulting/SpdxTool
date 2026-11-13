package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def CommandErrorException {
        doc /* Exception signaling an unrecoverable runtime error inside a command (reported without usage). */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/CommandErrorException.cs");
            designFiles = ("docs/design/spdx-tool/commands/command-error-exception.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/command-error-exception.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/command-error-exception.yaml");
        }
    }
}
