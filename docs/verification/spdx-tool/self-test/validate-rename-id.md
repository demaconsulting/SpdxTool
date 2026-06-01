### ValidateRenameId

#### Verification Approach

`ValidateRenameId` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateRenameIdTests.cs`, which runs the step
end-to-end against temporary SPDX content.

#### Test Environment

The test uses a temporary working directory with local SPDX fixtures in the standard xUnit v3
environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after renaming the
target SPDX identifier and its references.

#### Test Scenarios

**EndToEndRenameId**: the self-test step proves that `rename-id` updates the target identifier
consistently during validation. This scenario is tested by `SpdxTool_RenameId`.

**CommandFailure**: when the rename-id command exits with a non-zero exit code (triggered via the
`PreRunSpdxToolHookForTest` hook corrupting `validate.tmp/test.spdx.json`), `Run` records
`TestOutcome.Failed` and no exception propagates. This scenario is tested by
`ValidateRenameId_Run_CommandFailure_RecordsFailedOutcome`.

**IoError**: when `validate.tmp` cannot be created as a directory (e.g., it pre-exists as a file),
`Run` propagates the `IOException` uncaught and records no `TestResult`. This scenario is tested by
`ValidateRenameId_Run_IoError_PropagatesException`.
