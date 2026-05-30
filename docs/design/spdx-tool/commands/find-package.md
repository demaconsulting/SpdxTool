### FindPackage

#### Purpose

FindPackage searches an SPDX document for a package that matches a set of key-value criteria (id,
name, version, filename, download). Wildcard patterns are supported for each criterion. In CLI mode
it prints the matching package ID to the console; in workflow mode it stores the ID in the named
output variable. It is available from both the CLI and workflow YAML files.

#### Data Model

FindPackage carries no mutable instance state; all fields are static.

**Command** (`private const string`): The command name string `"find-package"` used for registration
with CommandsRegistry.

**Instance** (`public static readonly FindPackage`): The singleton instance registered with
CommandsRegistry.

**Entry** (`public static readonly CommandEntry`): The CommandEntry record that exposes the command
name, usage synopsis, help text, and singleton instance to the command registry.

#### Key Methods

**Run(Context, string[])**: Parses spdxFile and key=value criteria from CLI arguments, finds the
matching package, and writes its ID to the console.

- *Parameters*: `Context context` — execution context; `string[] args` — [spdxFile, criteria...].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 2. Each criterion must be in key=value format.
- *Post-conditions*: The matching package ID is written to context.

**Run(Context, YamlMappingNode, Dictionary)**: Parses output, spdx, and criteria inputs from the
YAML step node, finds the matching package, and stores its ID in variables[output].

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: output and spdx inputs are required; at least one criterion should be provided.
- *Post-conditions*: variables[output] is set to the matching package ID.

**ParseCriteria(IEnumerable, Dictionary)**: Splits each "key=value" string from args into a
criteria dictionary entry. Throws CommandUsageException if any entry does not contain "=", or if
the key part (the substring before "=") is empty. The split is performed on the first `=` only,
so field values containing `=` characters are preserved intact.

- *Parameters*: `IEnumerable<string> args` — criterion strings;
  `Dictionary<string, string> criteria` — dictionary to populate.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: criteria contains all parsed key-value pairs. When a key appears more than once
  the last occurrence silently overwrites earlier entries (last writer wins).

**ParseCriteria(YamlMappingNode?, Dictionary, Dictionary)**: Extracts the optional id, name,
version, filename, and download fields from a YAML inputs map into the criteria dictionary.

- *Parameters*: `YamlMappingNode? map` — YAML inputs map;
  `Dictionary<string, string> variables` — variable map;
  `Dictionary<string, string> criteria` — dictionary to populate.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: criteria contains any criteria fields present in the map.

**FindPackageByCriteria(string, IReadOnlyDictionary)**: Loads the SPDX document and returns the
unique package matching all criteria. Throws if zero or more than one package matches.

- *Parameters*: `string spdxFile` — SPDX JSON file path;
  `IReadOnlyDictionary<string, string> criteria` — search criteria.
- *Returns*: `SpdxPackage`
- *Preconditions*: spdxFile must exist.
- *Post-conditions*: Returns exactly one matching package. The document is loaded fresh from disk on
  every call; no caching is performed.

**IsPackageMatch(SpdxPackage, IReadOnlyDictionary)**: Tests a single package against all supplied
criteria using Wildcard.IsMatch for each field.

- *Parameters*: `SpdxPackage package` — package to test;
  `IReadOnlyDictionary<string, string> criteria` — criteria to match.
- *Returns*: `bool`
- *Preconditions*: None.
- *Post-conditions*: Pure function; no side effects.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when fewer than two arguments are
provided; thrown by ParseCriteria(IEnumerable, Dictionary) when a criterion string does not
contain "=", or when the key part (the substring before "=") is empty.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when the output or spdx
inputs are missing.

**CommandErrorException** — thrown by FindPackageByCriteria when no package matches or when multiple
packages match.

#### Dependencies

- Command (abstract base class)
- SpdxDocument, SpdxPackage (DemaConsulting.SpdxModel)
- SpdxHelpers (Spdx units)
- Wildcard (Utility subsystem — wildcard pattern matching)
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- GetVersion — calls ParseCriteria and FindPackageByCriteria to locate the target package
- RunWorkflow — dispatches this command when a workflow step specifies command: find-package
