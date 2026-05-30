### ValidateAddRelationship

#### Purpose

ValidateAddRelationship exercises the add-relationship command end-to-end within the Self-Test
subsystem. It verifies that a relationship can be added between two existing SPDX packages via a
workflow file and that the resulting document contains the expected relationship with the correct type,
element IDs, and comment.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the add-relationship self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: None.
- *Post-conditions*: A TestResult entry named SpdxTool_AddRelationship has been appended to results;
  a pass message has been written to the Context's standard output stream, or a fail message to the
  Context's error output stream.

**DoValidate**: performs the actual add-relationship validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the SPDX document matches expectations.
- *Preconditions*: A writable working directory is available.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if Directory.CreateDirectory
  never succeeded, the delete is skipped rather than raising a secondary exception.

Creates a validate.tmp directory, writes an SPDX JSON document containing two packages
(SPDXRef-Package-1 and SPDXRef-Package-2), and writes a workflow YAML that executes add-relationship
to add a CONTAINS relationship from SPDXRef-Package-1 to SPDXRef-Package-2 with a comment. Calls
Validate.RunSpdxTool with --silent and run-workflow arguments, then reads the modified SPDX document
and verifies the content using a positional list pattern match — the relationship order in the
deserialized document is significant. The validate.tmp directory is deleted in a finally block only
if it exists, guarding against a secondary DirectoryNotFoundException when directory creation fails.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the deserialized
SPDX document does not contain exactly one CONTAINS relationship from SPDXRef-Package-1 to
SPDXRef-Package-2 with the expected comment. Any exception thrown by DoValidate propagates uncaught
from Run; no TestResult is recorded for this step if an exception is thrown — the exception surfaces
to the Self-Test orchestrator. The finally block guards the Directory.Delete call with a
Directory.Exists check to prevent a secondary DirectoryNotFoundException masking the original
exception when Directory.CreateDirectory fails (e.g., because validate.tmp already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the add-relationship command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.
- **Spdx2JsonDeserializer** — from DemaConsulting.SpdxModel.IO; deserializes the output SPDX document
  for structural verification.
- **SpdxRelationshipType** — from DemaConsulting.SpdxModel; used in pattern-matching the relationship.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
