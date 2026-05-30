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

**VariableExpansion**: the unit expands `${{ variable }}` tokens in text lines before writing them
to output. This scenario is tested by `Print_Run_InWorkflow_PrintsText`, which supplies a variable
reference in the text and asserts the resolved value appears in the output.

**MissingTextInput**: the unit reports an error when the text input is absent from the workflow step.
This scenario is tested by `Print_Run_MissingTextInput_ReportsError`.

**UndefinedVariable**: the unit reports an error when a text line references a variable that is not
present in the workflow variable map. This scenario is tested by
`Print_Run_UndefinedVariable_ReportsError`.

**MalformedExpansion**: the unit reports an error when a text line contains an empty variable name
(`${{  }}`) or an unmatched macro delimiter (`${{ unclosed`). These scenarios are tested by
`Print_Run_EmptyVariableName_ReportsError` and `Print_Run_UnmatchedMacroDelimiter_ReportsError`.
