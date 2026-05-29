### ValidateUpdatePackage

#### Purpose

ValidateUpdatePackage exercises the update-package command end-to-end within the Self-Test subsystem.
It verifies that all updatable metadata fields of a package in an SPDX document can be modified via
a workflow file and that the resulting document reflects every changed value.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the update-package self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: None.
- *Post-conditions*: A TestResult entry named SpdxTool_UpdatePackage has been appended to results; a
  pass or fail message has been written to the Context.

**DoValidate**: performs the actual update-package validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and every updated field matches the new value.
- *Preconditions*: A writable working directory is available.
- *Post-conditions*: The validate.tmp directory has been deleted regardless of outcome.

Creates a validate.tmp directory, writes an SPDX JSON document containing a single package
(SPDXRef-Package-1) with initial metadata, and writes a workflow YAML that executes update-package
to change the name, download location, version, filename, supplier, originator, homepage, copyright
text, summary, description, and license fields. Calls Validate.RunSpdxTool with --silent and
run-workflow arguments, then reads the modified SPDX document and verifies that all twelve updated
fields match the values specified in the workflow.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the deserialized
SPDX document does not exactly match all twelve updated field values. The temporary directory is
always deleted in a finally block, guarded with a Directory.Exists check to prevent a secondary
DirectoryNotFoundException from masking the original exception.

Any exception thrown by DoValidate (such as IOException or UnauthorizedAccessException) propagates
uncaught from Run; no TestResult is recorded for this step if an exception is thrown — the exception
surfaces to the Self-Test orchestrator.

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the update-package command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.
- **Spdx2JsonDeserializer** — from DemaConsulting.SpdxModel.IO; deserializes the output SPDX document
  for structural verification.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
