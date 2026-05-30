### ValidateBasic

#### Verification Approach

`ValidateBasic` is verified by `test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateBasicTests.cs`,
which runs the validation step against both valid and invalid SPDX fixtures, and also verifies that
exceptions thrown by `DoValidate` propagate uncaught from `Run` with no `TestResult` recorded.

#### Test Environment

The test uses temporary local SPDX files in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when the self-test step passes only if valid SPDX content succeeds and
invalid SPDX content fails as expected.

#### Test Scenarios

**ValidAndInvalidDocuments**: the self-test step distinguishes valid SPDX input from malformed SPDX
input using the `validate` command. This scenario is tested by `SpdxTool_Basic`.

**ValidationFails**: when the validate command exits non-zero on the valid-document sub-test, `Run`
records `TestOutcome.Failed`. This scenario is tested by
`ValidateBasic_Run_ValidationFails_RecordsFailedOutcome`.

**IoExceptionPropagation**: when the working directory prevents `validate.tmp` from being created
as a directory, the IOException propagates uncaught from Run and no TestResult is recorded. This
scenario is tested by `ValidateBasic_Run_IoError_PropagatesException`.
