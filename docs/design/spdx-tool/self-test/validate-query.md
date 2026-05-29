### ValidateQuery

#### Purpose

ValidateQuery exercises the query command end-to-end within the Self-Test subsystem. It verifies that
an external program can be queried and a version string extracted from its output using a regular
expression pattern, with the result captured into a workflow variable.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the query self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: None.
- *Post-conditions*: A TestResult entry named SpdxTool_Query has been appended to results; a pass or
  fail message has been written to the Context.

**DoValidate**: performs the actual query validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the log matches the version pattern.
- *Preconditions*: The dotnet executable must be available on the system PATH.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if Directory.CreateDirectory
  never succeeded, the delete is skipped rather than raising a secondary exception.

Creates a validate.tmp directory and writes a workflow YAML that executes query against dotnet
--version, extracts the version using a regex pattern into the version variable, and prints it using
the print command. Calls Validate.RunSpdxTool with --silent, --log, and run-workflow arguments. Reads
the log file and verifies it matches the VersionRegex pattern (a dotted decimal version prefixed by
"Dotnet version ").

**VersionRegex**: source-generated regular expression used to validate the query output.

- *Parameters*: None.
- *Returns*: `Regex` — a compiled regular expression matching "Dotnet version N.N.N".
- *Preconditions*: None.
- *Post-conditions*: None.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the log file
content does not match the VersionRegex pattern. This step requires dotnet to be on the PATH; if
dotnet is unavailable the RunSpdxTool call will return a non-zero exit code. Any exception thrown by
DoValidate propagates uncaught from Run; no TestResult is recorded for this step if an exception is
thrown — the exception surfaces to the Self-Test orchestrator. The finally block guards the
Directory.Delete call with a Directory.Exists check to prevent a secondary DirectoryNotFoundException
masking the original exception when Directory.CreateDirectory fails (e.g., because validate.tmp
already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the query command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.
- **System.Text.RegularExpressions** — used for the source-generated VersionRegex method.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
