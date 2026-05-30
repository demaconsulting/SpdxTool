## Context

### Verification Approach

`Context` is verified indirectly through program integration tests in
`test/DemaConsulting.SpdxTool.Tests/ProgramTests.cs`. The tests exercise Context creation,
flag parsing, output writing, and exit code calculation through the Program.Run entry point.

### Test Environment

Most Context tests run in the standard xUnit v3 environment without file system access.
The LogTests scenarios (`SpdxTool_Log_*`) require file system write access because they
create temporary log files in the test working directory and verify their content.
ProgramTests-based scenarios do not require file system access.

### Acceptance Criteria

Verification is acceptable when execution state is correctly maintained across a program
invocation, error counts are reflected in the exit code, and output is written to the
appropriate destinations.

### Test Scenarios

**ExecutionState**: Context correctly tracks version, help, silent, validate, and other flag
state after parsing command-line arguments. This scenario is primarily exercised by
`SpdxTool_Silent_ShortFlag_SuppressesOutput`, with supplementary coverage by
`SpdxTool_Program_Run_VersionContext_WritesVersion` and
`SpdxTool_Program_Run_HelpContext_WritesUsage`.

> **Note**: `SpdxTool_Program_Run_*` tests reside in `ProgramTests.cs` and are covered by
> the `SpdxTool-Architecture` review set.

**ErrorCount**: Context accumulates errors and translates the error count into an exit code
of 1 when errors are present. This scenario is exercised by
`SpdxTool_Usage_NoArguments_DisplaysError`.

**LogOutput**: Context writes output to a log file when the `-l`/`--log` flag is supplied,
and continues writing to the log even when `--silent` suppresses console output. This scenario
is exercised by `SpdxTool_Log_ShortFlag_WritesOutputToFile`,
`SpdxTool_Log_LongFlag_WritesOutputToFile`, and
`SpdxTool_Log_SilentFlag_WritesToLogButNotConsole`.

**InvalidDepthRejection**: Context.Create rejects a negative depth value with
InvalidOperationException. This scenario is exercised by
`Context_Create_NegativeDepth_ThrowsInvalidOperationException`.

**InvalidLogFileRejection**: Context.Create rejects an invalid log file path with
InvalidOperationException. This scenario is exercised by
`Context_Create_InvalidLogFilePath_ThrowsInvalidOperationException`.
