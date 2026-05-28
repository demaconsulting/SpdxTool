### CommandUsageException

#### Purpose

CommandUsageException signals that a command was invoked incorrectly (wrong number of
arguments, invalid option, or workflow-only command called from the CLI). Program catches
this exception, reports the message via context.WriteError, and then prints usage
information, guiding the user toward correct invocation.

#### Data Model

N/A — CommandUsageException carries only the inherited Exception.Message string.

#### Key Methods

**CommandUsageException(string)**: Constructs the exception with a message describing the
usage error. Implemented as a primary constructor.

- *Parameters*: `string message` — human-readable description of the incorrect usage.

#### Error Handling

N/A — this class is itself an exception type.

#### Dependencies

- System.Exception (base class)

#### Callers

- Program.Run — catches CommandUsageException, calls context.WriteError(ex.Message), and
  calls PrintUsage(context)
- Command subclasses — throw CommandUsageException when CLI arguments are invalid or when a
  workflow-only command is called from the CLI
- SpdxHelpers.LoadJsonDocument — throws CommandUsageException when the specified SPDX file
  does not exist
