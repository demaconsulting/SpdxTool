### ValidateFindPackage

#### Verification Approach

`ValidateFindPackage` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateFindPackageTests.cs`, which runs the step
end-to-end with temporary SPDX content.

#### Test Environment

The test uses a temporary working directory with local SPDX fixtures in the standard xUnit v3
environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after locating the
expected package identifier.

#### Test Scenarios

**EndToEndFindPackage**: the self-test step proves that `find-package` returns the expected package
identifier during validation. This scenario is tested by `SpdxTool_FindPackage`.

**ExceptionPropagation**: when an I/O error prevents `DoValidate` from running, the exception
propagates uncaught from `Run()` and no `TestResult` is recorded. This scenario is tested by
`ValidateFindPackage_Run_IoError_PropagatesException`.
