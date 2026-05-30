## Context

### Purpose

Context holds all mutable execution state for a single invocation of SpdxTool. It encapsulates
the parsed global flag values (Version, Help, Silent, Validate, ValidationFile, Depth), the
remaining command arguments, an optional log-file writer, and an error counter. Program creates
one Context per invocation and passes it to all commands; the ExitCode property translates the
accumulated error count into a process exit code.

### Data Model

**_log**: `StreamWriter?` — optional log-file writer; null when no `--log` argument was provided.
**Version**: `bool` — true when `-v` or `--version` was specified.
**Help**: `bool` — true when `-h`, `-?`, or `--help` was specified.
**Silent**: `bool` — true when `-s` or `--silent` was specified.
**Validate**: `bool` — true when `--validate` was specified.
**ValidationFile**: `string` — path supplied to `-r`/`--result`; empty string when not specified.
**Depth**: `int` — depth value supplied to `--depth`; defaults to 1 when not specified.
**Arguments**: `IReadOnlyCollection<string>` — positional arguments following global flags.
**Errors**: `int` — count of errors recorded via WriteError.
**ExitCode**: `int` — 0 when Errors is zero, 1 otherwise.

### Key Methods

**Create(string[])**: Parses the program argument array and constructs a Context. Opens the
log file if `--log` was specified. Calls the private `ParseArgument` helper for each
recognized flag to consume its required value from the argument sequence.

- *Parameters*: `string[] args` — raw command-line arguments. Must not be null.
- *Returns*: `Context`
- *Preconditions*: args must not be null.
- *Post-conditions*: Returns a fully initialized Context. Throws ArgumentNullException when args
  is null. Throws InvalidOperationException if a flag is missing its required value argument or
  if the depth value is not a valid integer.

**WriteLine(string)**: Writes a line to the console (unless Silent) and to the log (if open).

- *Parameters*: `string text` — line to write.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Text written to console and/or log.
- *Thread-safety*: Not thread-safe; do not call concurrently from multiple threads.

**WriteWarning(string)**: Writes a warning line in dark yellow to the console (unless Silent)
and to the log.

- *Parameters*: `string message` — warning message.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Message written; console color restored to default.
- *Thread-safety*: Not thread-safe; do not call concurrently from multiple threads.

**WriteError(string)**: Writes an error line in red to the console (unless Silent), to the
log, and increments the Errors counter.

- *Parameters*: `string message` — error message.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Errors counter incremented; message written to console and/or log.
- *Thread-safety*: Not thread-safe; do not call concurrently from multiple threads.

**Dispose()**: Closes and disposes the log-file writer if one was opened.

- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Log writer is disposed; further calls to WriteLine/WriteError write only
  to the console. Dispose must be the final operation on the Context instance; calling
  Dispose while output is still being written from another thread is not supported.

### Error Handling

**ArgumentNullException** — thrown by Create when args is null.

**InvalidOperationException** — thrown by Create when a flag argument is missing (e.g.,
`--log` without a filename) or when `--depth` is followed by a non-integer value. Also
thrown by Create when the log file cannot be created (wraps UnauthorizedAccessException,
ArgumentException, NotSupportedException, IOException).

### Dependencies

- System.IO.StreamWriter (log file output)
- System.Console (console output with color support)

### Callers

- Program.Main — creates Context via Create, calls Run, then reads ExitCode
- Program.Run — reads flag properties and calls Write* methods
- All Command subclasses — call context.WriteLine, context.WriteError, context.WriteWarning
- SelfTest subsystem — reads ValidationFile and Depth, calls Write* methods
