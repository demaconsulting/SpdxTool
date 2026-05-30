### ValidateHash

#### Verification Approach

`ValidateHash` is verified by `test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateHashTests.cs`,
which runs the step end-to-end through hash generation and verification.

#### Test Environment

The test uses temporary local files in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after generating and
then verifying the expected SHA-256 hash.

#### Test Scenarios

**EndToEndHashGenerationAndVerification**: the self-test step proves that `hash` can generate and
verify file hashes during validation. This scenario is tested by `ValidateHash_Run_ValidHashWorkflow_Passes`.

**ExceptionPropagation**: when an I/O error prevents `DoValidate` from running, the exception
propagates uncaught from `Run()` and no `TestResult` is recorded. This scenario is tested by
`ValidateHash_Run_IoError_PropagatesException`.
