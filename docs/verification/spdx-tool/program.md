## Program

### Verification Approach

`Program` is verified with integration tests in
`test/DemaConsulting.SpdxTool.Tests/ProgramTests.cs`. The tests exercise the program entry
point for version reporting, help display, and error handling when no command is supplied.

### Test Environment

ProgramTests.cs exercises the in-process `Program.Run()` and `Program.Main()` entry points
directly in the standard xUnit v3 environment. Other test files (VersionTests.cs, UsageTests.cs,
SilentTests.cs, LogTests.cs, IntegrationTests.cs) launch the tool as a subprocess via Runner.cs.
No external service or file system access is required for the core program tests.

### Acceptance Criteria

Verification is acceptable when version information is reported correctly, help text is
printed for help requests, and a usage error is reported when no command is supplied. `Main`
answers the `help` command before the options are parsed, and the usage information ends with
the pointer to the help command.

### Test Scenarios

**VersionDisplay**: the program entry point reports the build version when invoked with the
version flag. This scenario is tested by
`SpdxTool_Program_Run_VersionContext_WritesVersion`.

**HelpDisplay**: the program entry point prints usage information, including the
`spdx-tool help [topic]` usage line, when invoked with the help flag, and the usage information
ends with the pointer to the help command. This scenario is tested by
`SpdxTool_Program_Run_HelpContext_WritesUsage`.

**HelpCommand**: `Main` invoked with `help`, which is not an option, returns exit code 0 and
prints the help topic index without the banner, confirming the help command is answered before
the options are parsed. This scenario is tested by `Program_Main_HelpVerb_PrintsHelpIndex`.

**MissingCommandError**: the program entry point reports a usage error and exits with code 1
when invoked without a command or recognized option. This scenario is tested by
`SpdxTool_Program_Run_NoArguments_WritesErrorAndUsage`.
