## Program

### Purpose

Program is the application entry point and top-level orchestrator. Main creates a Context from
the command-line arguments, calls Run, and sets Environment.ExitCode from the context error
count. Run implements the global dispatch logic: version output, help output, self-validation,
and command dispatch. It also handles exceptions by routing CommandUsageException and
CommandErrorException to appropriate error output and printing usage when needed.

### Data Model

**Version**: `string` (static readonly) — the assembly informational version, read from
AssemblyInformationalVersionAttribute at startup.

### Key Methods

**Main(string[])**: Application entry point. Creates a Context, runs it, and sets the process
exit code.

- *Parameters*: `string[] args` — raw command-line arguments.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Environment.ExitCode is set to 0 (no errors) or 1 (errors recorded or
  uncaught InvalidOperationException). Unhandled non-InvalidOperationException exceptions are
  re-thrown after printing.
- *Remarks*: InvalidOperationException from Context.Create (e.g., missing argument, invalid
  depth) is caught and reported as 'Error: {message}' with exit code 1. All other unhandled
  exceptions are reported and re-thrown. Environment.ExitCode is set from context.ExitCode
  (1 if any errors were recorded, 0 otherwise).

**Run(Context)**: Core execution logic. Dispatches to version display, help display,
self-validation, or command execution based on Context flags.

- *Parameters*: `Context context` — execution context carrying all parsed flag values.
- *Returns*: `void`
- *Preconditions*: context must not be null.
- *Post-conditions*: The requested operation has been performed; any errors are recorded in
  context.Errors.
- *Remarks*: Each call to context.WriteError increments the error counter; the caller should
  check context.ExitCode after Run returns to determine whether any errors occurred.

**PrintUsage(Context)**: Writes usage information to the context output.

- *Parameters*: `Context context` — execution context.
- *Returns*: `void`
- *Preconditions*: None.
- *Post-conditions*: Full usage text including global options and registered commands is written
  to the context output.

### Error Handling

- **InvalidOperationException**: caught in Main; message is printed to console in red and the
  process exits with code 1.
- **Exception** (non-InvalidOperationException): caught in Main; the full exception including
  stack trace is printed to console in red and then re-thrown.
- **CommandUsageException**: caught in Run; message is printed via context.WriteError and usage
  information is printed.
- **CommandErrorException**: caught in Run; message is printed via context.WriteError.
- **Exception** (in Run): caught; full exception text printed via context.WriteError.

### Dependencies

- Context (creates and consumes)
- CommandsRegistry (looks up commands by name)
- SelfTest.Validate (invoked when --validate flag is set)
- System.Environment (sets ExitCode)
- System.Console (for exception reporting in Main)
- System.Reflection.AssemblyInformationalVersionAttribute (reads Version)

### Callers

- .NET runtime entry point (calls Main)
- Test suite (calls Run directly with a test Context for unit testing)
