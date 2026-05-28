### CommandUsageException

#### Verification Approach

`CommandUsageException` is verified indirectly through command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/`. The exception is thrown when a command is
invoked incorrectly (such as a workflow-only command invoked from the CLI) and is caught by
Program, which prints usage information.

#### Test Environment

Tests run in the standard xUnit v3 environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when incorrect usage causes CommandUsageException to be thrown and
results in usage information being printed.

#### Test Scenarios

**IncorrectUsageSignaling**: CommandUsageException is thrown when a workflow-only command is
invoked from the command line, causing the tool to print usage information. This scenario is
tested by `AddPackage_OnCommandLine_ReportsWorkflowOnlyError`.
