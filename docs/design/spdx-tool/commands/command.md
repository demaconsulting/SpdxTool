### Command

#### Purpose

Command is the abstract base class for all SpdxTool commands. It defines the execution
contract that every command must satisfy: handling both CLI invocation (`Run(Context, string[])`)
and workflow-step invocation (`Run(Context, YamlMappingNode, Dictionary)`). It also provides
shared helper methods for YAML node access and variable expansion, which all concrete command
implementations inherit.

#### Data Model

N/A — Command is an abstract class with no instance state; all state is provided through
method parameters.

#### Key Methods

**Run(Context, string[])**: Abstract method invoked when the command is called from the CLI.

- *Parameters*: `Context context` — execution context; `string[] args` — command arguments.
- *Returns*: `void`
- *Preconditions*: None (enforced by concrete implementations).
- *Post-conditions*: Concrete implementations complete their operation or throw CommandUsageException
  or CommandErrorException.

**Run(Context, YamlMappingNode, Dictionary)**: Abstract method invoked when the command is
called from a workflow step.

- *Parameters*: `Context context` — execution context; `YamlMappingNode step` — YAML step
  node; `Dictionary<string, string> variables` — current workflow variable map.
- *Returns*: `void`
- *Preconditions*: None (enforced by concrete implementations).
- *Post-conditions*: Concrete implementations complete their operation or throw YamlException
  or CommandErrorException.

**Expand(string, Dictionary)**: Expands `${{ variable }}` tokens in a text string by looking
up variable names in the provided dictionary or the process environment.

- *Parameters*: `string text` — input text possibly containing `${{ ... }}` tokens;
  `Dictionary<string, string> variables` — variable map.
- *Returns*: Expanded string with all tokens replaced.
- *Preconditions*: None.
- *Post-conditions*: All `${{ name }}` tokens are replaced with their values. Throws
  InvalidOperationException if a variable is undefined, the token is malformed, or brackets
  are unmatched.

**GetMapString(YamlMappingNode, string, Dictionary)**: Extracts a string value from a YAML
mapping node by key, applying variable expansion.

- *Parameters*: `YamlMappingNode? map` — YAML mapping node (may be null); `string key` —
  map key; `Dictionary<string, string> variables` — variable map.
- *Returns*: Expanded string value, or null if the map is null or the key is absent.
- *Preconditions*: None.
- *Post-conditions*: None.

#### Error Handling

**InvalidOperationException** — thrown by Expand when a variable name is undefined, a token
is malformed, or macro brackets are unmatched.

#### Dependencies

- YamlDotNet (YamlMappingNode, YamlSequenceNode)
- System.Text.StringBuilder (used by Expand)

#### Callers

- CommandsRegistry — holds CommandEntry references to Command subclass instances
- RunWorkflow — dispatches commands by calling Run(Context, YamlMappingNode, Dictionary)
- Program — dispatches CLI commands by calling Run(Context, string[])
- All Command subclasses inherit from Command
