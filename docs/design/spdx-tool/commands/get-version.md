### GetVersion

#### Purpose

GetVersion retrieves the version string of a package from an SPDX document that matches the
supplied search criteria. In CLI mode it writes the version to the console; in workflow mode it
stores the version in the named output variable. It delegates package lookup to FindPackage. It
is available from both the CLI and workflow YAML files.

#### Data Model

**Instance**: `GetVersion` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for GetVersion.

#### Key Methods

**Run(Context, string[])**: Parses spdxFile and key=value criteria from CLI arguments, finds the
matching package via FindPackage.FindPackageByCriteria, and writes the version to the console.

- *Parameters*: `Context context` — execution context; `string[] args` — [spdxFile, criteria...].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 2.
- *Post-conditions*: The version string (or empty string if null) is written to context.

**Run(Context, YamlMappingNode, Dictionary)**: Parses spdx, criteria, and output inputs from the
YAML step node, finds the matching package, and stores the version in variables[output].

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: spdx and output inputs are required.
- *Post-conditions*: variables[output] is set to the version string or empty string.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when fewer than two arguments are
provided.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when the spdx or output
inputs are missing.

**CommandErrorException** — propagated from FindPackage.FindPackageByCriteria when no package
matches or multiple packages match.

#### Dependencies

- Command (abstract base class)
- FindPackage (sibling command — ParseCriteria and FindPackageByCriteria static methods)
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: get-version
