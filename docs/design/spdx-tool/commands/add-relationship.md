### AddRelationship

#### Purpose

AddRelationship adds one or more relationships between SPDX elements in a document. It is available
from both the CLI and from workflow YAML files. It also exposes static helper methods (Parse and Add)
that are used by AddPackage and CopyPackage when they need to attach relationships as part of their
own operations.

#### Data Model

N/A — AddRelationship is a stateless singleton; all state is carried via method parameters.

**Instance**: `AddRelationship` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record advertising name, summary, usage details, and the
  singleton instance to CommandsRegistry.

#### Key Methods

**Run(Context, string[])**: Parses four or five positional CLI arguments and calls
Add(string, SpdxRelationship[]).

- *Parameters*: `Context context` — execution context; `string[] args` — [spdxFile, id, type,
  element, optional comment].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 4.
- *Post-conditions*: One relationship is added to the specified SPDX file.
- *Note*: CLI invocation always adds without replacing (replace is fixed to `false` when calling `Add`).

**Run(Context, YamlMappingNode, Dictionary)**: Parses spdx, id, replace, and relationships inputs
from the step node and calls Add(string, SpdxRelationship[], bool).

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — current workflow variable map.
- *Returns*: `void`
- *Preconditions*: step inputs must contain spdx, id, and relationships keys.
- *Post-conditions*: The specified relationships are added (or replace existing relationships) in the
  SPDX file.
- *Note*: The workflow `replace` input defaults to `true` when omitted.

**Add(string, SpdxRelationship[], bool)**: Loads the SPDX document, delegates to
Add(SpdxDocument, SpdxRelationship[], bool), and saves the document.

- *Parameters*: `string spdxFile` — SPDX document file path; `SpdxRelationship[] relationships`
  — relationships to add; `bool replace` — whether to replace existing matching relationships.
- *Returns*: `void`
- *Preconditions*: spdxFile must exist and be a valid SPDX JSON document.
- *Post-conditions*: The file is updated in place.

**Add(SpdxDocument, SpdxRelationship[], bool)**: Delegates to SpdxRelationships.Add, wrapping any
exception in a CommandErrorException.

- *Parameters*: `SpdxDocument doc` — in-memory SPDX document; `SpdxRelationship[] relationships`
  — relationships to add; `bool replace` — replace flag.
- *Returns*: `void`
- *Preconditions*: doc must not be null.
- *Post-conditions*: doc.Relationships contains the new relationships.

**Parse(string, string, YamlSequenceNode?, Dictionary)**: Parses a YAML sequence of relationship
mappings into an array of SpdxRelationship instances. Returns an empty array when the sequence
node is null.

- *Parameters*: `string command` — command name for error messages; `string packageId` — the source
  element ID; `YamlSequenceNode? relationships` — optional YAML sequence;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `SpdxRelationship[]`
- *Preconditions*: Each child node must be a YamlMappingNode containing type and element keys.
- *Post-conditions*: Returns one SpdxRelationship per sequence entry.

**Parse(string, string, YamlMappingNode, Dictionary)**: Parses a single relationship mapping node
into an SpdxRelationship.

- *Parameters*: `string command` — command name; `string packageId` — source element ID;
  `YamlMappingNode relationshipMap` — YAML map with type, element, and optional comment keys;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `SpdxRelationship`
- *Preconditions*: relationshipMap must contain type and element keys.
- *Post-conditions*: Returns an SpdxRelationship with Id set to packageId.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when fewer than four arguments are
provided.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when spdx, id, or
relationships inputs are missing or the replace value is not a valid boolean; thrown by the Parse
methods when a relationship node is not a mapping or is missing type or element keys.

**System.IO.IOException** — propagated by Add(string, SpdxRelationship[], bool) when the SPDX file
cannot be read or written (for example, access denied or other I/O failure).

**CommandErrorException** — thrown by Add(SpdxDocument, SpdxRelationship[], bool) when
SpdxRelationships.Add raises an exception (for example duplicate relationships when replace is false).

#### Dependencies

- Command (abstract base class)
- SpdxDocument, SpdxRelationship, SpdxRelationshipTypeExtensions (DemaConsulting.SpdxModel)
- SpdxRelationships (DemaConsulting.SpdxModel.Transform)
- SpdxHelpers (Spdx units)
- YamlDotNet (YamlMappingNode, YamlSequenceNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- AddPackage — calls Parse to parse relationships from the add-package step, and calls
  Add(SpdxDocument, SpdxRelationship[], bool replace = false) to persist them
- CopyPackage — calls Parse to parse relationships, and calls Add(SpdxDocument, SpdxRelationship[], bool replace = false)
  to add root relationships to the destination document
