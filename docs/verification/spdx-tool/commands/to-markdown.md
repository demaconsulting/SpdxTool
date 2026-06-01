### ToMarkdown

#### Verification Approach

`ToMarkdown` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/ToMarkdownTests.cs`. The tests cover missing arguments,
missing input-file handling, and Markdown summary generation from a valid SPDX document.

#### Test Environment

The tests use local SPDX JSON input and Markdown output files in the standard xUnit v3 environment. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when:

- Missing or insufficient CLI arguments are rejected with a usage error.
- A whitespace-only title argument is rejected with a usage error.
- A non-positive depth argument is rejected with a usage error.
- A missing SPDX input file is rejected with an error.
- Valid SPDX input produces the expected Markdown summary file with correct headings, metadata,
  and package classification.
- Valid SPDX input processed via a workflow YAML step produces the expected Markdown summary file.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when required arguments are omitted. This
scenario is tested by `ToMarkdown_Run_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the SPDX input file does not exist. This
scenario is tested by `ToMarkdown_Run_MissingSpdxFile_ReportsError`.

**MarkdownGeneration**: the unit generates a Markdown summary from a valid SPDX document. This
scenario is tested by `ToMarkdown_Run_ValidSpdxFile_GeneratesMarkdown`.

**WorkflowInvocation**: the unit generates a Markdown summary when invoked from a workflow YAML
file. This scenario is tested by `ToMarkdown_Run_InWorkflow_GeneratesMarkdown`.

**InvalidTitle**: the unit reports a usage error when the title argument is empty or contains only
whitespace. This scenario is tested by `ToMarkdown_Run_InvalidTitle_ReportsError`.

**InvalidDepth**: the unit reports a usage error when the depth argument is not a positive integer.
This scenario is tested by `ToMarkdown_Run_InvalidDepth_ReportsError`.

**YamlMissingSpdx**: the unit throws a YAML exception when the spdx input is absent from the
workflow step. This scenario is tested by `ToMarkdown_Run_YamlMissingSpdxInput_ThrowsException`.

**YamlMissingMarkdown**: the unit throws a YAML exception when the markdown input is absent from
the workflow step. This scenario is tested by `ToMarkdown_Run_YamlMissingMarkdownInput_ThrowsException`.

**YamlWhitespaceTitle**: the unit throws a YAML exception when the title input is whitespace or
empty in a workflow step. This scenario is tested by `ToMarkdown_Run_YamlWhitespaceTitle_ThrowsException`.

**YamlNonPositiveDepth**: the unit throws a YAML exception when the depth input is non-positive in
a workflow step. This scenario is tested by `ToMarkdown_Run_YamlNonPositiveDepth_ThrowsException`.
