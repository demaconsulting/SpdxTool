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
semantics. This scenario is tested by `Command_GetMapString_WithVariableExpansion_ReturnsExpanded`
and the null-map guard is verified by `Command_GetMapString_NullMap_ReturnsNull`.

**WorkflowExecution**: the subsystem executes a valid workflow and dispatches its steps in order.
This scenario is tested by `RunWorkflow_Run_ValidWorkflowFile_ExecutesWorkflow`.

**ValidationCommand**: the subsystem validates a conformant SPDX document without reporting issues.
This scenario is tested by `Validate_Run_ValidSpdxDocument_Succeeds`.

**VariableExpansionErrorPaths**: variable expansion throws InvalidOperationException for
unmatched `}}`, unmatched `${{`, and empty variable names. These scenarios are tested by
`Command_Expand_UnmatchedClose_ThrowsInvalidOperationException`,
`Command_Expand_UnmatchedOpen_ThrowsInvalidOperationException`, and
`Command_Expand_EmptyVariableName_ThrowsInvalidOperationException`.

**BasicVariableSubstitution**: a single `${{ name }}` token is replaced with its mapped
value. This scenario is tested by `Command_Expand_BasicVariable_ReturnsExpanded`.

**EnvironmentVariableExpansion**: a `${{ environment.NAME }}` token is replaced with the
corresponding process environment variable value. This scenario is tested by
`Command_Expand_EnvironmentVariable_ReturnsEnvironmentValue`.

**RegistryCompleteness**: all sixteen expected command names are present as keys in the
registry dictionary. This scenario is tested by
`CommandsRegistry_Commands_AllExpectedNames_ArePresent`.

**NullInputBehavior**: helper methods `GetMapMap`, `GetMapSequence`, and `GetSequenceString`
return null when their primary input parameter is null. These scenarios are tested by
`Command_GetMapMap_NullMap_ReturnsNull`, `Command_GetMapSequence_NullMap_ReturnsNull`, and
`Command_GetSequenceString_NullSequence_ReturnsNull`.

**VariableExpansionNormalPath**: when no `${{ }}` tokens appear in the input string, the
original string is returned unchanged. This scenario is tested by
`Command_Expand_NoVariables_ReturnsOriginal`.

**VariableExpansionMissingVariable**: when a `${{ name }}` token refers to a variable that is
not present in the variables dictionary, `Command.Expand` throws
`InvalidOperationException`. This scenario is tested by
`Command_Expand_MissingVariable_ThrowsInvalidOperationException`.

**MapStringMissingEntry**: when the requested key is absent from the YAML mapping node,
`Command.GetMapString` returns null. This scenario is tested by
`Command_GetMapString_MissingEntry_ReturnsNull`.

**RegistryDispatchKnownCommand**: a known command name resolves to a non-null registry entry
with a non-null instance. This scenario is tested by
`CommandsRegistry_Commands_KnownCommandName_ResolvesEntry`.

**EnvironmentVariableExpansionUndefined**: when a `${{ environment.NAME }}` token refers to an
environment variable that is not set, `Command.Expand` throws `InvalidOperationException`.
This scenario is tested by
`Command_Expand_UndefinedEnvironmentVariable_ThrowsInvalidOperationException`.
