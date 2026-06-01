### ValidateRunNuGetWorkflow

#### Verification Approach

`ValidateRunNuGetWorkflow` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateRunNuGetWorkflowTests.cs`, which runs the step
end-to-end using a workflow resolved from a NuGet package.

#### Test Environment

The test uses the standard xUnit v3 environment and requires either a populated NuGet cache or network
access so the referenced package workflow can be resolved. No mocking of the NuGet resolution layer is
used; the real `DemaConsulting.SpdxWorkflows` package is resolved from the local cache or configured
feeds because end-to-end NuGet-to-workflow-execution behavior is the subject of the test.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after resolving and
executing the packaged workflow.

#### Test Scenarios

**EndToEndNuGetWorkflow**: the self-test step proves that `run-workflow` can resolve and execute a
NuGet-packaged workflow during validation. This scenario is tested by
`ValidateRunNuGetWorkflow_Run_ValidNuGetWorkflow_Passes`.

**CommandFailure**: when the run-workflow command exits with a non-zero exit code (triggered via the
`PreRunSpdxToolHookForTest` hook corrupting `validate.tmp/workflow.yaml`), `Run` records
`TestOutcome.Failed` and no exception propagates. This scenario is tested by
`ValidateRunNuGetWorkflow_Run_CommandFailure_RecordsFailedOutcome`.

**ExceptionPropagation**: when an I/O error prevents `DoValidate` from running, the exception
propagates uncaught from `Run()` and no `TestResult` is recorded. This scenario is tested by
`ValidateRunNuGetWorkflow_Run_IoError_PropagatesException`.
