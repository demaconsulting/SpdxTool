### ValidateDiagram

#### Verification Approach

`ValidateDiagram` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateDiagramTests.cs`, which runs the step
end-to-end and inspects the generated Mermaid output.

#### Test Environment

The test uses temporary SPDX input and Mermaid output files in the standard xUnit v3 environment. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after generating the
expected diagram output file.

#### Test Scenarios

**EndToEndDiagramGeneration**: the self-test step proves that `diagram` produces Mermaid output
during validation. This scenario is tested by `SpdxTool_Diagram`.

**ExceptionPropagation**: when an I/O error prevents `DoValidate` from running, the exception
propagates uncaught from `Run()` and no `TestResult` is recorded. This scenario is tested by
`ValidateDiagram_Run_IoError_PropagatesException`.
