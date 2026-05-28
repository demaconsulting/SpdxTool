### Print

#### Verification Approach

`Print` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/PrintTests.cs`. The tests cover both direct
command-line output and workflow-driven output behavior.

#### Test Environment

N/A - the unit is verified in the standard xUnit v3 environment with no special setup beyond the test
runner because it only writes to the command context output stream.

#### Acceptance Criteria

Verification is acceptable when the unit writes the expected text in both direct and workflow
execution modes.

#### Test Scenarios

**CommandLineOutput**: the unit writes text when invoked directly from the command line. This scenario
is tested by `Print_Run_OnCommandLine_PrintsText`.

**WorkflowOutput**: the unit writes text when invoked from a workflow step. This scenario is tested
by `Print_Run_InWorkflow_PrintsText`.

**MissingTextInput**: the unit reports an error when the text input is absent from the workflow step.
This scenario is tested by `Print_Run_MissingTextInput_ThrowsYamlException`.
