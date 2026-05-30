## Spdx

### Overview

The Spdx subsystem is a logical grouping of stateless SPDX-domain helper units shared across
the DemaConsulting.SpdxTool system (namespace DemaConsulting.SpdxTool.Spdx). It contains two
units:

- SpdxHelpers - centralized SPDX JSON document loading and saving.
- RelationshipDirection and RelationshipDirectionExtensions - enumeration and static mapping of
  SPDX relationship types to traversal directions.

The two units are independent of each other. Both are consumed by the Commands subsystem and by
the SelfTest subsystem. The subsystem has no internal dispatcher; units are invoked directly by
their consumers through static method calls.

### Interfaces

**SpdxHelpers.LoadJsonDocument**: Loads an SPDX 2.x document from a JSON file at the given path,
delegating deserialization to DemaConsulting.SpdxModel's Spdx2JsonDeserializer.

- *Type*: Static method (public).
- *Role*: Provider.
- *Contract*: Returns a populated SpdxDocument on success; throws CommandUsageException if the
  file does not exist.
- *Constraints*: Expects SPDX 2.x JSON format as defined by DemaConsulting.SpdxModel.

**SpdxHelpers.SaveJsonDocument**: Serializes an SpdxDocument to SPDX 2.x JSON and writes it to
the given file path. Before serializing, the method ensures the tool creator entry
("Tool: DemaConsulting.SpdxTool-{version}") is present in the document's creation information.

- *Type*: Static method (public).
- *Role*: Provider.
- *Contract*: Writes the serialized JSON to the specified file path; appends the tool creator
  entry if not already present in the document's creation information.
- *Constraints*: Output format is SPDX 2.x JSON as produced by DemaConsulting.SpdxModel's
  Spdx2JsonSerializer.

**RelationshipDirection**: Enumeration expressing the traversal direction of an SPDX relationship
query relative to the element under inspection. Values are Parent (the element is the origin of
the relationship), Child (the element is the target), and Sibling (the relationship is symmetric
or directionally neutral).

- *Type*: Public enum.
- *Role*: Provider (type definition).
- *Contract*: Consumed by query and find operations in the Commands subsystem to express traversal
  intent without coupling callers to raw SpdxRelationshipType values.
- *Constraints*: None.

**RelationshipDirectionExtensions.GetDirection**: Extension method on SpdxRelationshipType that
returns the corresponding RelationshipDirection by looking up the type in a static dictionary.
Relationship types not present in the map default to Sibling.

- *Type*: Extension method (public static).
- *Role*: Provider.
- *Contract*: Returns the RelationshipDirection for the given SpdxRelationshipType; returns
  Sibling for any type not present in the mapping table.
- *Constraints*: The mapping table covers all SPDX 2.x relationship types known at design time;
  new relationship types introduced in future SPDX versions will default to Sibling.

### Design

SpdxHelpers acts as the single point of file I/O for SPDX documents across the Commands subsystem.
Every command that reads an SPDX document calls SpdxHelpers.LoadJsonDocument, and every command
that writes one calls SpdxHelpers.SaveJsonDocument. This centralizes format handling and ensures
the tool creator entry is consistently stamped on every written document. SpdxHelpers depends on
DemaConsulting.SpdxModel for the SpdxDocument type and the JSON serialization routines, and reads
Program.Version to construct the tool creator stamp.

RelationshipDirection and GetDirection decouple the Commands subsystem from the raw
SpdxRelationshipType enumeration defined by DemaConsulting.SpdxModel. Commands that traverse SPDX
relationships (such as Diagram and CopyPackage) call GetDirection to convert a relationship type into
a directional intent and filter traversal results accordingly, without needing to enumerate every
SpdxRelationshipType case inline.
