### CopyPackage

#### Verification Approach

`CopyPackage` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/CopyPackageTests.cs`. The test suite covers command
validation and successful copy operations for a single package, recursive package copies, and copies
that include file entries.

#### Test Environment

The tests use local source and destination SPDX fixtures in the standard xUnit v3 environment. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when missing inputs are rejected and successful executions copy the
expected package graph and file content into the destination SPDX document.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when required copy arguments are missing. This
scenario is tested by `CopyPackage_Run_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when a referenced SPDX file is absent. This scenario
is tested by `CopyPackage_Run_MissingFile_ReportsError`.

**CommandLineCopy**: the unit successfully copies a package between SPDX documents when invoked
from the command line with valid arguments. This scenario is tested by
`CopyPackage_Run_OnCommandLine_CopiesPackage`.

**BasicWorkflowCopy**: the unit copies a package from a source SPDX document to a destination and
adds the requested relationship when invoked from a workflow step. This scenario is tested by
`CopyPackage_Run_InWorkflow_CopiesPackage`.

**RecursiveCopy**: the unit copies a package together with its dependent package graph when
recursive mode is requested. This scenario is tested by
`CopyPackage_Run_InWorkflowRecursive_CopiesPackageRecursively`.

**CopyWithFiles**: the unit copies package-associated file entries into the destination document.
This scenario is tested by `CopyPackage_Run_InWorkflowWithFiles_CopiesPackageAndFiles`.

**EnhanceExistingPackage**: the unit enhances and renames the existing package in the destination
document when the source package has the same identity (name and version) rather than adding a
duplicate. This scenario is tested by `CopyPackage_Run_InWorkflowWithExistingPackage_EnhancesPackage`.

**WorkflowMissingFrom**: the unit reports an error when the `from` input is absent from the workflow
step. This scenario is tested by `CopyPackage_Run_MissingFromInput_ReportsError`.

**WorkflowMissingTo**: the unit reports an error when the `to` input is absent from the workflow
step. This scenario is tested by `CopyPackage_Run_MissingToInput_ReportsError`.

**WorkflowMissingPackage**: the unit reports an error when the `package` input is absent from the
workflow step. This scenario is tested by `CopyPackage_Run_MissingPackageInput_ReportsError`.

**WorkflowInvalidRecursive**: the unit reports an error when the `recursive` input cannot be parsed
as a boolean. This scenario is tested by `CopyPackage_Run_InvalidRecursiveInput_ReportsError`.

**WorkflowInvalidFiles**: the unit reports an error when the `files` input cannot be parsed as a
boolean. This scenario is tested by `CopyPackage_Run_InvalidFilesInput_ReportsError`.

**PackageNotFound**: the unit reports an error when the specified package identifier does not exist
in the source SPDX document. This scenario is tested by `CopyPackage_Run_PackageNotFound_ReportsError`.

**InvalidPackageId**: the unit reports a usage error when the package argument is empty or equals
`SPDXRef-DOCUMENT`. This scenario is tested by `CopyPackage_Run_InvalidPackageId_ReportsError`
and `CopyPackage_Run_EmptyPackageId_ReportsError`.

**EmptyPackageId**: the unit reports a usage error when the package argument is an empty string.
This scenario is tested by `CopyPackage_Run_EmptyPackageId_ReportsError`.
