### UpdatePackage

#### Verification Approach

`UpdatePackage` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/UpdatePackageTests.cs`. The tests cover workflow-only
guarding and successful updates of package metadata through workflow execution.

#### Test Environment

The tests use local SPDX JSON and workflow YAML fixtures in the standard xUnit v3 environment. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when direct CLI invocation is rejected and workflow execution updates the
expected package metadata fields in the target SPDX document.

#### Test Scenarios

**WorkflowOnlyGuard**: the unit rejects direct command-line invocation. This scenario is tested by
`UpdatePackage_OnCommandLine_ReportsWorkflowOnlyError`.

**PackageMetadataUpdate**: the unit updates package metadata fields through a workflow step. This
scenario is tested by `UpdatePackage_InWorkflow_UpdatesPackage`.
