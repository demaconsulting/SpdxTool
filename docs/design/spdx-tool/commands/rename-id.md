### RenameId

#### Purpose

RenameId renames an SPDX element ID throughout an SPDX document, updating all package IDs, file
IDs, snippet IDs, snippet `SnippetFromFile` references, relationship references, HasFiles arrays,
and the Describes array. It is available from both the CLI and workflow YAML files, and exposes a
static Rename method used internally by AddPackage and CopyPackage.

#### Data Model

**Instance**: `RenameId` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for RenameId.

#### Key Methods

**Run(Context, string[])**: Validates that exactly three arguments are provided and calls
Rename(string, string, string).

- *Parameters*: `Context context` — execution context; `string[] args` — [spdxFile, oldId, newId].
- *Returns*: `void`
- *Preconditions*: args.Length must be exactly 3.
- *Post-conditions*: All occurrences of oldId in the SPDX file are replaced with newId.

**Run(Context, YamlMappingNode, Dictionary)**: Reads spdx, new, and old inputs from the YAML step
node and calls Rename(string, string, string).

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: spdx, new, and old inputs are required (read in that order).
- *Post-conditions*: All occurrences of old are replaced with new in the SPDX file.

**Rename(string, string, string)**: Loads the SPDX document, calls
Rename(SpdxDocument, string, string), and saves the updated document.

- *Parameters*: `string spdxFile` — SPDX file path; `string oldId` — element ID to replace;
  `string newId` — replacement ID.
- *Returns*: `void`
- *Preconditions*: spdxFile must exist.
- *Post-conditions*: spdxFile is updated in place.

**Rename(SpdxDocument, string, string)**: Performs the in-memory rename across all element
collections. Skips the operation when oldId == newId. Validates that neither ID is empty or
"SPDXRef-DOCUMENT". Validates that newId is not already in use.

- *Parameters*: `SpdxDocument doc` — in-memory SPDX document; `string oldId` — old element ID;
  `string newId` — new element ID.
- *Returns*: `void`
- *Preconditions*: oldId and newId must not be empty or equal to "SPDXRef-DOCUMENT". newId must
  not already be used by any package, file, or snippet in doc.
- *Post-conditions*: All references to oldId in packages, files, snippets (including the
  SnippetFromFile field of each snippet and, by extension, the `reference` fields inside
  the snippet's start and end range pointers — which the serializer derives from SnippetFromFile),
  relationships, HasFiles arrays, and Describes are
  updated to newId. If oldId matches no element in the document, the method returns with no
  changes applied (silent no-op).

**UpdateId(string, string, string)**: Returns the new ID if `id == oldId`, otherwise returns `id`
unchanged. Used as the single consistent replacement primitive called by all collection-update
loops in Rename(SpdxDocument, string, string).

- *Parameters*: `string id` — current ID; `string oldId` — ID to match; `string newId` — replacement.
- *Returns*: `string` — newId when id matches oldId, otherwise id.
- *Visibility*: private static.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when the argument count is not exactly
3; thrown by Rename(SpdxDocument, string, string) when oldId or newId is empty or equals
"SPDXRef-DOCUMENT".

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when spdx, old, or new
inputs are missing.

**CommandErrorException** — thrown by Rename(SpdxDocument, string, string) when newId is already
used by another element in the document.

#### Dependencies

- Command (abstract base class)
- SpdxDocument, SpdxPackage, SpdxFile, SpdxSnippet, SpdxRelationship (DemaConsulting.SpdxModel)
- SpdxHelpers (Spdx units)
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- AddPackage — calls Rename(SpdxDocument, string, string) when enhancing an existing package to
  reconcile the package ID
- CopyPackage — calls Rename(SpdxDocument, string, string) when enhancing an existing package or
  file in the destination document
- RunWorkflow — dispatches this command when a workflow step specifies command: rename-id
