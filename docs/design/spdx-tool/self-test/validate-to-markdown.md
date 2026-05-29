### ValidateToMarkdown

#### Purpose

ValidateToMarkdown exercises the to-markdown command end-to-end within the Self-Test subsystem. It
verifies that an SPDX document can be converted to a Markdown summary file and that the output
contains the expected title, section headings, and package information.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the to-markdown self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: None.
- *Post-conditions*: A TestResult entry named SpdxTool_ToMarkdown has been appended to results; a
  pass or fail message has been written to the Context.

**DoValidate**: performs the actual to-markdown validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the Markdown output matches expectations.
- *Preconditions*: A writable working directory is available.
- *Post-conditions*: The validate.tmp directory has been deleted regardless of outcome.

Creates a validate.tmp directory and writes an SPDX JSON document containing two packages (Test
Application at version 1.0.0 with MIT license, and Test Library at version 2.0.0 with Apache-2.0
license). The document includes a DESCRIBES relationship from SPDXRef-DOCUMENT to SPDXRef-Application
and a CONTAINS relationship from SPDXRef-Application to SPDXRef-Library. Calls Validate.RunSpdxTool
with --silent, to-markdown,
the SPDX file path, an output .md file path, and the title "Test SBOM Summary". Verifies that the
output Markdown file exists and contains the title, "Root Packages" and "Packages" section headings,
both package names, and both version strings.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the output
Markdown file does not exist or does not contain all expected strings. Any exception thrown by
DoValidate propagates uncaught from Run; no TestResult is recorded for this step if an exception is
thrown — the exception surfaces to the Self-Test orchestrator. The finally block guards the
Directory.Delete call with a Directory.Exists check to prevent a secondary DirectoryNotFoundException
masking the original exception when Directory.CreateDirectory fails (e.g., because validate.tmp
already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the to-markdown command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
