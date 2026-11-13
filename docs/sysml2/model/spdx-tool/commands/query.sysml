package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def Query {
        doc /* Command that runs an external program and extracts a value from its output with a regular expression. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/Query.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/QueryTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/query.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/query.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/query.yaml");
        }
    }
}
