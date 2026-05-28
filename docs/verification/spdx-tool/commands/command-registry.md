### CommandsRegistry

#### Verification Approach

`CommandsRegistry` is verified through integration tests that dispatch commands by name in
`test/DemaConsulting.SpdxTool.Tests/`. Known commands are exercised via every direct command
test. Unknown-command rejection is directly tested by the program integration tests.

#### Test Environment

Tests run in the standard xUnit v3 environment. No external service or file system access is
required.

#### Acceptance Criteria

Verification is acceptable when known command names resolve to the correct Command
implementation and when unrecognized command names produce an error report.

#### Test Scenarios

**KnownCommandLookup**: known command names are found in the registry and dispatched to their
Command implementation. This scenario is exercised by any test that invokes a registered
command (e.g., `Command_Expand_NoVariables_ReturnsOriginal`).

**UnknownCommandRejection**: unrecognized command names cause the tool to report an error and
print usage information. This scenario is tested by
`UnknownCommand_UnrecognizedCommand_ReportsError`.
