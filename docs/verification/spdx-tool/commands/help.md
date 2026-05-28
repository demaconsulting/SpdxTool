### Help

#### Verification Approach

`Help` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/HelpTests.cs`. The tests cover missing-argument
handling, too-many-arguments handling, unknown-command handling, and successful rendering of
command-specific help text.

#### Test Environment

N/A - the unit is verified in the standard xUnit v3 environment with no special setup beyond the test
runner because it only formats console output.

#### Acceptance Criteria

Verification is acceptable when the unit rejects incomplete requests, reports unknown commands
clearly, and prints extended help for a registered command.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when no target command is provided. This
scenario is tested by `Help_Run_NoArguments_ReportsError`.

**TooManyArguments**: the unit reports a usage error when more than one argument is provided. This
scenario is tested by `Help_Run_TooManyArguments_ReportsError`.

**UnknownCommand**: the unit reports an error for an unregistered command name. This scenario is
tested by `Help_Run_UnknownCommand_ReportsError`.

**CommandSpecificHelp**: the unit prints extended help for a registered command. This scenario is
tested by `Help_Run_RunWorkflowCommand_DisplaysHelp`.

**YamlInvocation**: the unit displays help when invoked from a YAML workflow step with the about input. This scenario is tested by `Help_Run_YamlInvocation_DisplaysHelp`.
