### ValidateCopyPackage

#### Purpose

ValidateCopyPackage exercises the copy-package command end-to-end within the Self-Test subsystem. It
verifies that a package can be copied from one SPDX document to another via a workflow file and that
the destination document contains the copied package with the expected relationship.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the copy-package self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: None.
- *Post-conditions*: A TestResult entry named SpdxTool_CopyPackage has been appended to results; a
  pass or fail message has been written to the Context.

**DoValidate**: performs the actual copy-package validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the destination SPDX document matches
  expectations.
- *Preconditions*: A writable working directory is available.
- *Post-conditions*: The validate.tmp directory has been deleted regardless of outcome.

Creates a validate.tmp directory and writes two SPDX JSON documents: a destination document
(to.spdx.json) containing SPDXRef-Package-1, and a source document (from.spdx.json) containing
SPDXRef-Package-2. Writes a workflow YAML that executes copy-package to copy SPDXRef-Package-2 from
the source into the destination with a CONTAINED_BY relationship to SPDXRef-Package-1. Calls
Validate.RunSpdxTool with --silent and run-workflow arguments, then reads the destination document
and verifies that both packages exist and the CONTAINED_BY relationship is present.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the deserialized
destination SPDX document does not contain both packages or does not contain the expected CONTAINED_BY
relationship. The temporary directory is always deleted in a finally block.

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the copy-package command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.
- **Spdx2JsonDeserializer** — from DemaConsulting.SpdxModel.IO; deserializes the destination SPDX
  document for structural verification.
- **SpdxRelationshipType** — from DemaConsulting.SpdxModel; used in pattern-matching the relationship.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
