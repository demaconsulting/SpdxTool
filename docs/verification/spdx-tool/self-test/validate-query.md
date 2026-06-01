### ValidateQuery

#### Verification Approach

`ValidateQuery` is verified by `test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateQueryTests.cs`,
which runs the step end-to-end against an external process query scenario.

The defensive guard in `DoValidate` that returns `false` when the log file is absent after a
successful tool exit is intentionally untested. This path cannot be triggered without mock
infrastructure to intercept the file system between a zero exit code and the subsequent log file
check, and that level of mocking lies outside the integration-test-only test strategy applied to
all self-test units.

#### Test Environment

The test uses the standard xUnit v3 environment and requires `dotnet` on the system path because the
step queries external process output.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after extracting the
expected value from external process output.

#### Test Scenarios

**EndToEndQuery**: the self-test step proves that `query` can capture a named value from external
process output during validation. This scenario is tested by `SpdxTool_Query`.

**CommandFailure**: when the query command exits with a non-zero exit code (triggered via the
`PreRunSpdxToolHookForTest` hook corrupting `validate.tmp/workflow.yaml`), `Run` records
`TestOutcome.Failed` and no exception propagates. This scenario is tested by
`ValidateQuery_Run_CommandFailure_RecordsFailedOutcome`.

**IoError**: when `validate.tmp` cannot be created as a directory (e.g., it pre-exists as a file),
`Run` propagates the `IOException` uncaught and records no `TestResult`. This scenario is tested by
`ValidateQuery_Run_IoError_PropagatesException`.
