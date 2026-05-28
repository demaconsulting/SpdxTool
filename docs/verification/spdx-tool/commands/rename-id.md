### RenameId

#### Verification Approach

`RenameId` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/RenameIdTests.cs`. The tests cover argument validation,
file validation, and full document updates when an SPDX identifier is renamed.

#### Test Environment

The tests use local SPDX JSON fixtures in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when missing inputs are rejected and successful execution updates all
affected references to the renamed SPDX identifier.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when required rename arguments are omitted.
This scenario is tested by `RenameId_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the input SPDX file does not exist. This
scenario is tested by `RenameId_MissingFile_ReportsError`.

**ReferenceWideRename**: the unit renames the target SPDX identifier across the full document. This
scenario is tested by `RenameId_ValidSpdxFile_RenamesId`.
