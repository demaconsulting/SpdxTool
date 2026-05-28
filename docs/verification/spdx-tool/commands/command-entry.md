### CommandEntry

#### Verification Approach

`CommandEntry` is verified indirectly through registry dispatch tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/`. Every test that successfully dispatches a
known command exercises the CommandEntry record. Unknown-command rejection is directly tested
in the program integration tests.

#### Test Environment

Tests run in the standard xUnit v3 environment. No external service or file system access is
required.

#### Acceptance Criteria

Verification is acceptable when known commands are dispatched through their CommandEntry
records and when unrecognized command names are rejected.

#### Test Scenarios

**RecordAssociation**: the CommandEntry record correctly associates a command name, usage
metadata, and Command instance as an immutable unit. This scenario is exercised by any test
that dispatches a registered command (e.g., `Command_Expand_NoVariables_ReturnsOriginal`).

**RegistryDispatch**: CommandEntry is used by CommandsRegistry to route command-line
invocations to the correct Command implementation. This scenario is tested by
`Commands_Dispatch_UnknownCommand_ReportsError`.
