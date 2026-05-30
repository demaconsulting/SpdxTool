### ValidateDiagram

#### Purpose

ValidateDiagram exercises the diagram command end-to-end within the Self-Test subsystem. It verifies
that a Mermaid entity-relationship diagram can be generated from an SPDX document and that the output
file is created with the expected diagram syntax and package content.

#### Data Model

N/A - this unit is a static class with no instance state.

#### Key Methods

**Run**: executes the diagram self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: A TestResult entry named SpdxTool_Diagram has been appended to results (when no
  exception is thrown); the entry has `Name` set to `SpdxTool_Diagram`, `Outcome` set to Passed or
  Failed, `ClassName` set to `DemaConsulting.SpdxTool.SelfTest.ValidateDiagram`, `ComputerName` set
  to the current machine name, and `StartTime` set to the time of execution; a pass or fail message
  has been written to the Context.

**DoValidate**: performs the actual diagram validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if the command succeeded and the output file contains expected content.
- *Preconditions*: A writable working directory is available. Note: `Validate.RunSpdxTool` temporarily
  mutates the process-wide current working directory; callers must ensure serial execution to avoid
  races on that shared state.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if Directory.CreateDirectory
  never succeeded, the delete is skipped rather than raising a secondary exception.

Creates a validate.tmp directory and writes an SPDX JSON document containing two packages (Test
Application and Test Library) connected by a DEPENDS_ON relationship. Calls Validate.RunSpdxTool
with --silent, diagram, the SPDX file path, and an output .txt file path. Verifies that the output
file exists, contains the erDiagram keyword, references both package names and versions, and contains
the DEPENDS_ON relationship label.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. Returns false if the output
Mermaid file does not exist or does not contain the expected diagram syntax, package names, or
relationship labels. The finally block guards the Directory.Delete call with a Directory.Exists check
to prevent a secondary DirectoryNotFoundException masking the original exception when
Directory.CreateDirectory fails (e.g., because validate.tmp already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the diagram command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
