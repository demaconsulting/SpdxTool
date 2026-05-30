### GetVersion

#### Verification Approach

`GetVersion` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/GetVersionTests.cs`. The tests cover argument
validation, file validation, direct command invocation, and workflow-based version retrieval.

#### Test Environment

The tests use local SPDX JSON and workflow YAML fixtures in the standard xUnit v3 environment. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when invalid invocations are rejected and successful invocations return
the expected package version for the matching package criteria.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when the SPDX path or search criteria are
omitted. This scenario is tested by `GetVersion_Run_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the referenced SPDX file does not exist. This
scenario is tested by `GetVersion_Run_MissingFile_ReportsError`.

**CommandLineRetrieval**: the unit accepts direct CLI invocation and writes the matched package version to the console.
This scenario is tested by `GetVersion_Run_OnCommandLine_ReturnsPackageVersion`.

**WorkflowRetrieval**: the unit stores the matched package version for downstream workflow steps.
This scenario is tested by `GetVersion_Run_InWorkflow_ReturnsPackageVersion`.

**PackageNotFound**: the unit reports an error when no package in the SPDX document matches the
supplied criteria. This scenario is tested by `GetVersion_Run_PackageNotFound_ReportsError`.

**MissingWorkflowInputs**: the unit reports an error when a required workflow input (`spdx` or
`output`) is absent from the step. This scenario is tested by
`GetVersion_Run_MissingWorkflowInputs_ReportsError`.
