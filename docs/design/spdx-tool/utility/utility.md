## Utility

### Overview

The Utility subsystem is a logical grouping of stateless cross-cutting helper units shared
across the DemaConsulting.SpdxTool system. It contains two unit groups: the Utility unit group
(namespace DemaConsulting.SpdxTool.Utility) and the Spdx units
(namespace DemaConsulting.SpdxTool.Spdx).

The Utility unit group provides file-system safety and pattern-matching support:

- PathHelpers - safe path combining that rejects path-traversal sequences.
- Wildcard - glob-style pattern matching via compiled regular expressions.

The Spdx units provide SPDX-domain support for the Commands subsystem:

- SpdxHelpers - centralized SPDX JSON document loading and saving.
- RelationshipDirection and RelationshipDirectionExtensions - enumeration and static mapping of
  SPDX relationship types to traversal directions.

Neither unit group depends on the other. Both are consumed by the Commands subsystem and by the
SelfTest subsystem. The subsystem has no internal dispatcher; units are invoked directly by their
consumers through static method calls.

### Interfaces

**PathHelpers.SafePathCombine**: Combines a base path with a relative path after validating that
the relative path contains no ".." components and is not rooted. A secondary check using
Path.GetFullPath confirms the resolved path remains under the base directory.

- *Type*: Static method (internal).
- *Role*: Provider.
- *Contract*: Returns the combined path when the relative path is safe; throws ArgumentException
  when the path contains ".." sequences, is rooted, or resolves outside the base directory.
- *Constraints*: Used exclusively for NuGet-embedded workflow paths in the RunWorkflow command;
  not a general-purpose path utility.

**Wildcard.IsMatch**: Determines whether an input string matches a wildcard pattern that may
contain * (any sequence of characters) and ? (any single character). Matching is case-insensitive
and uses a compiled regular expression derived from the wildcard pattern, with a 100-millisecond
evaluation timeout to guard against catastrophic backtracking.

- *Type*: Static method (public).
- *Role*: Provider.
- *Contract*: Returns true when the input matches the entire pattern from start to end; returns
  false otherwise.
- *Constraints*: Matching is always case-insensitive; the regex evaluation timeout is fixed at
  100 milliseconds.

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

The four units collaborate through direct static method calls with no shared state or internal
coupling between unit groups.

PathHelpers is invoked by the RunWorkflow command when resolving workflow file paths within a
NuGet package cache directory. It ensures that no step in an embedded workflow can reference a
file outside the package directory by applying both an upfront string check (rejects ".." and
rooted paths) and a defense-in-depth check using Path.GetFullPath to confirm the resolved path
stays under the base directory.

Wildcard.IsMatch is invoked by the FindPackage command and other commands that accept glob-style
name filters to match against SPDX package names or element IDs. The conversion of a wildcard
pattern to a regular expression is performed inline on each call; patterns are not cached.

SpdxHelpers acts as the single point of file I/O for SPDX documents across the Commands subsystem.
Every command that reads an SPDX document calls SpdxHelpers.LoadJsonDocument, and every command
that writes one calls SpdxHelpers.SaveJsonDocument. This centralizes format handling and ensures
the tool creator entry is consistently stamped on every written document. SpdxHelpers depends on
DemaConsulting.SpdxModel for the SpdxDocument type and the JSON serialization routines, and reads
Program.Version to construct the tool creator stamp.

RelationshipDirection and GetDirection decouple the Commands subsystem from the raw
SpdxRelationshipType enumeration defined by DemaConsulting.SpdxModel. Commands that traverse SPDX
relationships (such as Query and FindPackage) call GetDirection to convert a relationship type into
a directional intent and filter traversal results accordingly, without needing to enumerate every
SpdxRelationshipType case inline.
