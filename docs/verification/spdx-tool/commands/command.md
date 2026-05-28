### Command

#### Verification Approach

`Command` is verified with direct unit tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/CommandTests.cs`. The tests cover the abstract
base class contract, variable token expansion behavior, missing variable handling, and nested
variable expansion.

#### Test Environment

The tests exercise the Command base class in isolation using the standard xUnit v3 environment.
No external service or file system access is required.

#### Acceptance Criteria

Verification is acceptable when variable expansion correctly substitutes defined tokens,
throws for undefined tokens, and handles nested expansion in a single pass.

#### Test Scenarios

**AbstractContract**: the base class provides the execution interface consumed by all command
implementations. This scenario is tested by `Command_Expand_NoVariables_ReturnsOriginal`.

**VariableExpansion**: variable tokens in step inputs are expanded to their values at
execution time. This scenario is tested by `Command_Expand_BasicVariable_ReturnsExpanded`
and `Command_GetMapString_WithVariableExpansion_ReturnsExpanded`.

**MissingVariableRejection**: undefined variable tokens cause expansion to throw an
`InvalidOperationException` rather than silently substituting an empty string. This scenario is
tested by `Command_Expand_MissingVariable_ThrowsInvalidOperationException`.

**NestedVariableExpansion**: tokens whose values themselves contain token references are
fully expanded in a single pass. This scenario is tested by
`Command_Expand_NestedVariable_ReturnsFullyExpanded`.
