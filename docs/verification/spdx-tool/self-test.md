## SelfTest

### Verification Approach

The SelfTest subsystem is verified with direct unit tests in
`test/DemaConsulting.SpdxTool.Tests/SelfTest/` and with CLI integration tests in
`test/DemaConsulting.SpdxTool.Tests/IntegrationTests.cs`. The direct tests call
`Validate.Run(Context)` and the individual validation step types, while the integration tests
verify the installed command-line behavior of `--validate`.

### Test Environment

The subsystem runs in the standard test harness using temporary working directories populated
with local fixture content. `dotnet` must be available on the path for the query-related
validation path, and the NuGet workflow validation path requires either cached packages or
network access to restore the referenced package.

### Acceptance Criteria

Verification is acceptable when the orchestrator reports success, depth-controlled output is
emitted on demand, and result files are produced in both supported formats while all validation
steps record a passing outcome.

### Test Scenarios

**SubsystemOrchestration**: the self-test orchestrator runs the complete validation suite
in-process. This scenario is tested by `SelfTest_Validate_ValidContext_Succeeds`.

**DepthControlledOutput**: the self-test subsystem emits hierarchical output when a depth is
requested. This scenario is tested by `SelfTest_Validate_WithDepth_Succeeds`.

**TRXReporting**: the subsystem writes TRX output for CI consumers. This scenario is tested by
`SelfTest_ValidateWithTrxResult_GeneratesTrxFile`.

**JUnitReporting**: the subsystem writes JUnit XML output for CI consumers. This scenario is tested
by `SelfTest_ValidateWithJUnitResult_GeneratesJUnitFile`.

**UnsupportedResultExtension**: the subsystem reports an error and exits with a non-zero code when
the result file path uses an extension other than `.trx` or `.xml`. This scenario is tested by
`SpdxTool_SelfTest_ValidateFlagWithResults_UnsupportedExtension_ReportsError`.

**CliValidateFlag**: the installed tool exposes the self-test subsystem through the `--validate`
command-line flag. This scenario is tested by `SpdxTool_SelfTest_ValidateFlag_Succeeds`.
