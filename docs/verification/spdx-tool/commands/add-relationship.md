### AddRelationship

#### Verification Approach

`AddRelationship` is verified with command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/AddRelationshipTests.cs`. The tests exercise CLI
invocation, workflow invocation, replacement behavior, and input validation for missing arguments or
files.

#### Test Environment

The tests use local SPDX fixture files in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when valid inputs create or replace relationships as requested and
invalid inputs report usage or file errors without corrupting the source document.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when the required arguments are omitted. This
scenario is tested by `AddRelationship_Run_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the target SPDX file does not exist. This
scenario is tested by `AddRelationship_Run_MissingFile_ReportsError`.

**CommandLineRelationshipAdditionWithoutComment**: the unit adds a relationship without a comment when
invoked from the command line with the four-argument form (no optional comment argument). This scenario
is tested by `AddRelationship_Run_OnCommandLine_WithoutComment_AddsRelationship`.

**CommandLineRelationshipAddition**: the unit adds a relationship with a comment when invoked from the
command line with the five-argument form. This scenario is tested by
`AddRelationship_Run_OnCommandLine_AddsRelationship`.

**WorkflowRelationshipAddition**: the unit adds a relationship when invoked from a workflow step.
This scenario is tested by `AddRelationship_Run_InWorkflow_AddsRelationship`.

**ReplaceMode**: the unit replaces existing relationships when replacement mode is requested. This
scenario is tested by `AddRelationship_Run_ReplaceMode_ReplacesExistingRelationship`.

**MissingSpdxInput**: the unit reports an error when the `spdx` input is missing from the workflow
step. This scenario is tested by `AddRelationship_Run_InWorkflowMissingSpdxInput_ReportsError`.

**MissingIdInput**: the unit reports an error when the `id` input is missing from the workflow step.
This scenario is tested by `AddRelationship_Run_InWorkflowMissingIdInput_ReportsError`.

**MissingRelationshipsInput**: the unit reports an error when the `relationships` input is missing
from the workflow step.
This scenario is tested by `AddRelationship_Run_InWorkflowMissingRelationshipsInput_ReportsError`.

**InvalidReplaceValue**: the unit reports an error when the `replace` input is not a valid boolean
value. This scenario is tested by `AddRelationship_Run_InWorkflowInvalidReplaceValue_ReportsError`.

**NonMappingRelationshipNode**: the unit reports an error when a relationship entry is not a YAML
mapping node. This scenario is tested by
`AddRelationship_Run_InWorkflowNonMappingRelationshipNode_ReportsError`.

**MissingRelationshipType**: the unit reports an error when a relationship entry is missing the
`type` field.
This scenario is tested by `AddRelationship_Run_InWorkflowMissingRelationshipType_ReportsError`.

**MissingRelationshipElement**: the unit reports an error when a relationship entry is missing the
`element` field.
This scenario is tested by `AddRelationship_Run_InWorkflowMissingRelationshipElement_ReportsError`.
