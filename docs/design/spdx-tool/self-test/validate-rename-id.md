### ValidateRenameId

#### Purpose

ValidateRenameId exercises the rename-id command end-to-end within the Self-Test subsystem. It
verifies that an SPDX element ID can be renamed throughout an SPDX document via a workflow file, with
all relationship references updated to reflect the new identifier.

#### Data Model

N/A - this unit is a static class with no instance state. The `PreRunSpdxToolHookForTest` property
holds an optional `Action` delegate that is `null` in production; tests may set it to corrupt fixture
files immediately before `Validate.RunSpdxTool` is called, exercising the CommandFailure path.

#### Key Methods

**Run**: executes the rename-id self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: A TestResult entry named SpdxTool_RenameId has been appended to results; a pass
  or fail message has been written to the Context.

**DoValidate**: performs the actual rename-id validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the SPDX document reflects the rename.
- *Preconditions*: A writable working directory is available.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if Directory.CreateDirectory
  never succeeded, the delete is skipped rather than raising a secondary exception.

Creates a validate.tmp directory, writes an SPDX JSON document containing one package with ID
SPDXRef-Package-1, a documentDescribes entry referencing that ID, and a DESCRIBES relationship
referencing that ID, and writes a workflow YAML that executes rename-id to rename SPDXRef-Package-1
to SPDXRef-Package-2. Calls Validate.RunSpdxTool with --silent and run-workflow arguments. Returns
`false` immediately if the output SPDX file is absent after tool invocation, without attempting
deserialization. Otherwise reads the modified SPDX document and verifies that the package ID is now
SPDXRef-Package-2, the relationship's related element has also been updated, and the documentDescribes
entry has been updated to SPDXRef-Package-2.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the output SPDX
file is absent after the rename-id tool invocation. Returns false if `SPDXRef-Package-2` is not
found in the packages collection, if `SPDXRef-Package-2` is not found as the related element of any
relationship, or if `SPDXRef-Package-2` is not found in the `documentDescribes` entries. Any
exception thrown by DoValidate propagates uncaught from Run; no TestResult is recorded for this step
if an exception is thrown — the exception surfaces to the Self-Test orchestrator. The finally block
guards the Directory.Delete call with a Directory.Exists check to prevent a secondary
DirectoryNotFoundException masking the original exception when Directory.CreateDirectory fails
(e.g., because validate.tmp already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the rename-id command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.
- **Spdx2JsonDeserializer** — from DemaConsulting.SpdxModel.IO; deserializes the output SPDX document
  for structural verification.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
