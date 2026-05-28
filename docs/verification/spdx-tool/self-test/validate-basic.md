### ValidateBasic

#### Verification Approach

`ValidateBasic` is verified by `test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateBasicTests.cs`,
which runs the validation step against both valid and invalid SPDX fixtures.

#### Test Environment

The test uses temporary local SPDX files in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when the self-test step passes only if valid SPDX content succeeds and
invalid SPDX content fails as expected.

#### Test Scenarios

**ValidAndInvalidDocuments**: the self-test step distinguishes valid SPDX input from malformed SPDX
input using the `validate` command. This scenario is tested by `SpdxTool_Basic`.
