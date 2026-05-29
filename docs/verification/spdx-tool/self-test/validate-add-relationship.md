### ValidateAddRelationship

#### Verification Approach

`ValidateAddRelationship` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateAddRelationshipTests.cs`, which runs the
self-test step end-to-end against temporary SPDX fixtures.

#### Test Environment

The test uses a temporary working directory with local SPDX content in the standard xUnit v3
environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after confirming that
the expected SPDX relationship entries were created.

#### Test Scenarios

**EndToEndAddRelationship**: the self-test step proves that `add-relationship` creates the requested
relationship entries during validation. This scenario is tested by `SpdxTool_AddRelationship`.

**IoExceptionPropagation**: when the working directory contains `validate.tmp` as a file, `Run`
propagates the `IOException` thrown by `DoValidate` and records no `TestResult`. This scenario is
tested by `ValidateAddRelationship_Run_IoError_PropagatesException`.
