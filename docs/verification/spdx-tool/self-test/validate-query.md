### ValidateQuery

#### Verification Approach

`ValidateQuery` is verified by `test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateQueryTests.cs`,
which runs the step end-to-end against an external process query scenario.

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
