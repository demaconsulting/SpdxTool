### CommandsRegistry

#### Purpose

CommandsRegistry is the central static registry that maps command names to their CommandEntry
instances. It is populated at startup with one entry per supported command and exposes an
immutable read-only view for lookup and enumeration. Program and RunWorkflow consult
CommandsRegistry to dispatch commands by name.

#### Data Model

**InternalCommands**: `Dictionary<string, CommandEntry>` — private static dictionary mapping
  command name strings to their CommandEntry records, populated at type initialization.
**Commands**: `IReadOnlyDictionary<string, CommandEntry>` — public read-only view of
  InternalCommands.

#### Key Methods

N/A — CommandsRegistry exposes only the `Commands` read-only property; all population
occurs in the static field initializer.

#### Error Handling

N/A — CommandsRegistry performs no runtime operations that can fail. Lookup failures
(unknown command names) are handled by the caller.

#### Dependencies

- CommandEntry (record type stored in the dictionary)
- All Command subclasses (referenced via their static Entry fields during initialization)

#### Callers

- Program.Run — calls Commands.TryGetValue to look up and dispatch CLI commands
- Program.PrintUsage — iterates Commands.Values to list available commands
- RunWorkflow — calls Commands.TryGetValue to look up and dispatch workflow step commands
