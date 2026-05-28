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
scenario is tested by `AddRelationship_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the target SPDX file does not exist. This
scenario is tested by `AddRelationship_MissingFile_ReportsError`.

**CommandLineRelationshipAddition**: the unit adds a relationship when invoked from the command line
with valid arguments. This scenario is tested by `AddRelationship_OnCommandLine_AddsRelationship`.

**WorkflowRelationshipAddition**: the unit adds a relationship when invoked from a workflow step.
This scenario is tested by `AddRelationship_InWorkflow_AddsRelationship`.

**ReplaceMode**: the unit replaces existing relationships when replacement mode is requested. This
scenario is tested by `AddRelationship_ReplaceMode_ReplacesExistingRelationship`.

**MissingSpdxInput**: the unit reports an error when the `spdx` input is missing from the workflow
step. This scenario is tested by `AddRelationship_InWorkflowMissingSpdxInput_ReportsError`.

**MissingIdInput**: the unit reports an error when the `id` input is missing from the workflow step.
This scenario is tested by `AddRelationship_InWorkflowMissingIdInput_ReportsError`.

**MissingRelationshipsInput**: the unit reports an error when the `relationships` input is missing
from the workflow step.
This scenario is tested by `AddRelationship_InWorkflowMissingRelationshipsInput_ReportsError`.

**InvalidReplaceValue**: the unit reports an error when the `replace` input is not a valid boolean
value. This scenario is tested by `AddRelationship_InWorkflowInvalidReplaceValue_ReportsError`.

**NonMappingRelationshipNode**: the unit reports an error when a relationship entry is not a YAML
mapping node. This scenario is tested by
`AddRelationship_InWorkflowNonMappingRelationshipNode_ReportsError`.

**MissingRelationshipType**: the unit reports an error when a relationship entry is missing the
`type` field.
This scenario is tested by `AddRelationship_InWorkflowMissingRelationshipType_ReportsError`.

**MissingRelationshipElement**: the unit reports an error when a relationship entry is missing the
`element` field.
This scenario is tested by `AddRelationship_InWorkflowMissingRelationshipElement_ReportsError`.
