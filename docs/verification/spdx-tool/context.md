## Context

### Verification Approach

`Context` is verified indirectly through program integration tests in
`test/DemaConsulting.SpdxTool.Tests/ProgramTests.cs`. The tests exercise Context creation,
flag parsing, output writing, and exit code calculation through the Program.Run entry point.

### Test Environment

Tests run in the standard xUnit v3 environment. No external service or file system access is
required for the core Context tests.

### Acceptance Criteria

Verification is acceptable when execution state is correctly maintained across a program
invocation, error counts are reflected in the exit code, and output is written to the
appropriate destinations.

### Test Scenarios

**ExecutionState**: Context correctly tracks version, help, silent, validate, and other flag
state after parsing command-line arguments. This scenario is exercised by
`SpdxTool_Program_Run_VersionContext_WritesVersion` and
`SpdxTool_Program_Run_HelpContext_WritesUsage`.

> **Note**: `SpdxTool_Program_Run_*` tests reside in `ProgramTests.cs` and are covered by
> the `SpdxTool-Architecture` review set.

**ErrorCount**: Context accumulates errors and translates the error count into an exit code
of 1 when errors are present. This scenario is exercised by
`SpdxTool_Program_Run_NoArguments_WritesErrorAndUsage`.
