### CopyPackage

#### Purpose

CopyPackage copies a package (and optionally its child packages and associated files) from one SPDX
JSON document to another. If the destination already contains a package with the same identity it is
enhanced rather than duplicated. It is available from both the CLI and from workflow YAML files.

#### Data Model

N/A — CopyPackage is a stateless singleton.

**Instance**: `CopyPackage` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record advertising name, summary, usage details, and the
  singleton instance.

#### Key Methods

**Run(Context, string[])**: Parses from, to, package, and optional recursive/files flags from
positional CLI arguments and calls CopyPackageBetweenSpdxFiles.

- *Parameters*: `Context context` — execution context; `string[] args` — [fromFile, toFile,
  packageId, optional: "recursive", "files"].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 3. packageId must not be empty or "SPDXRef-DOCUMENT".
- *Post-conditions*: The package (and optionally children) are copied to the destination file.

**Run(Context, YamlMappingNode, Dictionary)**: Parses from, to, package, recursive, files, and
relationships inputs from the YAML step node and calls CopyPackageBetweenSpdxFiles.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: from, to, and package inputs are required.
- *Post-conditions*: The package is copied with any specified relationships added.

**CopyPackageBetweenSpdxFiles(string, string, string, SpdxRelationship[], bool, bool)**: Loads both
documents, calls Copy to copy the root package, calls AddRelationship.Add for root relationships,
optionally calls CopyChildren for recursive copy, and saves the destination document.

- *Parameters*: `string fromFile` — source SPDX file; `string toFile` — destination SPDX file;
  `string packageId` — ID of the package to copy; `SpdxRelationship[] relationships` — additional
  relationships to add to destination; `bool recursive` — copy child packages recursively;
  `bool files` — copy package files.
- *Returns*: `void`
- *Preconditions*: Both files must exist. packageId must not be empty or "SPDXRef-DOCUMENT".
- *Post-conditions*: toFile is updated in place.

**Copy(SpdxDocument, SpdxDocument, string, bool)**: Copies or enhances a single package in memory.
For a new copy (no same-identity package in toDoc), `FilesAnalyzed` is reset to false and `HasFiles`
is cleared because the destination does not carry the source file entries unless the files flag is
set. For an enhancement (same-identity package found), the existing package is enhanced and renamed
but `FilesAnalyzed` is not automatically reset — it remains as enhanced. In both paths, `FilesAnalyzed`
is set to true and files are copied only when all three conditions hold: (1) the `files` flag is
true, (2) `fromPackage.FilesAnalyzed == true`, and (3) `fromPackage.HasFiles.Length > 0`.

- *Parameters*: `SpdxDocument fromDoc` — source document; `SpdxDocument toDoc` — destination
  document; `string packageId` — package to copy; `bool files` — include analyzed files.
- *Returns*: `void`
- *Preconditions*: The package identified by packageId must exist in fromDoc.
- *Post-conditions*: toDoc contains the package; any required SpdxFile entries are added.

**CopyChildren(SpdxDocument, SpdxDocument, string, HashSet<string>, bool)**: Recursively copies child
packages (identified via RelationshipDirection on fromDoc relationships) and their relationships to
toDoc, guarding against infinite recursion with a visited set.

- *Parameters*: `SpdxDocument fromDoc` — source; `SpdxDocument toDoc` — destination;
  `string parentId` — parent package ID; `HashSet<string> copied` — already-copied IDs;
  `bool files` — include files.
- *Returns*: `void`
- *Preconditions*: None beyond document validity.
- *Post-conditions*: All reachable child packages and their relationships are present in toDoc.
- *Note*: After `Copy` and `SpdxRelationships.Add`, `copied.Add(childId)` both records the visit
  and gates recursion — `HashSet<T>.Add` returns `false` when the ID is already present, so
  recursion into a child's own children is skipped for already-visited IDs, guarding against
  infinite loops in cyclic graphs. `Copy` and `SpdxRelationships.Add` are both idempotent, so
  repeated calls for the same child in a diamond-shaped graph are safe and produce the same result.

**GetChild(SpdxRelationship, string)**: Returns the child package ID for a given relationship and
parent ID, using RelationshipDirection to determine the parent/child orientation, or null if the
relationship does not express a child of the given parent.

- *Parameters*: `SpdxRelationship relationship` — relationship to test; `string parentId` —
  candidate parent ID.
- *Returns*: `string?` — child package ID or null.
- *Preconditions*: None.
- *Post-conditions*: Pure function; no side effects.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) for fewer than three arguments or
unrecognized option tokens; thrown by CopyPackageBetweenSpdxFiles for an invalid packageId (empty
or "SPDXRef-DOCUMENT"); thrown by SpdxHelpers.LoadJsonDocument when either the source or destination
SPDX file does not exist on disk.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) for missing from, to, or
package inputs, or for non-boolean recursive/files values.

**CommandErrorException** — thrown by Copy when the source package is not found in fromDoc; also
thrown when a HasFiles entry references a missing SpdxFile in the source document.

#### Dependencies

- Command (abstract base class)
- SpdxDocument, SpdxPackage, SpdxFile, SpdxRelationship (DemaConsulting.SpdxModel)
- SpdxRelationships (DemaConsulting.SpdxModel.Transform)
- SpdxHelpers (Spdx units)
- AddRelationship (sibling command — Add static method for relationship insertion)
- RenameId (sibling command — Rename static method for package ID reconciliation on enhance)
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: copy-package
