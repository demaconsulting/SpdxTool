### ToMarkdown

#### Purpose

ToMarkdown generates a Markdown summary of an SPDX document and writes it to an output file. The
summary includes document metadata, root packages, non-root packages, and tool packages, each in
its own section with a configurable heading depth and title. It is available from both the CLI and
workflow YAML files.

#### Data Model

N/A — ToMarkdown is a stateless singleton.

**Instance**: `ToMarkdown` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for ToMarkdown.

#### Key Methods

**Run(Context, string[])**: Parses spdxFile, markdownFile, optional title (default "SPDX
Document"), and optional depth (default 2) from CLI arguments and calls
GenerateSummaryMarkdown.

- *Parameters*: `Context context` — execution context; `string[] args` — [spdxFile, markdownFile,
  optional title, optional depth].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 2. depth must be a positive integer if provided.
  title must not be whitespace.
- *Post-conditions*: The markdown file is written.

**Run(Context, YamlMappingNode, Dictionary)**: Reads spdx, markdown, title, and depth inputs from
the YAML step node and calls GenerateSummaryMarkdown.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: spdx and markdown inputs are required. depth must parse to a positive integer.
- *Post-conditions*: The markdown file is written.

**GenerateSummaryMarkdown(string, string, string, int)**: Loads the SPDX document, builds a
Markdown string using StringBuilder, and writes it to the output file. The document section lists
metadata in a two-column table. Packages are classified into root packages (from
SpdxDocument.GetRootPackages), tool packages (having BuildToolOf, DevToolOf, or TestToolOf
relationships), and remaining packages. The title heading is rendered at `depth` hash marks.
Each group (Root Packages, Packages, Tools) is rendered as a three-column table under a
sub-section heading at `depth+1` hash marks.

- *Parameters*: `string spdxFile` — SPDX JSON file path; `string markdownFile` — output file path;
  `string title` — heading title (default "SPDX Document"); `int depth` — heading level (default 2).
- *Returns*: `void`
- *Preconditions*: spdxFile must be a valid SPDX JSON document. depth must be at least 1.
- *Post-conditions*: markdownFile is written.

**License(SpdxPackage)**: Determines the display license for a package. Returns the concluded
license if it is non-empty and not "NOASSERTION". Falls back to the declared license under the
same condition. Returns "NOASSERTION" when neither field provides a usable value. Concluded
license takes priority because it represents the authoritative determination after analysis,
while declared license is the upstream assertion before review.

- *Parameters*: `SpdxPackage package` — the package whose license to resolve.
- *Returns*: `string` — the concluded license, declared license, or "NOASSERTION".
- *Preconditions*: None.
- *Post-conditions*: None — the method is read-only.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when fewer than two arguments are
provided, when the title is whitespace, or when depth is not a positive integer.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when spdx or markdown
inputs are missing, when the title is whitespace, or when the depth value is not a positive
integer.

**FileNotFoundException** — propagated from SpdxHelpers.LoadJsonDocument when the specified SPDX
input file does not exist on disk.

#### Dependencies

- Command (abstract base class)
- SpdxDocument, SpdxPackage, SpdxRelationshipType (DemaConsulting.SpdxModel)
- SpdxHelpers (Spdx units)
- System.Text.StringBuilder
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: to-markdown
