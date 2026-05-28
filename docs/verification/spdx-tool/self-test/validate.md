### Validate

#### Verification Approach

`Validate` is verified with direct self-test unit tests in
`test/DemaConsulting.SpdxTool.Tests/SelfTest/SelfTestTests.cs` and with command-line integration
tests in `test/DemaConsulting.SpdxTool.Tests/IntegrationTests.cs`. The evidence covers in-process
orchestration, depth control, and result-file generation.

#### Test Environment

The tests run in the standard xUnit v3 environment with temporary working directories and optional
result files. The CLI wrapper scenarios require `dotnet` on the system path.

#### Acceptance Criteria

Verification is acceptable when `Validate.Run(Context)` completes successfully, honors the requested
output depth, and emits TRX or JUnit XML result files when requested.

#### Test Scenarios

**InProcessOrchestration**: the orchestrator runs the complete self-test suite in-process. This
scenario is tested by `SelfTest_Validate_ValidContext_Succeeds`.

**DepthControl**: the orchestrator honors the requested reporting depth. This scenario is tested by
`SelfTest_Validate_WithDepth_Succeeds`.

**TrxOutput**: the orchestrator writes TRX results when a `.trx` output path is supplied. This
scenario is tested by `SelfTest_ValidateWithTrxResult_GeneratesTrxFile`.

**JUnitOutput**: the orchestrator writes JUnit XML results when an `.xml` output path is supplied.
This scenario is tested by `SelfTest_ValidateWithJUnitResult_GeneratesJUnitFile`.

**CliIntegration**: the installed tool exposes the orchestrator through the `--validate`
command-line flag. This scenario is tested by `SpdxTool_SelfTest_ValidateFlag_Succeeds`.

**UnsupportedResultExtension**: the orchestrator reports an error and produces no file when
`Context.ValidationFile` has an unsupported extension. This scenario is tested by
`SpdxTool_SelfTest_ValidateFlagWithResults_UnsupportedExtension_ReportsError`.
