## Commands

### Verification Approach

The Commands subsystem is verified by focused command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/`. These tests exercise command-line entry points,
workflow execution paths, YAML input handling, and helper behavior such as variable expansion and
command dispatch.

### Test Environment

Tests run under the standard xUnit v3 harness with temporary SPDX JSON and workflow YAML fixtures.
Most scenarios are fully local; URL and NuGet workflow scenarios additionally require HTTP access
or a populated package cache, and query scenarios require the `dotnet` executable to be
available.

### Acceptance Criteria

Verification is acceptable when command registry and dispatch helpers behave deterministically,
supported commands complete with the expected SPDX output or console output, and invalid inputs
report the documented usage or runtime errors.

### Test Scenarios

**RegistryDispatch**: unknown command names are rejected rather than dispatched to an arbitrary
handler. This scenario is tested by `Commands_Dispatch_UnknownCommand_ReportsError`.

**VariableExpansion**: workflow variables expand correctly, including nested substitutions. This
scenario is tested by `Command_Expand_NestedVariable_ReturnsFullyExpanded`.

**YamlInputExtraction**: string values read from workflow mappings preserve variable expansion
semantics. This scenario is tested by `Command_GetMapString_WithVariableExpansion_ReturnsExpanded`.

**WorkflowExecution**: the subsystem executes a valid workflow and dispatches its steps in order.
This scenario is tested by `RunWorkflow_ValidWorkflowFile_ExecutesWorkflow`.

**ValidationCommand**: the subsystem validates a conformant SPDX document without reporting issues.
This scenario is tested by `Validate_ValidSpdxDocument_Succeeds`.
