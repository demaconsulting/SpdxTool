### Query

#### Purpose

Query executes an external program, captures its combined stdout and stderr output, and applies a
regular expression with a named capture group "value" to extract a single result string. In CLI
mode the value is written to the console. In workflow mode it is stored in the named output
variable. It is available from both the CLI and workflow YAML files.

#### Data Model

Query carries no mutable instance state; all state is passed via method parameters. The following
static fields serve as registry entries:

**Command**: `private const string` — the registered command name `"query"`.

**RegexMatchTimeoutMs**: `private const int` — match timeout in milliseconds (100) guarding against
catastrophic backtracking (ReDoS protection).

**Instance**: `Query` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for Query.

#### Key Methods

**Run(Context, string[])**: Parses pattern, program, and optional arguments from CLI args, calls
QueryProgramOutput, and writes the result to context.

- *Parameters*: `Context context` — execution context; `string[] args` — [pattern, program,
  args...].
- *Returns*: `void`
- *Preconditions*: args.Length must be at least 2.
- *Post-conditions*: The captured value is written to context.

**Run(Context, YamlMappingNode, Dictionary)**: Reads output, pattern, program, and arguments inputs
from the YAML step node, calls QueryProgramOutput, and stores the result in variables[output].

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: output, pattern, and program inputs are required.
- *Post-conditions*: variables[output] contains the captured value.

**QueryProgramOutput(string, string, string[])**: Compiles the regular expression (requiring a
"value" capture group), launches the program with the supplied arguments using
System.Diagnostics.Process, reads stdout and stderr concurrently to avoid deadlock, and scans the
output lines for the first non-empty "value" match. The regex pattern is compiled with a 100 ms
match timeout to prevent catastrophic backtracking (ReDoS protection).

- *Parameters*: `string pattern` — regular expression pattern with a "value" capture group;
  `string program` — program path or name; `string[] arguments` — program arguments.
- *Returns*: `string` — the captured value.
- *Preconditions*: The pattern must contain a "value" named capture group. The program must be
  executable.
- *Post-conditions*: Returns the first non-empty "value" match from the combined output.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) when fewer than two arguments are
provided; thrown by QueryProgramOutput when the pattern is syntactically invalid or when the
pattern does not contain a "value" capture group.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when output, pattern, or
program inputs are missing.

**CommandErrorException** — thrown by QueryProgramOutput when the program cannot be started; also
thrown when the pattern is not matched in any line of the program output.

#### Dependencies

- Command (abstract base class)
- System.Diagnostics.Process, ProcessStartInfo
- System.Text.RegularExpressions.Regex
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: query
