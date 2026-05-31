## SelfTest

### Overview

The Self-Test subsystem implements the `--validate` self-test suite for DemaConsulting.SpdxTool. Core
SPDX-manipulation commands each have a dedicated step class that exercises them against embedded SPDX
fixtures to verify tool correctness after installation or deployment, without requiring external tools or
network access except where noted. The Help, Print, and SetVariable commands do not have dedicated step
classes; Print is exercised implicitly within ValidateQuery's workflow YAML, and Help and SetVariable
coverage is provided at the unit-test level.

The subsystem contains the following units:

- **Validate** — the orchestrator; entry point invoked by Program when the --validate flag is detected.
- **ValidateBasic** — exercises the validate command against valid and invalid SPDX documents.
- **ValidateAddPackage** — exercises the add-package command via a workflow file.
- **ValidateAddRelationship** — exercises the add-relationship command via a workflow file.
- **ValidateCopyPackage** — exercises the copy-package command via a workflow file.
- **ValidateDiagram** — exercises the diagram command directly.
- **ValidateFindPackage** — exercises the find-package command via a workflow file.
- **ValidateGetVersion** — exercises the get-version command via a workflow file.
- **ValidateHash** — exercises the hash generate and hash verify commands directly.
- **ValidateNtia** — exercises the validate command with the ntia flag on compliant and non-compliant documents.
- **ValidateQuery** — exercises the query command via a workflow file using dotnet --version.
- **ValidateRenameId** — exercises the rename-id command via a workflow file.
- **ValidateRunNuGetWorkflow** — exercises the run-workflow command with a NuGet package source.
- **ValidateToMarkdown** — exercises the to-markdown command directly.
- **ValidateUpdatePackage** — exercises the update-package command via a workflow file.

### Interfaces

**SelfTest Entry Point**: accepts a Program Context and runs the complete self-test suite.

- *Type*: Public static method on the Validate class.
- *Role*: Writes a system-information header, invokes all Validate step classes in sequence, collects
  TestResult entries, prints a pass/fail summary, and optionally writes a results file.
- *Contract*: Must be called with a fully initialized Context; distinguishes two failure modes:
  (1) step failures — a Validate step's `DoValidate` returns `false`, which is captured as a
  `TestResult.Failed` entry and the loop continues to the next step; (2) step exceptions — an
  uncaught exception thrown from a step's `DoValidate` propagates through the step's `Run` method
  into `Validate.Run`, aborting the validation loop immediately with no `TestResult` recorded for
  the failing step. Writes "Validation Passed" to the Context when all steps succeed with no
  errors.
- *Constraints*: The --validate flag must be detected by Program before normal command dispatch.

**RunSpdxTool Helper**: shared in-process runner used by all Validate step classes to invoke commands.

- *Type*: Internal static method on the Validate class (two overloads).
- *Role*: Creates a Context from the supplied arguments, calls Program.Run, disposes the Context, and
  returns the exit code.
- *Contract*: The working-folder overload temporarily changes the current directory to the supplied path
  before running and restores it in a finally block regardless of outcome.
- *Constraints*: Not thread-safe with respect to the current directory; all steps must run sequentially.

### Design

Validate.Run is the orchestrator. It writes a system-information header (tool version, machine name, OS
description, .NET runtime version, and UTC timestamp) to the Context output stream, then creates a
TestResults collection and invokes each Validate step class in a fixed sequence: ValidateAddPackage,
ValidateAddRelationship, ValidateBasic, ValidateCopyPackage, ValidateDiagram, ValidateFindPackage,
ValidateGetVersion, ValidateHash, ValidateNtia, ValidateQuery, ValidateRenameId, ValidateRunNuGetWorkflow,
ValidateToMarkdown, and ValidateUpdatePackage.

The heading written at the start of the report uses `Context.Depth` `#` characters (e.g., depth 1
produces `#`, depth 2 produces `##`). This controls the nesting level of the validation output, allowing
the report to be embedded at any heading level within a larger Markdown document.

Each step creates a validate.tmp directory, writes inline fixture files (SPDX JSON and/or workflow YAML),
calls Validate.RunSpdxTool to invoke one or more commands in-process, verifies the result by inspecting exit
codes or output file content, and deletes the temporary directory in a finally block. The step records its
outcome as a TestResult entry with a Passed or Failed status.

After all steps complete, Validate.Run tallies total, passed, and failed counts and writes them to the
Context. If no context errors were recorded it prints "Validation Passed". If Context.ValidationFile is set,
WriteResultsFile serializes the TestResults to either TRX format (for a .trx extension) using TrxSerializer
or JUnit XML format (for a .xml extension) using JUnitSerializer.

If `Context.ValidationFile` is set to a path with an extension other than `.trx` or `.xml`,
`WriteResultsFile` calls `context.WriteError` with the message
`"Unsupported results file format '{extension}'. Use .trx or .xml extension."` and returns
without writing any file. Calling `context.WriteError` increments the error count and results
in a non-zero exit code (see *Context Design*). The validation summary (pass/fail counts) is
still written to the output; only the result file is skipped.

Most steps run entirely in-process. ValidateQuery spawns dotnet as an external process and requires dotnet
on the system PATH. ValidateRunNuGetWorkflow may restore a NuGet package on a cache miss, which requires
network access to NuGet feeds.
