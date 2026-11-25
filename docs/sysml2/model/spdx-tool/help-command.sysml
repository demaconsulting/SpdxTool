package SpdxTool_HelpCommand {
    private import SoftwareItemMetadata::*;
    private import OtsDependencies::*;

    part def HelpCommand {
        doc /* Built-in help command: answers the help verb from embedded help cards before option parsing. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/HelpCommand.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/HelpCommandTests.cs");
            designFiles = ("docs/design/spdx-tool/help-command.md");
            verificationFiles = ("docs/verification/spdx-tool/help-command.md");
            requirementFiles = ("docs/reqstream/spdx-tool/help-command.yaml");
        }
    }

    // Serves the help command from the cards embedded in the assembly
    dependency HelpCommand to HelpCards;
}
