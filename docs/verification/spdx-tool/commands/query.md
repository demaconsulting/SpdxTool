### Query

#### Verification Approach

`Query` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/QueryTests.cs`. The tests cover missing arguments,
invalid regular expressions, invalid programs, direct extraction of values from external process
output, and workflow-based variable storage.

#### Test Environment

The tests run in the standard xUnit v3 environment and require `dotnet` on the system path because the
verified scenarios query external process output. No additional external service is required.

#### Acceptance Criteria

Verification is acceptable when invalid inputs are rejected, direct invocations print the captured
value, and workflow invocations store the captured value for later steps.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when the query pattern or program is omitted.
This scenario is tested by `Query_MissingArguments_ReportsError`.

**PatternMissingValueGroup**: the unit reports an error when the supplied regular expression does not contain a `value` capture group. This
scenario is tested by `Query_PatternMissingValueGroup_ReportsError`.

**InvalidProgram**: the unit reports an error when the external program cannot be started. This
scenario is tested by `Query_InvalidProgram_ReportsError`.

**CommandLineCapture**: the unit extracts a named value from external program output and prints it.
This scenario is tested by `Query_DotNetVersion_OnCommandLine_ReturnsVersion`.

**WorkflowCapture**: the unit stores the captured value for downstream workflow use. This scenario
is tested by `Query_DotNetVersion_InWorkflow_StoresVersion`.

**InvalidRegexPattern**: the unit reports an error when the supplied regular expression is syntactically invalid. This scenario is tested by `Query_InvalidRegexPattern_ReportsError`.

**PatternNotFound**: the unit reports an error when the pattern is not matched in any line of the program output. This scenario is tested by `Query_PatternNotFound_ReportsError`.
