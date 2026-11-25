### Help

![Help Structure](CommandsView.svg)

#### Purpose

Help displays the extended usage details for a named command by looking it up in CommandsRegistry
and writing each line of its Details array to the console. It is a workflow-only command: on the
command line, `spdx-tool help` as the first argument is answered by HelpCommand from the built-in
help cards before the options are parsed, so a `help` that reaches the command dispatcher (after
global options) is rejected.

#### Data Model

Help maintains three static members and carries no per-invocation state.

**Command**: `string` constant (`"help"`) — command name used for dispatch registration.
**Instance**: `Help` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for Help.

#### Key Methods

**Run(Context, string[])**: Rejects command-line invocation.

- *Parameters*: `Context context` — execution context (unused); `string[] args` — arguments
  (unused).
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Always throws CommandUsageException with the message
  `'help' command is only valid in a workflow`.

**Run(Context, YamlMappingNode, Dictionary)**: Reads the about input from the YAML step node and
calls ShowUsage.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: about input is required. An absent `inputs` node is treated identically to an
  absent `about` input and results in a `YamlException`.
- *Post-conditions*: The named command's detailed usage text is written to context. Workflow
  variable references in the `about` value are expanded using the `variables` dictionary before
  the resolved name is passed to `ShowUsage`.

**ShowUsage(Context, string)**: Looks up the command entry in CommandsRegistry.Commands and writes
each line of entry.Details to context.

- *Parameters*: `Context context` — execution context; `string command` — name of the command to
  display help for.
- *Returns*: `void`
- *Preconditions*: None (unknown command names trigger a usage error).
- *Post-conditions*: All detail lines are written to context.

#### Error Handling

**CommandUsageException** — always thrown by Run(Context, string[]), because the command is
workflow only; thrown by ShowUsage when the requested command name is not present in
CommandsRegistry.Commands.

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when the about input is
missing.

#### Dependencies

- Command (abstract base class)
- CommandsRegistry (sibling registry — Commands dictionary lookup)
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: help
