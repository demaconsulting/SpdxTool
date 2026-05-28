### RelationshipDirection

#### Verification Approach

`RelationshipDirection` is verified indirectly through command tests in
`test/DemaConsulting.SpdxTool.Tests/`. The Diagram command calls GetDirection on each
relationship to determine rendering orientation, providing direct evidence of the enumeration
and extension method behavior.

#### Test Environment

Tests use local SPDX JSON fixtures in the standard xUnit v3 environment. No external service
is required.

#### Acceptance Criteria

Verification is acceptable when SPDX relationship types are correctly mapped to their
traversal directions and when the diagram output reflects the correct parent/child
orientation.

#### Test Scenarios

**DirectionEnumeration**: RelationshipDirection enumerates the three traversal directions
(Parent, Child, Sibling) and GetDirection maps SpdxRelationshipType values to their
directions. This scenario is exercised by `Diagram_ValidSpdxFile_GeneratesDiagram`.
