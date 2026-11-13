package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def RunWorkflow {
        doc /* Command that executes a parameterized multi-step workflow from a YAML file, URL, or NuGet package. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/Commands/RunWorkflow.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/Commands/RunWorkflowTests.cs");
            designFiles = ("docs/design/spdx-tool/commands/run-workflow.md");
            verificationFiles = ("docs/verification/spdx-tool/commands/run-workflow.md");
            requirementFiles = ("docs/reqstream/spdx-tool/commands/run-workflow.yaml");
        }
    }
}
