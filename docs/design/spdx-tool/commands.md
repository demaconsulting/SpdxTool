## Commands

### Overview

The Commands subsystem provides all CLI command implementations for the DemaConsulting.SpdxTool.
It defines the abstract Command base class, the CommandEntry dispatch record, the CommandsRegistry
lookup table, CommandUsageException and CommandErrorException exception types, and sixteen concrete
command classes.

Each concrete command encapsulates a single SPDX document operation. Commands are registered by
name in CommandsRegistry, making them available both for direct CLI invocation and as steps in
workflow YAML files.

Contained units:

- Command — abstract base class defining the two-overload Run interface and YAML helper methods
- CommandEntry — immutable record carrying metadata (Name, CommandLine, Summary, Details, Instance) for a command
- CommandsRegistry — static registry mapping command names to CommandEntry instances
- CommandUsageException — signals incorrect CLI argument usage
- CommandErrorException — signals a command runtime failure
- AddPackage — adds or merges a package in an SPDX document (workflow only)
- AddRelationship — adds a relationship between SPDX elements
- CopyPackage — copies a package from one SPDX document to another
- Diagram — generates a Mermaid entity-relationship diagram from an SPDX document
- FindPackage — finds a package in an SPDX document by criteria
- GetVersion — retrieves the version of a package from an SPDX document
- Hash — computes or verifies SHA-256 file hashes
- Help — displays extended help for a named command
- Print — writes text lines to the console
- Query — executes an external program and extracts a value by regex
- RenameId — renames an SPDX element ID throughout a document
- RunWorkflow — executes a multi-step workflow YAML file
- SetVariable — sets a workflow variable (workflow only)
- ToMarkdown — generates a Markdown summary of an SPDX document
- UpdatePackage — updates package metadata fields in an SPDX document (workflow only)
- Validate — validates an SPDX document for specification conformance

### Interfaces

**CommandCliDispatch**: The CLI dispatch interface used by Program to execute any registered command.

- *Type*: In-process .NET public abstract method
- *Role*: Provider (CommandsRegistry and Program call Run on concrete Command subclasses)
- *Contract*: `void Run(Context context, string[] args)` — executes the command using the supplied
  CLI arguments array; writes results to context.
- *Constraints*: Throws CommandUsageException for incorrect argument count or invalid options.
  Throws CommandErrorException for runtime failures. Commands that are workflow-only throw
  CommandUsageException immediately.

**CommandWorkflowDispatch**: The workflow step dispatch interface used by RunWorkflow to execute
a command step from a YAML workflow file.

- *Type*: In-process .NET public abstract method
- *Role*: Provider (RunWorkflow calls Run on concrete Command subclasses via CommandsRegistry)
- *Contract*: `void Run(Context context, YamlMappingNode step, Dictionary<string, string> variables)`
  — executes the command using the YAML step node and the current variable map; writes results
  to context and may update variables.
- *Constraints*: Throws YamlException for missing or invalid YAML inputs. Throws
  CommandErrorException for runtime failures.

**CommandsRegistry**: The command lookup service providing access to all registered commands.

- *Type*: In-process .NET public static API
- *Role*: Provider (Program and RunWorkflow call Commands to look up command instances)
- *Contract*: `IReadOnlyDictionary<string, CommandEntry> Commands` — returns the mapping of
  command name strings to CommandEntry instances.
- *Constraints*: Read-only after static initialization; all entries are pre-populated at startup
  and cannot be modified at runtime.

### Design

Program receives the command name as the first CLI argument and looks it up in
CommandsRegistry.Commands. On a successful lookup it calls Run(Context, string[]) on the command
instance embedded in the CommandEntry. On a failed lookup it prints a usage summary listing all
registered command names and their summaries.

RunWorkflow reads a YAML workflow document, processes the parameters section into a local variables
dictionary, then iterates over the steps sequence. For each step it reads the command key, looks it
up in CommandsRegistry.Commands, and calls Run(Context, YamlMappingNode, Dictionary) on the instance.
This dispatch path allows any registered command to be used as a workflow step.

The two Run overloads on each concrete class share a common static implementation method (for example
AddPackage.AddPackageToSpdxFile, CopyPackage.CopyPackageBetweenSpdxFiles) so that the YAML parsing
code and the CLI parsing code converge at the same business logic. Command base class helper methods
(GetMapString, GetMapMap, GetMapSequence, GetSequenceString, Expand) are available to all subclasses
for consistent YAML node extraction and variable expansion.

Variable expansion also supports environment variable references. When a variable name starts
with `environment.`, the text after the prefix is passed to `Environment.GetEnvironmentVariable`
to read from the process environment. This allows workflow files to reference CI secrets and
system paths without hard-coding them. If the environment variable is not set,
`InvalidOperationException` is thrown as with any other undefined variable.
