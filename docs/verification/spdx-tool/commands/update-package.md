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
`UpdatePackage_Run_OnCommandLine_ReportsWorkflowOnlyError`.

**PackageMetadataUpdate**: the unit updates all supported package metadata fields through a workflow
step. This scenario is tested by `UpdatePackage_Run_InWorkflow_UpdatesPackage`.

**MissingSpdxInput**: the unit reports an error when the `spdx` input is missing from the workflow
step. This scenario is tested by `UpdatePackage_Run_MissingSpdxInput_ReportsError`.

**MissingPackageInput**: the unit reports an error when the `package` input is missing from the
workflow step. This scenario is tested by `UpdatePackage_Run_MissingPackageInput_ReportsError`.

**MissingPackageIdInput**: the unit reports an error when the `package.id` sub-key is absent from
the package map. This scenario is tested by `UpdatePackage_Run_MissingPackageIdInput_ReportsError`.

**PackageNotFound**: the unit reports an error when the specified package ID does not exist in the
SPDX document. This scenario is tested by `UpdatePackage_Run_PackageNotFound_ReportsError`.

**PartialUpdate**: the unit updates only the fields present in the workflow step, leaving all other
package fields unchanged. This scenario is tested by
`UpdatePackage_Run_PartialUpdate_PreservesUnspecifiedFields`.

**UnrecognizedField**: the unit reports an error when an unrecognized field name is supplied in the
package map. This scenario is tested by `UpdatePackage_Run_UnrecognizedField_ReportsError`.
