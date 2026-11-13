package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def SetVariable {
        doc /* Workflow-only command that assigns a value to a workflow variable. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/SetVariable.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/SetVariableTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/set-variable.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/set-variable.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/set-variable.yaml");
        }
    }
}
