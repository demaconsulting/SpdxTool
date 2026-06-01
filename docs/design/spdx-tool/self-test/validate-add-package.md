### ValidateAddPackage

#### Purpose

ValidateAddPackage exercises the add-package command end-to-end within the Self-Test subsystem. It
verifies that a package can be added to an SPDX document via a workflow file and that the resulting
document contains the expected package and relationship entries.

#### Data Model

`PreRunSpdxToolHookForTest` — an internal property, `null` in production, that tests may set to a
delegate invoked immediately before `Validate.RunSpdxTool` is called. Allows tests to corrupt
`validate.tmp/test.spdx.json` to exercise the CommandFailure path deterministically.

`PostRunSpdxToolHookForTest` — an internal property, `null` in production, that tests may set to a
delegate invoked immediately after `Validate.RunSpdxTool` returns exit code zero and before the
content-verification step. Allows tests to replace the output SPDX document with content containing
wrong package IDs to exercise the ContentMismatch path deterministically.

> **Working-directory isolation**: the command-failure and content-mismatch tests change the
> process-wide current directory to a unique temporary path via `Directory.SetCurrentDirectory`
> before running. This is required because `DoValidate` creates and deletes `validate.tmp` relative
> to the current directory; without isolation, concurrent or sequential test runs could collide on the
> same `validate.tmp` path in the repository root. The success test omits this step because it relies
> on the `SelfTestValidation` collection serializing test execution, but the failure tests additionally
> need to guarantee a clean working directory that is not shared with other in-flight tests.

#### Key Methods

**Run**: executes the add-package self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: None.
- *Post-conditions*: If DoValidate returns without throwing, a TestResult entry named
  SpdxTool_AddPackage has been appended to results; a pass or fail message has been written to the
  Context.

**DoValidate**: performs the actual add-package validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the SPDX document matches expectations.
- *Preconditions*: A writable working directory is available.
- *Post-conditions*: The validate.tmp directory has been deleted regardless of outcome.

Creates a validate.tmp directory, writes a minimal SPDX JSON document containing one package
(SPDXRef-Package-1), and writes a workflow YAML that executes add-package to add SPDXRef-Package-2
with a BUILD_TOOL_OF relationship to SPDXRef-Package-1 and a purl external reference. Calls
Validate.RunSpdxTool with --silent and run-workflow arguments, then reads the modified SPDX document
and verifies the content using a positional list pattern match — package and relationship order in
the deserialized document is significant.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the deserialized
SPDX document does not contain exactly two packages with the expected IDs or does not contain the
expected BUILD_TOOL_OF relationship. Any exception thrown by DoValidate propagates uncaught from Run;
no TestResult is recorded for this step if an exception is thrown — the exception surfaces to the
Self-Test orchestrator. The validate.tmp directory is deleted in a finally block only if it exists,
guarding against a secondary DirectoryNotFoundException masking the original exception when
Directory.CreateDirectory fails.

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the add-package command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.
- **Spdx2JsonDeserializer** — from DemaConsulting.SpdxModel.IO; deserializes the output SPDX document
  for structural verification.
- **SpdxRelationshipType** — from DemaConsulting.SpdxModel; used in pattern-matching the relationship.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
