### RelationshipDirection

#### Verification Approach

`RelationshipDirection` is verified by direct unit tests in
`test/DemaConsulting.SpdxTool.Tests/Spdx/RelationshipDirectionTests.cs` and indirectly
through command tests. The Diagram command calls GetDirection on each relationship to
determine rendering orientation, providing additional integration evidence.

#### Test Environment

Tests use SpdxRelationshipType enum values directly in the standard xUnit v3 environment.
No external service or fixture files are required.

#### Acceptance Criteria

Verification is acceptable when SPDX relationship types are correctly mapped to their
traversal directions, the three direction values (Parent, Child, Sibling) are reachable, and
unmapped relationship types default to Sibling.

#### Test Scenarios

**ParentDirection**: A relationship type that expresses ownership (Describes) maps to
Parent direction. This scenario is exercised by
`RelationshipDirectionExtensions_GetDirection_DescribesRelationship_ReturnsParent`.

**ChildDirection**: A relationship type that expresses membership (DescribedBy) maps to
Child direction. This scenario is exercised by
`RelationshipDirectionExtensions_GetDirection_DescribedByRelationship_ReturnsChild`.

**SiblingDirection**: A relationship type with symmetric or neutral direction
(DependencyManifestOf) maps to Sibling direction. This scenario is exercised by
`RelationshipDirectionExtensions_GetDirection_DependencyManifestOfRelationship_ReturnsSibling`.

**UnmappedDefaultsToSibling**: An unmapped SpdxRelationshipType (Other) defaults to Sibling
direction. This scenario is exercised by
`RelationshipDirectionExtensions_GetDirection_UnmappedRelationshipType_ReturnsSibling`.
