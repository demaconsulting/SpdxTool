### ValidateBasic

#### Purpose

ValidateBasic exercises the validate command end-to-end within the Self-Test subsystem. It verifies
that the tool correctly accepts a well-formed SPDX document and rejects a malformed one, confirming
that basic validation logic functions correctly after installation.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the basic validation self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: None.
- *Post-conditions*: When DoValidate returns without throwing, a TestResult entry named SpdxTool_Basic
  has been appended to results and a pass or fail message has been written to the Context. If DoValidate
  throws an exception the exception propagates uncaught and no TestResult is recorded.

**DoValidate**: performs both sub-tests in a shared temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if both DoValidateValid and DoValidateInvalid succeed.
- *Preconditions*: A writable working directory is available.
- *Post-conditions*: The validate.tmp directory has been deleted regardless of outcome.

**DoValidateValid**: verifies that a well-formed SPDX document passes validation.

- *Parameters*: None.
- *Returns*: `bool` — true if RunSpdxTool returns exit code zero.
- *Preconditions*: validate.tmp exists.
- *Post-conditions*: A valid SPDX document has been written to validate.tmp and validated.

Writes a SPDX JSON document with a single package and a DESCRIBES relationship, then calls
Validate.RunSpdxTool with --silent and validate arguments. Expects exit code zero.

**DoValidateInvalid**: verifies that a malformed SPDX document fails validation.

- *Parameters*: None.
- *Returns*: `bool` — true if RunSpdxTool returns a non-zero exit code and the log contains the
  expected error text.
- *Preconditions*: validate.tmp exists.
- *Post-conditions*: An invalid SPDX document has been written to validate.tmp and validated.

Writes an SPDX JSON document with a package missing the required SPDXID field, then calls
Validate.RunSpdxTool with --silent, --log, and validate arguments. Expects a non-zero exit code and
verifies that the log file references the validation issue.

#### Error Handling

Returns false if `DoValidateValid` returns false (i.e., when the underlying `RunSpdxTool` call returns
a non-zero exit code). Returns false if DoValidateInvalid
returns exit code zero (validation unexpectedly passed) or if the log file does not contain expected
error text. The temporary directory is always deleted in a finally block.

Exceptions thrown by DoValidate propagate uncaught through Run; callers must handle them. Because
RunSpdxTool changes the process-wide current directory, all callers must ensure sequential execution
of ValidateBasic to avoid races on the validate.tmp directory and on the process-wide current
directory.

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the validate command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
