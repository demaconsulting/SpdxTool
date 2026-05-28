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
- *Preconditions*: None.
- *Post-conditions*: A TestResult entry named SpdxTool_FindPackage has been appended to results; a
  pass or fail message has been written to the Context.

**DoValidate**: performs the actual find-package validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the log contains the expected package ID.
- *Preconditions*: A writable working directory is available. Callers must execute serially because Validate.RunSpdxTool mutates the process-wide current working directory.
- *Post-conditions*: The validate.tmp directory has been deleted regardless of outcome.

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
does not contain the expected "Found package SPDXRef-Package-1" text. The temporary directory is
always deleted in a finally block.

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the find-package command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
