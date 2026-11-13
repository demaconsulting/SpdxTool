package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Print {
        doc /* Command that writes lines of text to the console. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/Print.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/PrintTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/print.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/print.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/print.yaml");
        }
    }
}
