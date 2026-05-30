### Diagram

#### Purpose

Diagram generates a Mermaid entity-relationship diagram file from the relationships defined in an
SPDX document. Only package-to-package relationships are rendered. Build, dev, and test tool
relationships are optionally filtered out. It is available from both the CLI and from workflow
YAML files.

#### Data Model

Diagram is a stateless singleton; all state is carried via method parameters.

**Instance**: `Diagram` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record advertising name, summary, usage details, and the
  singleton instance.

#### Key Methods

**Run(Context, string[])**: Parses spdxFile, mermaidFile, and an optional "tools" flag from CLI
arguments and calls GenerateDiagram.

- *Parameters*: `Context context` — execution context; `string[] args` — [spdxFile, mermaidFile,
  optional "tools"].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 2.
- *Post-conditions*: The mermaid file is written to disk.
- *Note*: The `Context` parameter is not used by this command because all output is written directly
  to the mermaid file via `File.WriteAllText`.

**Run(Context, YamlMappingNode, Dictionary)**: Parses spdx, mermaid, and tools inputs from the YAML
step node and calls GenerateDiagram.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: spdx and mermaid inputs are required.
- *Post-conditions*: The mermaid file is written.
- *Note*: The `Context` parameter is not used by this command because all output is written directly
  to the mermaid file via `File.WriteAllText`.

**GenerateDiagram(string, string, bool)**: Loads the SPDX document, filters relationships to those
between two packages, optionally excludes BuildToolOf/DevToolOf/TestToolOf relationships, resolves
RelationshipDirection to determine parent/child orientation, and writes an erDiagram block to the
output file. Each line uses the format "Name / Version" ||--|| "Name / Version" : "TYPE". When a
package's `versionInfo` is absent (null), the string `"unspecified"` is used as the version
placeholder to avoid null-reference exceptions and to produce valid Mermaid output.

- *Parameters*: `string spdxFile` — SPDX JSON file path; `string mermaidFile` — output file path;
  `bool tools` — include build/dev/test tool relationships (default false).
- *Returns*: `void`
- *Preconditions*: spdxFile must be a valid SPDX JSON document.
- *Post-conditions*: mermaidFile is written with an erDiagram block containing one line per
  qualifying relationship.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when fewer than two arguments are
provided or an unrecognized option token is encountered.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when spdx or mermaid inputs
are missing, or when the tools value is not a valid boolean.

**InvalidDataException** — defensive dead code in GenerateDiagram: the `switch` expression
on `RelationshipDirection` includes a `_ => throw new InvalidDataException()` arm, but
`RelationshipDirection.GetDirection()` always returns `Parent`, `Child`, or `Sibling`
(unmapped relationship types default to `Sibling`), making this arm permanently unreachable.
The arm is retained as a defensive guard against future changes to the direction enum.

**FileNotFoundException** — propagated from GenerateDiagram when the spdxFile path does not refer to
an existing file on disk.

**IOException** — propagated from GenerateDiagram when spdxFile cannot be read or mermaidFile cannot
be written.

**JsonException** — propagated from GenerateDiagram when spdxFile is not valid JSON.

#### Dependencies

- Command (abstract base class)
- SpdxDocument, SpdxPackage, SpdxRelationship, SpdxRelationshipType (DemaConsulting.SpdxModel)
- RelationshipDirection (Spdx units — GetDirection extension method)
- SpdxHelpers (Spdx units)
- System.Text.StringBuilder
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: diagram
