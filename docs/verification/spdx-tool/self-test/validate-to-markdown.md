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

**CommandFailure**: when the to-markdown command exits with a non-zero exit code (triggered via the
`PreRunSpdxToolHookForTest` hook corrupting `validate.tmp/test-markdown.spdx.json`), `Run` records
`TestOutcome.Failed` and no exception propagates. This scenario is tested by
`ValidateToMarkdown_Run_CommandFailure_RecordsFailedOutcome`.

**IoError**: when `validate.tmp` cannot be created as a directory (e.g., it pre-exists as a file),
`Run` propagates the `IOException` uncaught and records no `TestResult`. This scenario is tested by
`ValidateToMarkdown_Run_IoError_PropagatesException`.
