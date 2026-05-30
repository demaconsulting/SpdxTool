### AddPackage

#### Verification Approach

`AddPackage` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/AddPackageTests.cs` using workflow YAML and local SPDX
fixtures. The tests cover workflow-only guarding, package insertion, relationship creation, and
variable-expanded version values.

#### Test Environment

The tests run in the standard xUnit v3 environment with local SPDX JSON and workflow YAML files. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when direct CLI invocation is rejected, valid workflow steps add the
requested package data, and optional relationship or version inputs are persisted in the modified
SPDX document.

#### Test Scenarios

**WorkflowOnlyGuard**: the unit rejects direct command-line invocation and requires workflow
context. This scenario is tested by `AddPackage_OnCommandLine_ReportsWorkflowOnlyError`.

**PackageAndRelationshipAddition**: the unit adds a package and creates the requested relationship
entries in the target document. This scenario is tested by
`AddPackage_InWorkflowWithRelationship_AddsPackageAndRelationship`.

**PackageOnlyAddition**: the unit adds a package even when no relationship block is supplied. This
scenario is tested by `AddPackage_InWorkflowNoRelationship_AddsPackageOnly`.

**QueryExpandedVersion**: the unit accepts workflow-populated version data that was produced by an
earlier query step. This scenario is tested by `AddPackage_InWorkflowWithQueryVersion_AddsPackage`.

**EnhanceExistingPackage**: the unit enhances an existing package rather than duplicating it when a
package with the same identity is already present. This scenario is tested by
`AddPackage_InWorkflowWithExistingPackage_EnhancesPackage`.

**MissingSpdxInput**: the unit reports an error when the `spdx` input is missing from the workflow
step. This scenario is tested by `AddPackage_InWorkflowMissingSpdxInput_ReportsError`.

**MissingPackageInput**: the unit reports an error when the `package` input is missing from the
workflow step. This scenario is tested by `AddPackage_InWorkflowMissingPackageInput_ReportsError`.

**EmptyPackageId**: the unit reports an error when the package `id` input is empty.
This scenario is tested by `AddPackage_InWorkflowWithEmptyPackageId_ReportsError`.

**DocumentPackageId**: the unit reports an error when the package `id` is set to the reserved value
`SPDXRef-DOCUMENT`. This scenario is tested by
`AddPackage_InWorkflowWithDocumentPackageId_ReportsError`.

**MissingPackageName**: the unit reports an error when the package `name` field is absent from the
workflow step package definition. This scenario is tested by
`AddPackage_ParsePackage_MissingPackageName_ReportsError`.

**MissingPackageDownload**: the unit reports an error when the package `download` field is absent
from the workflow step package definition. This scenario is tested by
`AddPackage_ParsePackage_MissingPackageDownload_ReportsError`.
