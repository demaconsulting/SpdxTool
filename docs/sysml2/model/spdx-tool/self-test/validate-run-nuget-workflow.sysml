package SpdxTool {
    private import SoftwareItemMetadata::*;

    part def ValidateRunNuGetWorkflow {
        doc /* Self-test step exercising run-workflow with a workflow resolved from a NuGet package. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.SpdxTool/SelfTest/ValidateRunNuGetWorkflow.cs");
            testFiles = ("test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateRunNuGetWorkflowTests.cs");
            designFiles = ("docs/design/spdx-tool/self-test/validate-run-nuget-workflow.md");
            verificationFiles = ("docs/verification/spdx-tool/self-test/validate-run-nuget-workflow.md");
            requirementFiles = ("docs/reqstream/spdx-tool/self-test/validate-run-nuget-workflow.yaml");
        }
    }
}
