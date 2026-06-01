### Validate

#### Purpose

Validate is the orchestrator for the Self-Test subsystem. It is the entry point invoked by Program when
the --validate flag is detected on the command line. It runs all Validate step classes in sequence,
collects pass/fail TestResult entries, prints a summary to the console, and optionally serializes the
results to a TRX or JUnit XML file.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the complete self-test suite using the supplied Program Context.

- *Parameters*: `context` — the active Program Context providing output and error streams.
- *Returns*: void.
- *Preconditions*: Context must be fully initialized; the --validate flag must have been detected by
  Program before this method is called.
- *Post-conditions*: All Validate step results are recorded in a TestResults collection; a pass/fail
  summary has been written to the Context; if Context.ValidationFile is set the results file is written.

Writes a system-information header (tool version, machine name, OS description, .NET runtime version,
UTC timestamp) before invoking any steps. The header uses `Context.Depth` `#` characters for its
Markdown heading level: depth 1 produces `#`, depth 2 produces `##`, and so on, allowing the report to
be embedded at any nesting level within a larger Markdown document. Computes total, passed, and failed
counts after all steps complete, writing "Validation Passed" if Context.Errors is zero.

**WriteResultsFile**: serializes the collected TestResults to the file path in Context.ValidationFile.

- *Parameters*: `context` — the active Program Context; `results` — the collected TestResults.
- *Returns*: void.
- *Preconditions*: Context.ValidationFile is non-null and non-empty.
- *Post-conditions*: A .trx or .xml file has been written; for an unsupported extension an error message
  is written to the Context and no file is produced. IO exceptions from the file-write operation
  (e.g., disk full, invalid path, permission denied) propagate unhandled to the caller as fatal errors.

Extension matching is case-insensitive; `.TRX` and `.XML` are also accepted.

**RunSpdxTool** (args overload): runs Program in-process with the supplied argument array.
Internal helper — only callable within the assembly.

- *Parameters*: `args` — the command-line arguments to pass to SpdxTool.
- *Returns*: `int` — the exit code from `context.ExitCode` after `Program.Run` completes.
- *Preconditions*: None.
- *Post-conditions*: A Context has been created, Program.Run has completed, and the Context has been
  disposed.

**RunSpdxTool** (workingFolder overload): changes the current directory then runs Program in-process.
Internal helper — only callable within the assembly.

- *Parameters*: `workingFolder` — directory to set as current before running; `args` — argument array.
- *Returns*: `int` — the exit code returned by Program.Run.
- *Preconditions*: workingFolder must exist on disk.
- *Post-conditions*: The current directory has been restored to its original value regardless of outcome.
- *Thread safety*: This method mutates global process state via Directory.SetCurrentDirectory.
  All Self-Test step classes using this overload must execute sequentially to prevent racing on
  the current working directory.

#### Error Handling

Individual step failures are captured as TestResult entries with TestOutcome.Failed and do not terminate
the orchestrator; all remaining steps continue to execute. An unsupported extension in
Context.ValidationFile causes an error message to be written to the Context and no file is produced.
Exceptions thrown within a step's DoValidate method propagate uncaught through the step's Run method
into Validate.Run, aborting the validation loop. No TestResult is recorded for the failing step; the
exception then propagates through Validate.Run to the caller. IO exceptions from File.WriteAllText in
WriteResultsFile (e.g., disk full, invalid path, permission denied) propagate unhandled through Run to
the caller; this is intentional because a file-write failure at this stage is a fatal error.

#### Dependencies

- **Context** — provides output and error streams and the ValidationFile property.
- **Program** — provides the Version constant and the Run method invoked by RunSpdxTool.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to collect results.
- **TrxSerializer** — from DemaConsulting.TestResults.IO; serializes results to TRX format.
- **JUnitSerializer** — from DemaConsulting.TestResults.IO; serializes results to JUnit XML format.
- **ValidateAddPackage** — step class invoked by Run.
- **ValidateAddRelationship** — step class invoked by Run.
- **ValidateBasic** — step class invoked by Run.
- **ValidateCopyPackage** — step class invoked by Run.
- **ValidateDiagram** — step class invoked by Run.
- **ValidateFindPackage** — step class invoked by Run.
- **ValidateGetVersion** — step class invoked by Run.
- **ValidateHash** — step class invoked by Run.
- **ValidateNtia** — step class invoked by Run.
- **ValidateQuery** — step class invoked by Run.
- **ValidateRenameId** — step class invoked by Run.
- **ValidateRunNuGetWorkflow** — step class invoked by Run.
- **ValidateToMarkdown** — step class invoked by Run.
- **ValidateUpdatePackage** — step class invoked by Run.

#### Callers

- **Program** — detects the --validate flag and calls Validate.Run(context) instead of dispatching
  to a normal command.
