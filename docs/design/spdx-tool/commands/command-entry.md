### CommandEntry

#### Purpose

CommandEntry is an immutable record that associates a command name and usage metadata with
its Command implementation singleton. CommandsRegistry stores CommandEntry instances and uses
them both for dispatching commands and for generating help output.

#### Data Model

**Name**: `string` — the primary command name used for dispatch (e.g., "add-package").
**CommandLine**: `string` — example command-line invocation shown in usage output.
**Summary**: `string` — one-line description shown in the command list.
**Details**: `string[]` — extended help lines printed by the help command.
**Instance**: `Command` — the singleton Command implementation to invoke.

#### Key Methods

N/A — CommandEntry is a record with no methods beyond auto-generated equality and
deconstruct.

#### Error Handling

N/A — CommandEntry is a plain data record; it performs no operations that can fail.

#### Dependencies

- Command (abstract base class — the `Instance` field type)

#### Callers

- CommandsRegistry — stores and exposes CommandEntry instances
- Program.PrintUsage — reads CommandLine and Summary for usage output
- Help command — reads Details for extended help output
