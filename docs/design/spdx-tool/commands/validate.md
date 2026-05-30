### Validate

#### Purpose

Validate loads an SPDX document and checks it for specification compliance issues. Optionally it
also checks for NTIA minimum-elements compliance. Validation issues are written as warnings to the
context, and if any issues are found the command throws a CommandErrorException. It is available
from both the CLI and workflow YAML files.

#### Data Model

Validate holds no mutable state; it is a stateless singleton.

**Instance**: `Validate` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for Validate.

#### Key Methods

**Run(Context, string[])**: Parses spdxFile from CLI arguments. Detects the "ntia" flag (case-sensitive
exact match of the literal string "ntia") by scanning all arguments after args[0] — the flag may
appear at any position after the SPDX file path. Calls DoValidate.

- *Parameters*: `Context context` — execution context; `string[] args` — [spdxFile, ...optional
  flags including "ntia" at any position].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 1.
- *Post-conditions*: The document is validated; issues are reported via context.WriteWarning.

**Run(Context, YamlMappingNode, Dictionary<string, string>)**: Reads spdx and ntia inputs from the YAML step node
and calls DoValidate. The ntia input is evaluated case-insensitively via ToLowerInvariant(), so
"true", "True", and "TRUE" all enable NTIA checking.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: spdx input is required.
- *Post-conditions*: The document is validated; issues reported.

**DoValidate(Context, string, bool)**: Loads the SPDX document, calls doc.Validate to collect
issues, writes each issue as a warning to context, writes a blank line to separate the warning list
from the error summary, and throws CommandErrorException if any issues were found. Callable directly
by external callers (e.g., self-test) without going through the CLI or workflow dispatch paths.

- *Parameters*: `Context context` — execution context; `string spdxFile` — SPDX JSON file path;
  `bool ntia` — whether to apply NTIA minimum-elements checking.
- *Returns*: `void`
- *Preconditions*: spdxFile must exist and be a valid SPDX JSON document.
- *Post-conditions*: If no issues are found the method returns normally. If issues are found, each
  is written as a warning and CommandErrorException is thrown.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when no arguments are provided.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when the spdx input is
missing.

**CommandErrorException** — thrown by DoValidate when the loaded document contains one or more
validation issues; message indicates the count and file name.

#### Dependencies

- Command (abstract base class)
- SpdxDocument (DemaConsulting.SpdxModel — Validate method)
- SpdxHelpers (Spdx units)
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: validate
