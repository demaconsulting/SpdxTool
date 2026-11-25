### Help

#### Verification Approach

`Help` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/HelpTests.cs`. The tests cover rejection of
command-line invocation, unknown-command handling, missing-input handling, and successful
rendering of command-specific help text from a workflow step.

#### Test Environment

N/A - the unit is verified in the standard xUnit v3 environment with no special setup beyond the test
runner because it only formats console output.

#### Acceptance Criteria

Verification is acceptable when the unit rejects command-line invocation and incomplete requests,
reports unknown commands clearly, and prints extended help for a registered command.

#### Test Scenarios

**CommandLineRejection**: the unit reports a workflow-only usage error when invoked from the
command line. This scenario is tested by `Help_Run_OnCommandLine_ReportsWorkflowOnlyError`.

**UnknownCommand**: the unit reports an error when a workflow step names an unregistered command.
This scenario is tested by `Help_Run_UnknownCommand_ReportsError`.

**YamlInvocation**: the unit displays the extended help of a registered command when invoked from
a YAML workflow step with the about input. This scenario is tested by
`Help_Run_YamlInvocation_DisplaysHelp`.

**MissingAboutInput**: the unit reports an error when a YAML workflow step omits the required
`about` input. This scenario is tested by `Help_Run_YamlInvocation_MissingAbout_ReportsError`.
