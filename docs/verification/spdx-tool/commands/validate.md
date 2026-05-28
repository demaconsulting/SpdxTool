### Validate

#### Verification Approach

`Validate` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/ValidateTests.cs`. The tests cover missing arguments,
missing-file handling, successful SPDX validation, success for documents with no files analyzed, and
NTIA-specific pass or fail outcomes.

#### Test Environment

The tests use local SPDX JSON fixtures in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when invalid inputs are rejected, valid SPDX documents complete without
errors, and NTIA mode reports compliance failures only when required minimum elements are missing.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when no SPDX file is supplied. This scenario is
tested by `Validate_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the SPDX file does not exist. This scenario is
tested by `Validate_MissingSpdxFile_ReportsError`.

**ValidDocument**: the unit accepts a conformant SPDX document. This scenario is tested by
`Validate_ValidSpdxDocument_Succeeds`.

**NoFilesAnalyzed**: the unit accepts a valid SPDX document that does not contain analyzed files.
This scenario is tested by `Validate_ValidDocumentNoFilesAnalyzed_Succeeds`.

**NtiaCompliance**: the unit accepts an SPDX document that satisfies NTIA minimum elements. This
scenario is tested by `Validate_NtiaValidDocument_Succeeds`.

**NtiaFailureReporting**: the unit reports NTIA validation errors for a non-compliant SPDX document.
This scenario is tested by `Validate_NtiaInvalidDocument_ReportsNtiaErrors`.
