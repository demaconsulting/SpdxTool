### ValidateToMarkdown

#### Verification Approach

`ValidateToMarkdown` is verified by
`test/DemaConsulting.SpdxTool.Tests/SelfTest/ValidateToMarkdownTests.cs`, which runs the step
end-to-end and inspects the generated Markdown file.

#### Test Environment

The test uses temporary SPDX input and Markdown output files in the standard xUnit v3 environment. No
external service is required.

#### Acceptance Criteria

Verification is acceptable when the self-test step returns a passing result after generating the
expected Markdown summary.

#### Test Scenarios

**EndToEndMarkdownGeneration**: the self-test step proves that `to-markdown` generates a Markdown
summary during validation. This scenario is tested by `SpdxTool_ToMarkdown`.
