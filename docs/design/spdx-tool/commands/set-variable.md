### SetVariable

#### Purpose

SetVariable assigns a value to a named variable in the current workflow variable dictionary. It
is available in workflow mode only; direct CLI invocation is rejected.

#### Data Model

**Instance**: `SetVariable` — the singleton instance registered with CommandsRegistry.
**Entry**: `CommandEntry` — the CommandEntry record for SetVariable.

#### Key Methods

**Run(Context, string[])**: Rejects CLI invocation with a usage error.

- *Parameters*: `Context context` — execution context; `string[] args` — CLI arguments (unused).
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Throws CommandUsageException unconditionally.

**Run(Context, YamlMappingNode, Dictionary)**: Reads the value and output inputs from the YAML
step, then sets variables[output] = value. The output key is read without variable expansion so
that the literal key name is used as the variable name.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step node;
  `Dictionary<string, string> variables` — workflow variable map (mutated).
- *Returns*: `void`
- *Preconditions*: value and output inputs are required in the step.
- *Post-conditions*: variables[output] is set to the expanded value.

#### Error Handling

**CommandUsageException** — thrown by Run(Context, string[]) unconditionally (workflow-only
command).

**YamlException** — thrown by Run(Context, YamlMappingNode, Dictionary) when the value or output
inputs are missing.

#### Dependencies

- Command (abstract base class)
- Context (execution context)
- YamlDotNet (YamlMappingNode, YamlException)

#### Callers

- CommandsRegistry — holds the CommandEntry.Instance reference and routes workflow steps
- RunWorkflow — dispatches this command when a workflow step specifies command: set-variable
