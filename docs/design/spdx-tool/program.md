## Program

![Program Structure](SpdxToolView.svg)

### Purpose

Program is the application entry point and top-level orchestrator. Main first hands the `help`
command to HelpCommand; otherwise it creates a Context from the command-line arguments, calls
Run, and returns the exit code from the context error count. Run implements the global dispatch
logic: version output, usage output, self-validation,
and command dispatch. It also handles exceptions by routing CommandUsageException and
CommandErrorException to appropriate error output and printing usage when needed.

### Data Model

**Version**: `string` (static readonly) — the assembly informational version, read from
AssemblyInformationalVersionAttribute at startup.

### Key Methods

**Main(string[])**: Application entry point. Answers the `help` command, or creates a Context,
runs it, and returns the process exit code.

- *Parameters*: `string[] args` — raw command-line arguments.
- *Returns*: `int` — the exit code: for the `help` command, the exit code of HelpCommand (see
  *Help Command* in the system design); otherwise 0 (no errors) or 1 (errors recorded or
  uncaught InvalidOperationException).
- *Preconditions*: None.
- *Post-conditions*: Unhandled non-InvalidOperationException exceptions are re-thrown after
  printing.
- *Remarks*: Main first calls `HelpCommand.Default.TryRun(args, out exitCode)`; when the help
  command answers, Main returns its exit code without creating a Context, so no banner is printed
  and `--silent` and `--log` do not apply. InvalidOperationException from Context.Create (e.g.,
  missing argument, invalid depth) is caught and reported as 'Error: {message}' with exit code 1.
  All other unhandled exceptions are reported and re-thrown. Otherwise Main returns
  context.ExitCode (1 if any errors were recorded, 0 otherwise).

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
- *Post-conditions*: Full usage text including the `spdx-tool help [topic]` usage line, global
  options, and registered commands is written to the context output, followed by a blank line and
  `HelpCommand.Default.Footer`, the pointer to the help command.

### Error Handling

- **InvalidOperationException**: caught in Main; message is printed to console in red and Main
  returns exit code 1.
- **Exception** (non-InvalidOperationException): caught in Main; the exception message (not
  stack trace) is printed to console in red and then re-thrown.
- **CommandUsageException**: caught in Run; message is printed via context.WriteError and usage
  information is printed.
- **CommandErrorException**: caught in Run; message is printed via context.WriteError.
- **Exception** (in Run): caught; full exception text printed via context.WriteError.

### Dependencies

- HelpCommand (answers the `help` command before the options are parsed, and supplies the footer
  that ends the usage text)
- Context (creates and consumes)
- CommandsRegistry (looks up commands by name)
- SelfTest.Validate (invoked when --validate flag is set)

- System.Console (for exception reporting in Main)
- System.Reflection.AssemblyInformationalVersionAttribute (reads Version)

### Callers

- .NET runtime entry point (calls Main)
- Test suite (calls Run directly with a test Context, and Main with the help command, for unit
  testing)
