### RunWorkflow

#### Purpose

RunWorkflow executes a multi-step SPDX workflow defined in a YAML file, a URL, or a NuGet package.
It supports parameterized execution with named parameters, SHA-256 integrity verification of the
workflow source, and extraction of named output variables after execution. Workflows may be nested
(a workflow step may call run-workflow). It is available from both the CLI and workflow YAML files.

#### Data Model

RunWorkflow carries no mutable instance state; all fields are static constants or readonly singletons
initialized once at class load.

**Command** (`private const string`): The canonical command name `"run-workflow"` used for registration
and help text.

**Instance** (`public static readonly RunWorkflow`): The singleton instance registered with
CommandsRegistry. Created once via the private constructor.

**Entry** (`public static readonly CommandEntry`): The CommandEntry record that pairs the command name,
usage line, summary text, and extended help lines with the singleton Instance for dispatch.

#### Key Methods

**Run(Context, string[])**: Parses the workflow path or URL, optional key=value parameters, and the
--verbose flag from CLI arguments. Dispatches to RunUrl for HTTP/HTTPS paths (checked with explicit
scheme comparison) or RunFile for local file paths. Optionally prints the resulting output
variables. NuGet package sources are not supported from the CLI; only local file paths and HTTP/HTTPS
URLs are accepted. Integrity verification is only available via YAML workflow step inputs; the CLI
path always passes `null` for the integrity argument and therefore cannot perform hash checks.

- *Parameters*: `Context context` — execution context; `string[] args` — [workflowPathOrUrl,
  optional parameter=value pairs, optional --verbose].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 1.
- *Post-conditions*: All workflow steps are executed; outputs are optionally printed.

**Run(Context, YamlMappingNode, Dictionary)**: Reads file, url, nuget, integrity, parameters, and
outputs inputs. Resolves NuGet package paths when nuget is specified, then dispatches to
Run(Context, YamlMappingNode, string?, string?, string?, Dictionary). Maps declared output
variables back into the caller's variable dictionary.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — caller's variable map.
- *Returns*: `void`
- *Preconditions*: Exactly one of file or url must be non-null.
- *Post-conditions*: All steps are executed; requested outputs are stored in variables.

**public static Run(Context, YamlMappingNode, string?, string?, string?, Dictionary)**: Validates that file and
url are not both specified, then dispatches to RunFile or RunUrl. At least one of file or url must
be non-null.

- *Parameters*: `Context context`; `YamlMappingNode step` — for error reporting; `string? file`;
  `string? url`; `string? integrity`; `Dictionary<string, string> parameters`.
- *Returns*: `Dictionary<string, string>` — the resulting variable map (workflow outputs).
- *Preconditions*: Exactly one of file or url must be non-null.
- *Post-conditions*: Workflow is executed; output variables returned.

**RunFile(Context, string, string?, Dictionary)**: Reads the workflow YAML file from disk and calls
RunBytes.

- *Parameters*: `Context context`; `string workflowFile` — local file path; `string? integrity`;
  `Dictionary<string, string> parameters`.
- *Returns*: `Dictionary<string, string>`
- *Preconditions*: workflowFile must exist.
- *Post-conditions*: Workflow steps executed; variables returned.

**RunUrl(Context, string, string?, Dictionary)**: Downloads the workflow YAML bytes via an
HttpClient configured to use the system proxy and calls RunBytes.

- *Parameters*: `Context context`; `string url` — HTTP or HTTPS URL; `string? integrity`;
  `Dictionary<string, string> parameters`.
- *Returns*: `Dictionary<string, string>`
- *Preconditions*: url must be reachable and return HTTP 200.
- *Post-conditions*: Workflow steps executed; variables returned.

**RunBytes(Context, string, byte[], string?, Dictionary)**: Optionally verifies SHA-256 integrity,
parses the YAML stream, processes the parameters section, validates provided parameters, and
iterates over the steps sequence dispatching each command via CommandsRegistry.Commands. Before
each step is dispatched, if the step contains a `displayName` key, its value is printed to the
context. Caller-supplied parameter keys that are not declared in the workflow's `parameters:`
section cause a `CommandErrorException` to be thrown. Workflow-declared parameters that are not
supplied by the caller use the default value defined in the workflow's `parameters:` section.

- *Parameters*: `Context context`; `string source` — display name for error messages;
  `byte[] bytes` — YAML content; `string? integrity`;
  `Dictionary<string, string> parameters`.
- *Returns*: `Dictionary<string, string>` — the local variables map after all steps execute.
- *Preconditions*: bytes must be valid YAML with a root mapping node containing a steps sequence.
- *Post-conditions*: All steps executed in order; local variables returned as outputs.

**ResolveNuGetFile(YamlMappingNode, string, string?, string?)**: Validates that nuget and url are
not both specified, that file is present, parses the PackageName:version format, calls
NuGetCache.EnsureCachedAsync to download and cache the package, and uses
PathHelpers.SafePathCombine to construct the resolved file path. Path-traversal is prevented by
SafePathCombine, which rejects any file argument that escapes the package root directory.

- *Parameters*: `YamlMappingNode step` — for error reporting; `string nuget` — NuGet package spec
  in "PackageName:version" format; `string? file` — required relative path within the package;
  `string? url` — must be null when nuget is specified.
- *Returns*: `string` — the absolute local file path to the resolved workflow file.
- *Preconditions*: nuget must be non-null; url must be null; file must be non-null and must not
  escape the package root.
- *Error conditions*: YamlException when url is also specified, when file is null, or when nuget
  lacks the ":" separator. CommandErrorException (from PathHelpers.SafePathCombine) when file
  attempts path traversal.
- *Security rationale*: SafePathCombine prevents path-traversal attacks by rejecting any
  file argument that resolves outside the NuGet package root directory.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when no arguments are provided or a
parameter is malformed (missing "="); thrown by Run(Context, YamlMappingNode, Dictionary) when a
requested workflow output is not produced; thrown by RunBytes when an unknown command is referenced
in a step; thrown by RunFile when the workflow file does not exist.

**CommandErrorException** — thrown by RunUrl when the HTTP response is not 200; thrown by RunBytes
when the integrity check fails, when the YAML structure is invalid, when the steps key is missing,
when a step is not a mapping node, when a step mapping node lacks the `command` key (message:
"Workflow {source} step missing command"), or when a caller-supplied parameter name is not declared
in the workflow's parameters section.

**YamlException** — thrown by Run(Context, YamlMappingNode, string?, string?, string?, Dictionary)
when both `file` and `url` are specified or when neither is specified; thrown by ResolveNuGetFile
when `nuget` and `url` are both specified, when `nuget` is used without a `file` input, or when
the `nuget` value does not contain the `:` separator.

#### Dependencies

- Command (abstract base class)
- CommandsRegistry (sibling registry — dispatches each workflow step)
- Context (execution context)
- NuGetCache (DemaConsulting.NuGet.Caching — NuGet package resolution)
- PathHelpers (Utility subsystem — safe path combination)
- System.Net.Http.HttpClient, HttpClientHandler
- System.Security.Cryptography.SHA256
- YamlDotNet (YamlStream, YamlMappingNode, YamlSequenceNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- Itself (recursively, when a workflow step specifies command: run-workflow)
