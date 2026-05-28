### Hash

#### Verification Approach

`Hash` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/HashTests.cs`. The suite covers argument validation,
missing-file behavior, SHA-256 generation, verification success, and verification failure handling.

#### Test Environment

The tests run in the standard xUnit v3 environment with local files and SPDX fixtures. No external
service is required.

#### Acceptance Criteria

Verification is acceptable when the unit reports input errors correctly, writes generated hash
information to the SPDX package, and reliably distinguishes valid and invalid verification results.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when required hash arguments are omitted. This
scenario is tested by `Hash_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the file to hash does not exist. This scenario
is tested by `Hash_MissingFile_ReportsError`.

**GenerateHash**: the unit generates a SHA-256 hash and updates the target package metadata. This
scenario is tested by `Hash_GenerateOperation_UpdatesPackageHash`.

**DetectInvalidHash**: the unit reports failure when a supplied hash does not match the file
contents. This scenario is tested by `Hash_VerifyOperation_FailsForInvalidHash`.

**VerifyValidHash**: the unit accepts a matching hash during verification. This scenario is tested
by `Hash_VerifyOperation_SucceedsForValidHash`.

**MissingHashFile**: the unit reports an error when the sidecar hash file does not exist during verification. This scenario is tested by `Hash_VerifyMissingFile_ReportsError`.

**UnsupportedAlgorithm**: the unit reports a usage error when an algorithm other than SHA-256 is requested. This scenario is tested by `Hash_UnsupportedAlgorithm_ReportsError`.

**InvalidOperation**: the unit reports a usage error when an unrecognized operation is requested. This scenario is tested by `Hash_InvalidOperation_ReportsError`.
