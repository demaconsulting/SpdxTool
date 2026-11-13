package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Help {
        doc /* Command that displays the extended usage details for a named command. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/Help.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/HelpTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/help.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/help.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/help.yaml");
        }
    }
}
