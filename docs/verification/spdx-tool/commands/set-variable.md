### SetVariable

#### Verification Approach

`SetVariable` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/SetVariableTests.cs`. The tests confirm that the
command is workflow-only and that valid workflow invocations populate the variable map for later
steps.

#### Test Environment

N/A - the unit is verified in the standard xUnit v3 environment with no special setup beyond the test
runner because it only mutates workflow context state.

#### Acceptance Criteria

Verification is acceptable when direct CLI invocation is rejected and workflow invocations store the
requested variable value.

#### Test Scenarios

**WorkflowOnlyGuard**: the unit rejects direct command-line invocation. This scenario is tested by
`SetVariable_Run_OnCommandLine_ReportsWorkflowOnlyError`.

**VariableAssignment**: the unit stores the requested name-value pair in the workflow variables map.
This scenario is tested by `SetVariable_Run_InWorkflow_SetsVariable`.

**MissingValueInput**: the unit throws a YAML exception when the value input is absent from the
workflow step. This scenario is tested by `SetVariable_Run_MissingValue_ThrowsException`.

**MissingOutputInput**: the unit throws a YAML exception when the output input is absent from the
workflow step. This scenario is tested by `SetVariable_Run_MissingOutput_ThrowsException`.

**LiteralOutput**: the unit stores the expanded value under the literal output key string without
applying workflow variable expansion to the output key itself. This scenario is tested by
`SetVariable_Run_OutputWithVariableSyntax_StoredLiterally`.
