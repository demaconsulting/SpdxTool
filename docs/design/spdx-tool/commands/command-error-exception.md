### CommandErrorException

#### Purpose

CommandErrorException signals that a command has encountered a runtime error from which it
cannot recover. Program catches this exception and reports the message via context.WriteError
without printing usage information, distinguishing it from CommandUsageException (which also
prints usage). It is thrown when the problem is an operational failure rather than incorrect
usage.

#### Data Model

N/A — CommandErrorException carries only the inherited Exception.Message string.

#### Key Methods

**CommandErrorException(string)**: Constructs the exception with a message describing the
error.

- *Parameters*: `string message` — human-readable error description.

**CommandErrorException(string, Exception)**: Constructs the exception with a message and
an inner exception.

- *Parameters*: `string message` — human-readable error description; `Exception innerException`
  — the underlying cause.

#### Error Handling

N/A — this class is itself an exception type.

#### Dependencies

- System.Exception (base class)

#### Callers

- Program.Run — catches CommandErrorException and calls context.WriteError(ex.Message)
- Various Command subclasses — throw CommandErrorException for operational failures
