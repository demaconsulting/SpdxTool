## Spdx

### Verification Approach

The Spdx subsystem is verified with direct unit tests in
`test/DemaConsulting.SpdxTool.Tests/Spdx/`. The tests exercise the relationship direction
extension methods and the SPDX JSON document load and save helpers in isolation. Every command
that reads or writes an SPDX JSON file additionally exercises `SpdxHelpers` as integration
evidence.

### Test Environment

N/A - both units are verified in the standard xUnit v3 environment with no special setup beyond
the test runner because they are pure in-process helpers operating on in-memory SPDX content or
temporary files.

### Acceptance Criteria

Verification is acceptable when SPDX relationship types are correctly mapped to their traversal
directions, SPDX JSON files are loaded and saved without data loss, missing files are rejected
with a usage error, and the tool creator entry is stamped on every saved document.

### Test Scenarios

**RelationshipDirectionMapping**: SPDX relationship types are correctly classified as Parent,
Child, or Sibling direction. This scenario is exercised by
`RelationshipDirectionExtensions_GetDirection_DescribesRelationship_ReturnsParent`,
`RelationshipDirectionExtensions_GetDirection_DescribedByRelationship_ReturnsChild`,
`RelationshipDirectionExtensions_GetDirection_DependencyManifestOfRelationship_ReturnsSibling`,
and `RelationshipDirectionExtensions_GetDirection_UnmappedRelationshipType_ReturnsSibling`.

**DocumentLoadAndSave**: SPDX JSON files are loaded and saved correctly, with creator stamping
applied on save. This scenario is exercised by
`SpdxHelpers_LoadJsonDocument_ValidFile_ReturnsDocument`,
`SpdxHelpers_LoadJsonDocument_MissingFile_ThrowsCommandUsageException`, and
`SpdxHelpers_SaveJsonDocument_ValidDocument_StampsCreator`.
