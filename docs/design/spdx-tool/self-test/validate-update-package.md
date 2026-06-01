### ValidateUpdatePackage

#### Purpose

ValidateUpdatePackage exercises the update-package command end-to-end within the Self-Test subsystem.
It verifies that all updatable metadata fields of a package in an SPDX document can be modified via
a workflow file and that the resulting document reflects every changed value.

#### Data Model

N/A - this unit is a static class with no instance state.

**PreRunSpdxToolHookForTest**: `internal static Action? PreRunSpdxToolHookForTest { get; set; }` —
optional test hook that is `null` in production. Tests may set this to a delegate that corrupts
`validate.tmp/test.spdx.json` immediately before `Validate.RunSpdxTool` is called, exercising the
CommandFailure path. Callers must reset this property to `null` after the test completes.

#### Key Methods

**Run**: executes the update-package self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: A TestResult entry named SpdxTool_UpdatePackage has been appended to results; a
  pass or fail message has been written to the Context.

**DoValidate**: performs the actual update-package validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and every updated field matches the new value.
- *Preconditions*: A writable working directory is available. Callers must execute serially because
  Validate.RunSpdxTool temporarily mutates the process-wide current working directory.
- *Post-conditions*: The validate.tmp directory has been deleted in a finally block only if it exists,
  guarding against a secondary `DirectoryNotFoundException` masking the original exception.

Creates a validate.tmp directory, writes an SPDX JSON document containing a single package
(SPDXRef-Package-1) with initial metadata, and writes a workflow YAML that executes update-package
to change eleven fields: name, download location, version, filename, supplier, originator, homepage,
copyright text, summary, description, and license. Calls Validate.RunSpdxTool with --silent and
run-workflow arguments, then reads the modified SPDX document and uses LINQ to locate the package
by SPDX Id (`SPDXRef-Package-1`) — confirming correct package identity — then individually verifies
all twelve updated SPDX fields match the values specified in the workflow. The `license` workflow
input maps to two SPDX fields — `ConcludedLicense` and `DeclaredLicense` — which is why eleven
workflow inputs result in twelve SPDX field verifications; the Id check is a thirteenth assertion
that guards against verifying the wrong package.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the output SPDX
file (`validate.tmp/test.spdx.json`) is absent after the update-package tool exits successfully.
Returns false if the deserialized SPDX document does not contain a package with Id `SPDXRef-Package-1`
or does not exactly match all twelve updated SPDX field values (name, download location, version,
filename, supplier, originator, homepage, copyright text, summary, description, ConcludedLicense, and
DeclaredLicense — the last two both set by the single `license` workflow input). The temporary
directory is always deleted in a finally block, guarded with a Directory.Exists check to prevent a
secondary DirectoryNotFoundException from masking the original exception.

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
