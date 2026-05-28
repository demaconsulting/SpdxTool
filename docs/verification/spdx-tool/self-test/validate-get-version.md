### ValidateGetVersion

#### Verification Approach

`ValidateGetVersion` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateGetVersionTests.cs`, which runs the step
end-to-end against temporary SPDX content.

#### Test Environment

The test uses a temporary working directory with local SPDX fixtures in the standard xUnit v3
environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after retrieving the
expected package version.

#### Test Scenarios

**EndToEndGetVersion**: the self-test step proves that `get-version` retrieves the expected version
during validation. This scenario is tested by `SpdxTool_GetVersion`.

**ExceptionPropagation**: when an I/O error prevents `DoValidate` from running, the exception
propagates uncaught from `Run()` and no `TestResult` is recorded. This scenario is tested by
`ValidateGetVersion_Run_IoError_PropagatesException`.
