### ValidateUpdatePackage

#### Verification Approach

`ValidateUpdatePackage` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateUpdatePackageTests.cs`, which runs the step
end-to-end against temporary SPDX content. No mocking is used; all dependencies are exercised
with real implementations because this is an end-to-end integration test of the complete command
execution path.

#### Test Environment

The test uses a temporary working directory with local SPDX and workflow fixtures in the standard
xUnit v3 environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after updating the
expected package metadata in the SPDX document.

#### Test Scenarios

**EndToEndUpdatePackage**: the self-test step proves that `update-package` updates package metadata
during validation. This scenario is tested by `SpdxTool_UpdatePackage`.

**CommandFailure**: when the update-package command exits with a non-zero exit code,
`ValidateUpdatePackage.Run` records `TestOutcome.Failed` rather than `TestOutcome.Passed`. This
scenario is tested by `ValidateUpdatePackage_Run_CommandFailure_RecordsFailedOutcome`.

**ExceptionPropagation**: when an I/O error prevents DoValidate from running, the exception
propagates uncaught from Run() and no TestResult is recorded. This scenario is tested by
`ValidateUpdatePackage_Run_IoError_PropagatesException`.
