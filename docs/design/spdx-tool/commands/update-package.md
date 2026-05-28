### UpdatePackage

#### Purpose

UpdatePackage updates named metadata fields of an existing package in an SPDX JSON document. The
package is identified by its ID. Only the fields present in the inputs are updated; absent fields
leave the existing values unchanged. It is available in workflow mode only; direct CLI invocation
is rejected.

#### Data Model

N/A — UpdatePackage is a stateless singleton.

**Instance**: `UpdatePackage` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for UpdatePackage.

#### Key Methods

**Run(Context, string[])**: Rejects CLI invocation with a usage error.

- *Parameters*: `Context context` — execution context; `string[] args` — CLI arguments (unused).
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Throws CommandUsageException unconditionally.

**Run(Context, YamlMappingNode, Dictionary)**: Reads spdx and package inputs, extracts the package
ID, parses the update fields via ParseUpdates, and delegates to UpdatePackageInSpdxFile.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — workflow variable map.
- *Returns*: `void`
- *Preconditions*: spdx, package, and package.id inputs are required.
- *Post-conditions*: The named package in the SPDX file has its specified fields updated.

**UpdatePackageInSpdxFile(string, string, Dictionary)**: Loads the SPDX document, locates the
package by ID, applies each key-value pair in the updates dictionary to the corresponding package
field, and saves the document.

- *Parameters*: `string spdxFile` — SPDX file path; `string packageId` — ID of the package to
  update; `Dictionary<string, string> updates` — field-name to new-value map.
- *Returns*: `void`
- *Preconditions*: spdxFile must exist. A package with packageId must be present.
- *Post-conditions*: The package fields listed in updates are updated and the file is saved.
  Supported field names: name, download, version, filename, supplier, originator, homepage,
  copyright, summary, description, license (sets both ConcludedLicense and DeclaredLicense).

**ParseUpdates(YamlMappingNode?, Dictionary, Dictionary)**: Reads optional name, download,
version, filename, supplier, originator, homepage, copyright, summary, description, and license
fields from the YAML map and populates the updates dictionary.

- *Parameters*: `YamlMappingNode? map` — YAML package sub-map;
  `Dictionary<string, string> variables` — variable map;
  `Dictionary<string, string> updates` — output updates dictionary.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: updates contains only the fields that were present in the map.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) unconditionally (workflow-only
command).

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when the spdx, package, or
package.id inputs are missing.

**CommandErrorException** — thrown by UpdatePackageInSpdxFile when the package ID is not found in
the document; also thrown when an unrecognized field name is encountered in the updates dictionary.

#### Dependencies

- Command (abstract base class)
- SpdxDocument, SpdxPackage (DemaConsulting.SpdxModel)
- SpdxHelpers (Spdx units)
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — holds the CommandEntry.Instance reference and routes workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: update-package
