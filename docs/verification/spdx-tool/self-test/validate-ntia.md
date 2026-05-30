### ValidateNtia

#### Verification Approach

`ValidateNtia` is verified by `test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateNtiaTests.cs`,
which runs the NTIA-focused validation step against representative SPDX content.

#### Test Environment

The test uses temporary local SPDX files in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result only when NTIA minimum
elements are enforced correctly.

#### Test Scenarios

**EndToEndNtiaValidation**: the self-test step proves that `validate ntia` distinguishes compliant
and non-compliant SPDX content during validation. This scenario is tested by
`ValidateNtia_Run_ValidNtiaWorkflow_Passes`.

**IoError**: when `validate.tmp` cannot be created as a directory (e.g., it pre-exists as a file),
`Run` propagates the `IOException` uncaught and records no `TestResult`. This scenario is tested by
`ValidateNtia_Run_IoError_PropagatesException`.
