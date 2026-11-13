package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Program {
        doc /* Entry point that parses global flags, builds the Context, and dispatches to a command or the self-test suite. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Program.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/ProgramTests.cs");
            designFiles = ("docs/design/spdx-tool/program.md");
            verificationFiles = ("docs/verification/spdx-tool/program.md");
            requirementFiles = ("docs/reqstream/spdx-tool/program.yaml");
        }
    }
}
