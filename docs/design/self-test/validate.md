# DemaConsulting.SpdxTool SelfTest Orchestrator Design

## Purpose

`Validate.cs` is the entry point for the SelfTest subsystem. When the `Program`
class detects the `--validate` flag, it calls `Validate.Run(Context)` instead of
dispatching to a normal command. The orchestrator collects pass/fail results from
each individual validation step and writes a summary report.

## Architecture

`Validate.Run` performs the following work:

1. Writes a system information header (version, OS, .NET runtime, timestamp).
2. Creates a `TestResults` object to collect results.
3. Invokes each `Validate*` step class in order:
   - `ValidateAddPackage`, `ValidateAddRelationship`, `ValidateBasic`,

     `ValidateCopyPackage`, `ValidateDiagram`, `ValidateFindPackage`,
     `ValidateGetVersion`, `ValidateHash`, `ValidateNtia`, `ValidateQuery`,
     `ValidateRenameId`, `ValidateRunNuGetWorkflow`, `ValidateToMarkdown`,
     `ValidateUpdatePackage`.

4. Computes totals (total, passed, failed) and writes to the console.
5. If `Context.Errors == 0`, writes "Validation Passed".
6. If `Context.ValidationFile` is set, calls `WriteResultsFile`.

## Result File Writing

`WriteResultsFile` detects the file extension:

- `.trx` → serializes with `TrxSerializer.Serialize`.
- `.xml` → serializes with `JUnitSerializer.Serialize` (JUnit format).
- Other → writes an error to the context.

## Helper Methods

- `RunSpdxTool(string[] args)` — creates a `Context`, runs `Program.Run`, returns

  the exit code. Used by all `Validate*` step classes.

- `RunSpdxTool(string workingFolder, string[] args)` — temporarily changes the

  current directory before calling `RunSpdxTool(args)`.

## Error Handling

- Individual step failures are captured in `TestResult` entries with

  `TestOutcome.Failed`; they do not terminate the orchestrator.

- Unsupported result file extensions result in a context error message.

## Constraints

- Self-validation is entirely in-process; no external tools or network access are

  required (other than the NuGet workflow test).

- Each step uses a `validate.tmp` temporary directory, created and deleted within

  the step.

- The orchestrator does not throw exceptions on individual step failures; it

  continues executing all steps.
