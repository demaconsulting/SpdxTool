### ValidateRunNuGetWorkflow

#### Purpose

ValidateRunNuGetWorkflow exercises the run-workflow command's NuGet package source support within the
Self-Test subsystem. It verifies that a workflow file can be resolved from a NuGet package in the
local cache and executed successfully, with outputs captured into workflow variables.

#### Data Model

N/A - this unit is a static class with no instance state. The `PreRunSpdxToolHookForTest` property
holds an optional `Action` delegate that is `null` in production; tests may set it to corrupt fixture
files immediately before `Validate.RunSpdxTool` is called, exercising the CommandFailure path.

#### Key Methods

**Run**: executes the NuGet workflow self-test and records the result.

- *Parameters*: `context` — the active Program Context; `results` — the TestResults collection to
  append to.
- *Returns*: void.
- *Preconditions*: Sequential invocation is required; concurrent calls race on the process-wide
  current directory mutated by `Validate.RunSpdxTool`.
- *Post-conditions*: A TestResult entry named SpdxTool_RunNuGetWorkflow has been appended to results;
  a pass or fail message has been written to the Context.

**DoValidate**: performs the actual NuGet workflow validation in a temporary directory.

- *Parameters*: None.
- *Returns*: `bool` — true if RunSpdxTool returns exit code zero.
- *Preconditions*: The DemaConsulting.SpdxWorkflows NuGet package must be resolvable from the local
  NuGet cache or from the configured NuGet feeds.
- *Post-conditions*: The validate.tmp directory has been deleted if it exists; if Directory.CreateDirectory
  never succeeded, the delete is skipped rather than raising a secondary exception.

Creates a validate.tmp directory and writes a workflow YAML that uses the nuget input to reference
`DemaConsulting.SpdxWorkflows` version `1.0.0` and the `contentFiles/any/any/workflows/GetDotNetVersion.yaml` workflow file within it,
mapping its version output to the dotnet-version variable and then printing it. Calls
Validate.RunSpdxTool with --silent and run-workflow arguments. Returns true if the exit code is zero.

#### Error Handling

Returns false if Validate.RunSpdxTool returns a non-zero exit code. This may occur if the NuGet
package cannot be resolved because the package is absent from the local cache and network access is
unavailable. Any exception thrown by DoValidate propagates uncaught from Run; no TestResult is
recorded for this step if an exception is thrown — the exception surfaces to the Self-Test
orchestrator. The finally block guards the Directory.Delete call with a Directory.Exists check to
prevent a secondary DirectoryNotFoundException masking the original exception when
Directory.CreateDirectory fails (e.g., because validate.tmp already exists as a file).

#### Dependencies

- **Validate** — provides the RunSpdxTool helper used to invoke the run-workflow command.
- **Context** — provides output and error streams for pass/fail reporting.
- **TestResults / TestResult / TestOutcome** — from DemaConsulting.TestResults; used to record the
  step outcome.

#### Callers

- **Validate** — the Self-Test orchestrator invokes this step.
