### ValidateQuery

#### Verification Approach

`ValidateQuery` is verified by `test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateQueryTests.cs`,
which runs the step end-to-end against an external process query scenario.

#### Test Environment

The test uses the standard xUnit v3 environment and requires `dotnet` on the system path because the
step queries external process output.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after extracting the
expected value from external process output.

#### Test Scenarios

**EndToEndQuery**: the self-test step proves that `query` can capture a named value from external
process output during validation. This scenario is tested by `SpdxTool_Query`.
