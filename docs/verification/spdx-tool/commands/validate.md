### Validate

#### Verification Approach

`Validate` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/ValidateTests.cs`. The tests cover missing arguments,
missing-file handling, successful SPDX validation, success for documents with no files analyzed,
NTIA-specific pass or fail outcomes, YAML workflow invocation, missing YAML input detection, and
case-insensitive NTIA YAML input.

#### Test Environment

The tests use local SPDX JSON fixtures in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when invalid inputs are rejected, valid SPDX documents complete without
errors, and NTIA mode reports compliance failures only when required minimum elements are missing.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when no SPDX file is supplied. This scenario is
tested by `Validate_Run_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the SPDX file does not exist. This scenario is
tested by `Validate_Run_MissingSpdxFile_ReportsError`.

**ValidDocument**: the unit accepts a conformant SPDX document. This scenario is tested by
`Validate_Run_ValidSpdxDocument_Succeeds` and `Validate_DoValidate_ValidSpdxFile_ReturnsNoErrors`.

**NoFilesAnalyzed**: the unit accepts a valid SPDX document that does not contain analyzed files.
This scenario is tested by `Validate_Run_ValidDocumentNoFilesAnalyzed_Succeeds`.

**NtiaCompliance**: the unit accepts an SPDX document that satisfies NTIA minimum elements. This
scenario is tested by `Validate_Run_NtiaValidDocument_Succeeds` and
`Validate_DoValidate_NtiaMinimumValid_ReturnsNoErrors`.

**NtiaFailureReporting**: the unit reports NTIA validation errors for a non-compliant SPDX document.
This scenario is tested by `Validate_Run_NtiaInvalidDocument_ReportsNtiaErrors`.

**MissingSpdxInput**: the unit reports a YAML error when the `spdx` input is missing from a workflow
step. This scenario is tested by `Validate_Run_MissingSpdxInput_ThrowsYamlException`.

**ValidYamlWorkflow**: the unit accepts a conformant SPDX document when invoked from a YAML workflow
step. This scenario is tested by `Validate_Run_ValidYamlWorkflow_Succeeds`.

**NtiaYamlInputCaseInsensitive**: the unit treats the YAML `ntia` input as case-insensitive, so
`ntia: True` enables NTIA checking. This scenario is tested by
`Validate_Run_NtiaYamlInputCaseInsensitive_Succeeds`.

**DoValidateDirectInvocation**: DoValidate is callable directly without CLI or workflow dispatch,
enabling reuse from the self-test subsystem and other callers. This scenario is tested by
`Validate_DoValidate_ValidSpdxFile_ReturnsNoErrors` and
`Validate_DoValidate_NtiaMinimumValid_ReturnsNoErrors`.
