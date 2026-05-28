### SpdxHelpers

#### Verification Approach

`SpdxHelpers` is verified by direct unit tests in
`test/DemaConsulting.SpdxTool.Tests/Spdx/SpdxHelpersTests.cs` and indirectly through command
tests. Every command that loads or saves an SPDX JSON file exercises
SpdxHelpers.LoadJsonDocument and SpdxHelpers.SaveJsonDocument, providing additional evidence
of correct integration behavior.

#### Test Environment

Tests use in-memory SPDX JSON content and temporary files in the standard xUnit v3
environment. No external service is required.

#### Acceptance Criteria

Verification is acceptable when SPDX JSON files are loaded and saved correctly, missing files
are rejected with a usage error, and the tool creator entry is stamped on every saved
document.

#### Test Scenarios

**DocumentLoading**: SpdxHelpers loads a valid SPDX JSON file and returns a populated
SpdxDocument. This scenario is exercised by
`SpdxHelpers_LoadJsonDocument_ValidFile_ReturnsDocument`.

**MissingFileRejection**: SpdxHelpers throws CommandUsageException when the specified file
does not exist. This scenario is exercised by
`SpdxHelpers_LoadJsonDocument_MissingFile_ThrowsCommandUsageException`.

**CreatorStamping**: SpdxHelpers stamps the tool creator entry in the document's creation
information when saving. This scenario is exercised by
`SpdxHelpers_SaveJsonDocument_ValidDocument_StampsCreator`.
