### ValidateFindPackage

#### Purpose

ValidateFindPackage exercises the find-package command end-to-end within the Self-Test subsystem. It
verifies that a package can be located in an SPDX document by name and that its SPDX ID is captured
into a workflow variable and printed to the log output.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the find-package self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: `StartTime` records the time at which `Run` was entered, captured before
  `DoValidate()` is called. A TestResult entry named SpdxTool_FindPackage has been appended to
  results. On success, `"✓ SpdxTool_FindPackage - Passed"` has been written to
  `context.WriteLine`; on failure, `"✗ SpdxTool_FindPackage - Failed"` has been written to
  `context.WriteError`.

**DoValidate**: performs the actual find-package validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the log contains the expected package ID.
- *Preconditions*: A writable working directory is available. Callers must execute serially because
  Validate.RunSpdxTool mutates the process-wide current working directory.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if Directory.CreateDirectory
  never succeeded, the delete is skipped rather than raising a secondary exception.

Creates a validate.tmp directory, writes an SPDX JSON document containing two packages, and writes
a workflow YAML that executes find-package to locate "Test Package" by name, captures its ID into
the packageId variable, and then prints it using the print command. Calls Validate.RunSpdxTool with
--silent, --log, and run-workflow arguments. Reads the log file and verifies it contains the text
"Found package SPDXRef-Package-1". Returns false if the log file is absent after a successful tool
exit, guarding against output.log not being written.

Note: Validate.RunSpdxTool temporarily mutates the process-wide current working directory; callers
must execute serially to avoid races.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the log file
`output.log` is absent after a successful tool exit, guarding against the tool exiting zero without
writing the log. Returns false if the log file does not contain the expected "Found package
SPDXRef-Package-1" text. The finally block guards the
Directory.Delete call with a Directory.Exists check to prevent a secondary DirectoryNotFoundException
masking the original exception when Directory.CreateDirectory fails (e.g., because validate.tmp
already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the find-package command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
