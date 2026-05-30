### AddPackage

#### Purpose

AddPackage adds a new package to an existing SPDX JSON document or enhances (merges) an existing
package that shares the same identity, then optionally adds relationships between that package and
other elements in the document. The command is available in workflow mode only; direct CLI
invocation is rejected.

#### Data Model

AddPackage carries no instance state; all state is method-local. The following static fields serve
as registry entries:

**Instance**: `AddPackage` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record advertising name, summary, usage details, and the
  singleton instance to CommandsRegistry.

#### Key Methods

**Run(Context, string[])**: Rejects CLI invocation with a usage error.

- *Parameters*: `Context context` — execution context; `string[] args` — CLI arguments (unused).
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Throws CommandUsageException unconditionally.

**Run(Context, YamlMappingNode, Dictionary<string, string>)**: Parses workflow inputs, builds the package and
relationships, and delegates to AddPackageToSpdxFile.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — current workflow variable map.
- *Returns*: `void`
- *Preconditions*: step must contain an inputs map with spdx and package keys.
- *Post-conditions*: The named SPDX file is updated with the new or enhanced package and any
  specified relationships.

**ParsePackage(string, YamlMappingNode, Dictionary<string, string>)**: Constructs an SpdxPackage from a YAML
mapping node, appending optional purl and cpe23 entries as SpdxExternalReference instances.

- *Parameters*: `string command` — command name for error messages; `YamlMappingNode packageMap`
  — YAML node containing package fields; `Dictionary<string, string> variables` — variable map.
- *Returns*: `SpdxPackage`
- *Preconditions*: packageMap must contain id, name, and download keys. id must not be empty or
  "SPDXRef-DOCUMENT".
- *Post-conditions*: Returns a fully constructed SpdxPackage with optional external references.
- *Defaults*: When `copyright` is absent, `CopyrightText` defaults to `NOASSERTION`. The `license`
  input is mapped to both `ConcludedLicense` and `DeclaredLicense`; both default to `NOASSERTION`
  when `license` is absent.

**AddPackageToSpdxFile(string, SpdxPackage, SpdxRelationship[])**: Loads the SPDX document, calls
Add and AddRelationship.Add, and saves the document.

- *Parameters*: `string spdxFile` — path to the SPDX JSON file; `SpdxPackage package` — package to
  add or merge; `SpdxRelationship[] relationships` — relationships to add.
- *Returns*: `void`
- *Preconditions*: spdxFile must exist and be a valid SPDX JSON document.
- *Post-conditions*: The file is updated in place with the package and relationships applied.

**Add(SpdxDocument, SpdxPackage)**: Adds or enhances a package in memory. If an existing package
with the same identity (by SpdxPackage.Same equality) is found, it is enhanced and renamed;
otherwise a deep copy of the package is appended.

- *Parameters*: `SpdxDocument doc` — in-memory SPDX document; `SpdxPackage package` — package to
  add or merge.
- *Returns*: `void`
- *Preconditions*: doc must not be null.
- *Post-conditions*: doc.Packages contains the package; if an existing same-identity package was found
  its ID is updated to match the supplied package ID via RenameId.Rename. If the existing package's
  ID already matches `package.Id`, the rename is a no-op; the enhance still runs.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) unconditionally (workflow-only command);
also thrown by ParsePackage when the package ID is empty or equals "SPDXRef-DOCUMENT".

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when the spdx or package
inputs are missing; thrown by ParsePackage when the id, name, or download fields are absent from
the packageMap. When the `inputs:` block is entirely absent from a workflow step, `GetMapMap`
returns null; the subsequent `GetMapString` call on that null map also returns null, causing the
`?? throw` guard to throw a YamlException with the expected message.

**CommandErrorException** — thrown by AddPackageToSpdxFile when the relationships cannot be applied
to the document (propagated from AddRelationship.Add).

#### Dependencies

- Command (abstract base class providing YAML helper methods and Expand)
- SpdxDocument, SpdxPackage, SpdxExternalReference, SpdxReferenceCategory (DemaConsulting.SpdxModel)
- SpdxHelpers (Spdx units — LoadJsonDocument, SaveJsonDocument)
- AddRelationship (sibling command — Parse and Add static methods)
- RenameId (sibling command — Rename static method for package ID update on enhance)
- YamlDotNet (YamlMappingNode, YamlSequenceNode, YamlException)

#### Callers

- CommandsRegistry — holds the CommandEntry.Instance reference and routes workflow steps to this
  command
- RunWorkflow — dispatches this command when a workflow step specifies command: add-package
