### Print

#### Purpose

Print writes one or more lines of text to the console. In CLI mode each argument is printed as a
separate line. In workflow mode a "text" sequence node provides the lines, with variable expansion
applied. It is available from both the CLI and workflow YAML files.

#### Data Model

Print holds no mutable instance state; its data model consists of static members only.

**Command**: `string` constant (`"print"`) — command name used for dispatch registration.
**Instance**: `Print` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for Print.

#### Key Methods

**Run(Context, string[])**: Writes each element of args as a line to context.

- *Parameters*: `Context context` — execution context; `string[] args` — lines to print.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Each arg is written to context.

**Run(Context, YamlMappingNode, Dictionary<string, string>)**: Reads the text sequence from the YAML step
inputs, applies variable expansion to each entry via GetSequenceString, and writes each line to context.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: `void`
- *Preconditions*: text input (a YAML sequence) is required.
- *Post-conditions*: All expanded text lines are written to context.

#### Error Handling

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary<string, string>) when the text
sequence input is absent from the step inputs.

**InvalidOperationException** — propagated from GetSequenceString/Expand when a text line contains
an undefined variable reference, an empty variable name, or an unmatched macro delimiter (`${{` or
`}}`).

#### Dependencies

- Command (abstract base class)
- Context (output channel — WriteLine method)
- YamlDotNet (YamlMappingNode, YamlSequenceNode, YamlException)

#### Callers

- CommandsRegistry — routes CLI and workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: print
