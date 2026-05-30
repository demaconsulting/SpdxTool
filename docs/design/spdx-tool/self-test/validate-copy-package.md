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
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: A TestResult entry named SpdxTool_CopyPackage has been appended to results; a
  pass or fail message has been written to the Context.

**DoValidate**: performs the actual copy-package validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the destination SPDX document matches
  expectations.
- *Preconditions*: A writable working directory is available. Callers must execute serially because
  Validate.RunSpdxTool temporarily mutates the process-wide current working directory.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if
  `Directory.CreateDirectory` throws before the directory is created (e.g., because `validate.tmp`
  already exists as a file), `Directory.Exists` returns `false` and the delete is skipped, preventing
  a secondary `DirectoryNotFoundException` from masking the original exception.

Creates a validate.tmp directory and writes two SPDX JSON documents: a destination document
(to.spdx.json) containing SPDXRef-Package-1, and a source document (from.spdx.json) containing
SPDXRef-Package-2. Writes a workflow YAML that executes copy-package to copy SPDXRef-Package-2 from
the source into the destination with a CONTAINED_BY relationship to SPDXRef-Package-1. Calls
Validate.RunSpdxTool with --silent and run-workflow arguments, then reads the destination document
and verifies using order-insensitive LINQ checks that: SPDXRef-Package-1 exists in the packages
collection; SPDXRef-Package-2 exists in the packages collection; and a CONTAINED_BY relationship
from SPDXRef-Package-2 to SPDXRef-Package-1 exists in the relationships collection.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the deserialized
destination SPDX document does not contain both packages or does not contain the expected CONTAINED_BY
relationship. The finally block guards the Directory.Delete call with a Directory.Exists check to
prevent a secondary DirectoryNotFoundException masking the original exception when
Directory.CreateDirectory fails (e.g., because validate.tmp already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the copy-package command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.
- **Spdx2JsonDeserializer** — from DemaConsulting.SpdxModel.IO; deserializes the destination SPDX
  document for structural verification.
- **SpdxRelationshipType** — from DemaConsulting.SpdxModel; used in LINQ verification of the relationship type.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
