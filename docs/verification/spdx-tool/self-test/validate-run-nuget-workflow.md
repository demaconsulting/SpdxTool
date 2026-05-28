### ValidateRunNuGetWorkflow

#### Verification Approach

`ValidateRunNuGetWorkflow` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateRunNuGetWorkflowTests.cs`, which runs the step
end-to-end using a workflow resolved from a NuGet package.

#### Test Environment

The test uses the standard xUnit v3 environment and requires either a populated NuGet cache or network
access so the referenced package workflow can be resolved.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after resolving and
executing the packaged workflow.

#### Test Scenarios

**EndToEndNuGetWorkflow**: the self-test step proves that `run-workflow` can resolve and execute a
NuGet-packaged workflow during validation. This scenario is tested by `SpdxTool_RunNuGetWorkflow`.
