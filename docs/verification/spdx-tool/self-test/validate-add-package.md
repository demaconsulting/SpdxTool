### ValidateAddPackage

#### Verification Approach

`ValidateAddPackage` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateAddPackageTests.cs`, which runs the self-test
step end-to-end against a temporary SPDX document and workflow file.

#### Test Environment

The test uses a temporary working directory with local SPDX and workflow content in the standard
xUnit v3 environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after adding the
expected package and relationship to the SPDX document.

#### Test Scenarios

**EndToEndAddPackage**: the self-test step proves that `add-package` can add a package and
relationship in a realistic validation run. This scenario is tested by `SpdxTool_AddPackage`.

**CommandFailure**: when the add-package command exits non-zero, `Run` records
`TestOutcome.Failed`. This scenario is tested by
`ValidateAddPackage_Run_CommandFailure_RecordsFailedOutcome`.

**ExceptionPropagation**: when an I/O error prevents `DoValidate` from running, the exception
propagates uncaught from `Run()` and no `TestResult` is recorded. This scenario is tested by
`ValidateAddPackage_Run_IoError_PropagatesException`.
