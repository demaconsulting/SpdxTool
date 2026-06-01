### ValidateGetVersion

#### Purpose

ValidateGetVersion exercises the get-version command end-to-end within the Self-Test subsystem. It
verifies that a package version can be retrieved from an SPDX document by package ID and captured
into a workflow variable that is subsequently printed to the log output.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the get-version self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: A TestResult entry named SpdxTool_GetVersion has been appended to results; a
  pass or fail message has been written to the Context.

**DoValidate**: performs the actual get-version validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the log contains the expected version string.
- *Preconditions*: A writable working directory is available. Callers must execute serially because
  Validate.RunSpdxTool temporarily mutates the process-wide current working directory.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if Directory.CreateDirectory
  never succeeded, the delete is skipped rather than raising a secondary exception.

Creates a validate.tmp directory, writes an SPDX JSON document containing two packages where
SPDXRef-Package-2 has version "2.0.0", and writes a workflow YAML that executes get-version to
retrieve the version of SPDXRef-Package-2 into the version variable and then prints it using the
print command. Calls Validate.RunSpdxTool with --silent, --log, and run-workflow arguments.
Reads the log file and verifies it contains the text "Found version 2.0.0".

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the log file
is absent after a successful tool exit, guarding against output.log not being written. Returns false
if the log file does not contain the expected "Found version 2.0.0" text. The finally block guards the
Directory.Delete call with a Directory.Exists check to prevent a secondary DirectoryNotFoundException
masking the original exception when Directory.CreateDirectory fails (e.g., because validate.tmp
already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the get-version command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
