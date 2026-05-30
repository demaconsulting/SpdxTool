### ValidateCopyPackage

#### Verification Approach

`ValidateCopyPackage` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateCopyPackageTests.cs`, which runs the step
against temporary source and destination SPDX fixtures.

#### Test Environment

The test uses local SPDX files in a temporary working directory under the standard xUnit v3
environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after copying the
expected package content into the destination document.

#### Test Scenarios

**EndToEndCopyPackage**: the self-test step proves that `copy-package` updates the destination SPDX
document during validation. This scenario is tested by `ValidateCopyPackage_Run_ValidPackageWorkflow_Passes`.

**ExceptionPropagation**: when an I/O error prevents `DoValidate` from running, the exception
propagates uncaught from `Run()` and no `TestResult` is recorded. This scenario is tested by
`ValidateCopyPackage_Run_IoError_PropagatesException`.
