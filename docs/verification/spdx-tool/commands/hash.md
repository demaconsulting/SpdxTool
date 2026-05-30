### Hash

#### Verification Approach

`Hash` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/HashTests.cs`. The suite covers argument validation,
missing-file behavior, SHA-256 generation, verification success, and verification failure handling.

#### Test Environment

The tests run in the standard xUnit v3 environment with local files and SPDX fixtures. No external
service is required.

#### Acceptance Criteria

Verification is acceptable when the unit reports input errors correctly, writes the generated hash
to a sidecar file, and reliably distinguishes valid and invalid verification results.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when required hash arguments are omitted. This
scenario is tested by `Hash_Run_MissingArguments_ReportsError`.

**ExcessArguments**: the unit reports a usage error when more than three arguments are provided. This
scenario is tested by `Hash_Run_ExcessArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the file to hash does not exist. This scenario
is tested by `Hash_Run_MissingFile_ReportsError`.

**GenerateHash**: the unit generates a SHA-256 hash and writes it to a sidecar file. This scenario
is tested by `Hash_Run_GenerateOperation_WritesSidecarFile`.

**DetectInvalidHash**: the unit reports failure when a supplied hash does not match the file
contents. This scenario is tested by `Hash_Run_VerifyOperation_FailsForInvalidHash`.

**VerifyValidHash**: the unit accepts a matching hash during verification. This scenario is tested
by `Hash_Run_VerifyOperation_SucceedsForValidHash`.

**VerifyUppercaseDigest**: the unit accepts a matching hash stored in uppercase in the sidecar
file, confirming case-insensitive digest comparison. This scenario is tested by
`Hash_Run_VerifySha256_UppercaseDigest_Succeeds`.

**MissingHashFile**: the unit reports an error when the sidecar hash file does not exist during
verification. This scenario is tested by `Hash_Run_VerifyMissingSidecarFile_ReportsError`.

**MissingTargetFile**: the unit reports an error when the sidecar file exists but the target file
does not exist during verification. This scenario is tested by
`Hash_Run_VerifyTargetMissing_ReportsError`.

**UnsupportedAlgorithm**: the unit reports a usage error when an algorithm other than SHA-256 is
requested. This scenario is tested by `Hash_Run_UnsupportedAlgorithm_ReportsError`.

**InvalidOperation**: the unit reports a usage error when an unrecognized operation is requested.
This scenario is tested by `Hash_Run_InvalidOperation_ReportsError`.

**WorkflowGenerateHash**: the unit generates a SHA-256 hash sidecar file when invoked via a
workflow YAML step. This scenario is tested by `Hash_Run_InWorkflow_GeneratesHash`.

**WorkflowMissingOperation**: the unit reports an error when the workflow step omits the
required 'operation' input. This scenario is tested by
`Hash_Run_InWorkflow_MissingOperation_ReportsError`.

**WorkflowMissingAlgorithm**: the unit reports an error when the workflow step omits the
required 'algorithm' input. This scenario is tested by
`Hash_Run_InWorkflow_MissingAlgorithm_ReportsError`.

**WorkflowMissingFile**: the unit reports an error when the workflow step omits the required
'file' input. This scenario is tested by `Hash_Run_InWorkflow_MissingFile_ReportsError`.
