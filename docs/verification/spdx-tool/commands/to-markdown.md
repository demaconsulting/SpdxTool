### ToMarkdown

#### Verification Approach

`ToMarkdown` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/ToMarkdownTests.cs`. The tests cover missing arguments,
missing input-file handling, and Markdown summary generation from a valid SPDX document.

#### Test Environment

The tests use local SPDX JSON input and Markdown output files in the standard xUnit v3 environment. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when invalid invocations are rejected and valid SPDX input produces the
expected Markdown summary file.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when required arguments are omitted. This
scenario is tested by `ToMarkdown_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the SPDX input file does not exist. This
scenario is tested by `ToMarkdown_MissingSpdxFile_ReportsError`.

**MarkdownGeneration**: the unit generates a Markdown summary from a valid SPDX document. This
scenario is tested by `ToMarkdown_ValidSpdxFile_GeneratesMarkdown`.

**WorkflowInvocation**: the unit generates a Markdown summary when invoked from a workflow YAML
file. This scenario is tested by `ToMarkdown_Run_InWorkflow_GeneratesMarkdown`.

**InvalidTitle**: the unit reports a usage error when the title argument is empty or contains only
whitespace. This scenario is tested by `ToMarkdown_InvalidTitle_ReportsError`.

**InvalidDepth**: the unit reports a usage error when the depth argument is not a positive integer.
This scenario is tested by `ToMarkdown_InvalidDepth_ReportsError`.
