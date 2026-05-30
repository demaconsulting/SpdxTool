### CommandErrorException

#### Verification Approach

`CommandErrorException` is verified indirectly through command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/`. The exception is thrown when a command
encounters a runtime failure (such as a missing file) and is caught by Program, which reports
the error without printing usage information.

#### Test Environment

Tests run in the standard xUnit v3 environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when runtime failures cause CommandErrorException to be thrown and
result in an error message being reported without usage information.

#### Test Scenarios

**RuntimeFailureSignaling**: CommandErrorException is thrown for runtime failures distinct
from incorrect usage and causes the tool to report the error message without printing full
usage information. This scenario is tested by `GetVersion_Run_MissingFile_ReportsError`.
