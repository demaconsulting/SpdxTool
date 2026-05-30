### RelationshipDirection

#### Purpose

RelationshipDirection provides an enumeration of the three abstract traversal directions
(Parent, Child, Sibling) for SPDX relationship types, and the RelationshipDirectionExtensions
class maps every recognized SpdxRelationshipType to its direction via the GetDirection
extension method. Commands that need to traverse or render SPDX relationships use this
abstraction rather than switching on individual relationship types.

#### Data Model

**RelationshipDirection** (enum): Three values — Parent (the source element is the parent),
Child (the source element is a child), and Sibling (the elements are at the same level).

**DirectionMap**: `Dictionary<SpdxRelationshipType, RelationshipDirection>` — private static
read-only dictionary mapping every recognized SpdxRelationshipType to its RelationshipDirection.
Initialized at type load; unrecognized types default to Sibling.

#### Key Methods

**GetDirection(this SpdxRelationshipType)**: Returns the RelationshipDirection for the given
relationship type using DirectionMap, defaulting to Sibling for unrecognized types.

- *Parameters*: `SpdxRelationshipType type` — the relationship type to look up.
- *Returns*: `RelationshipDirection`
- *Preconditions*: None.
- *Post-conditions*: Pure function; always returns a valid RelationshipDirection.

#### Error Handling

N/A — GetDirection never throws; unrecognized types return Sibling.

#### Dependencies

- SpdxRelationshipType (DemaConsulting.SpdxModel)

#### Callers

- Diagram — calls GetDirection on each relationship to determine rendering orientation
- CopyPackage — uses direction to identify dependent packages in recursive copy
