package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def CommandsRegistry {
        doc /* Central static registry mapping command names to their CommandEntry instances for dispatch and help. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/CommandsRegistry.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/CommandTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/commands-registry.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/commands-registry.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/commands-registry.yaml");
        }
    }
}
