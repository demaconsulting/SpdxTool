### RunWorkflow

#### Verification Approach

`RunWorkflow` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/RunWorkflowTests.cs`. The test suite covers local
workflow execution, parameter validation, default and explicit parameters, workflow outputs,
integrity checking, and workflow retrieval from NuGet packages or URLs.

#### Test Environment

The tests run in the standard xUnit v3 environment with local workflow files and SPDX fixtures. URL
and NuGet scenarios require HTTP access or a populated package cache.

#### Acceptance Criteria

Verification is acceptable when invalid workflows report clear errors, valid workflows execute their
steps in order, parameters and outputs behave as defined, and external workflow sources are resolved
securely.

#### Test Scenarios

**ValidFileExecution**: the unit executes all steps in a valid local workflow file. This
scenario is tested by `RunWorkflow_Run_ValidWorkflowFile_ExecutesWorkflow`.

**SpecifiedParametersOverride**: the unit uses the caller-supplied parameter values instead of
the workflow defaults when parameters are explicitly specified. This scenario is tested by
`RunWorkflow_Run_WithSpecifiedParameters_UsesSpecified`.

**UrlWorkflowExecution**: the unit downloads and executes a workflow from an HTTP URL. This
scenario is tested by `RunWorkflow_Run_UrlWorkflow_ExecutesWorkflow`.

**MissingFileError**: the unit reports an error when the specified workflow file does not exist.
This scenario is tested by `RunWorkflow_Run_MissingFile_ReportsError`.

**MissingParameterError**: the unit reports an error when a workflow step is missing a required
input parameter. This scenario is tested by `RunWorkflow_Run_MissingParameter_ReportsError`.

**MissingArguments**: the unit reports a usage error when no workflow source is supplied. This
scenario is tested by `RunWorkflow_Run_MissingArguments_ReportsError`.

**InvalidWorkflowStructure**: the unit reports an error for malformed workflow content. This
scenario is tested by `RunWorkflow_Run_InvalidWorkflowFile_ReportsError`.

**DefaultParameters**: the unit uses declared default parameter values when explicit overrides are
not provided. This scenario is tested by `RunWorkflow_Run_WithDefaultParameters_UsesDefaults`.

**OutputExtraction**: the unit exposes requested workflow outputs after execution. This scenario is
tested by `RunWorkflow_Run_WithOutputs_PopulatesOutputs`.

**IntegrityAccepted**: the unit executes a workflow whose SHA-256 hash matches the expected integrity
value. This scenario is tested by `RunWorkflow_Run_WithValidIntegrity_ExecutesWorkflow`.

**IntegrityRejected**: the unit rejects workflow content whose integrity hash does not match the
expected value. This scenario is tested by `RunWorkflow_Run_WithBadIntegrity_ReportsError`.

**ExternalWorkflowSources**: the unit executes workflows retrieved from NuGet packages or URLs. This
scenario is tested by `RunWorkflow_Run_NuGetWorkflow_ExecutesWorkflow`.

**UndeclaredParameter**: the unit reports an error when a caller-supplied CLI parameter key is not
declared in the workflow's parameters section. This scenario is tested by
`RunWorkflow_Run_UndeclaredParameter_ReportsError`.

**MalformedCliArgument**: the unit reports a usage error when a CLI argument does not contain the
`=` separator. This scenario is tested by `RunWorkflow_Run_MalformedCliArgument_ReportsError`.

**VerboseOutput**: the unit prints each workflow step's outputs to the console when the `--verbose`
flag is supplied. This scenario is tested by `RunWorkflow_Run_WithVerboseFlag_PrintsOutputs`.

**DisplayName**: the unit prints the `displayName` label for each workflow step that declares one.
This scenario is tested by `RunWorkflow_Run_WithDisplayName_PrintsLabel`.
