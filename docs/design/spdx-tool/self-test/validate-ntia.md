### ValidateNtia

#### Purpose

ValidateNtia exercises the NTIA minimum elements validation within the Self-Test subsystem. It
verifies that the validate command correctly detects missing NTIA-required fields in a non-compliant
SPDX document and accepts a fully compliant one.

#### Data Model

The `PreRunSpdxToolHookForTest` property
holds an optional `Action` delegate that is `null` in production. When set by a test, the delegate is
invoked in `DoValidateMissingSupplier` immediately after the fixture file is written and before
`Validate.RunSpdxTool` is called. This hook allows tests to corrupt the fixture so that the validate
command returns a non-zero exit code, exercising the CommandFailure path without spawning an external
process.

#### Key Methods

**Run**: executes the NTIA self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: A TestResult entry named SpdxTool_Ntia has been appended to results; a pass or
  fail message has been written to the Context.

**DoValidate**: orchestrates both sub-tests in a shared temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if both DoValidateMissingSupplier and DoValidateCompliant succeed.
- *Preconditions*: A writable working directory is available.
- *Post-conditions*: The validate.tmp directory has been deleted regardless of outcome.

**DoValidateMissingSupplier**: verifies that a document missing the supplier field fails NTIA validation.

- *Parameters*: None.
- *Returns*: `bool` — true if basic validation passes (exit code zero) and NTIA validation fails
  (non-zero exit code) with an appropriate error in the log.
- *Preconditions*: validate.tmp exists.
- *Post-conditions*: An SPDX document and log file have been written to validate.tmp.

Writes an SPDX JSON document with a package that has no supplier field. Runs validate without the
ntia flag (expects exit code zero) and then runs validate with the ntia flag (expects non-zero exit
code). Reads the log file and verifies it contains the text "NTIA: Package 'Test Package' Missing
Supplier".

**DoValidateCompliant**: verifies that a fully NTIA-compliant document passes NTIA validation.

- *Parameters*: None.
- *Returns*: `bool` — true if RunSpdxTool returns exit code zero.
- *Preconditions*: validate.tmp exists.
- *Post-conditions*: An NTIA-compliant SPDX document has been written and validated.

Writes an SPDX JSON document with a package that includes the supplier field, then calls
Validate.RunSpdxTool with --silent, validate, the SPDX file path, and the ntia flag. Expects exit
code zero.

#### Error Handling

Returns false if basic validation of the non-compliant document returns a non-zero exit code, if NTIA
validation of the non-compliant document returns exit code zero, if the log file is absent after the
NTIA validation run, or if the log does not contain the expected "Missing Supplier" error text.
Returns false if NTIA validation of the compliant document
returns a non-zero exit code. Any exception thrown by DoValidate propagates uncaught from Run; no
TestResult is recorded for this step if an exception is thrown — the exception surfaces to the
Self-Test orchestrator. The finally block guards the Directory.Delete call with a Directory.Exists
check to prevent a secondary DirectoryNotFoundException masking the original exception when
Directory.CreateDirectory fails (e.g., because validate.tmp already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the validate command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
