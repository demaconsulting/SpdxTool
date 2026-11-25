## HelpCommand

![HelpCommand Structure](SpdxToolView.svg)

### Purpose

HelpCommand answers the `spdx-tool help` command from the help cards that are embedded in the
tool assembly. It is the only unit that references the HelpCards library.

### Data Model

**ToolName**: `string` constant — `spdx-tool`, the name of the tool as the user types it. The
usage lines, the index preamble, and the `{{tool}}` token of the cards are derived from it.

**RootCardIds**: `string[]` — the ids of the root cards, which are the topics that
`spdx-tool help` lists. It is the one list of topics; a missing or extra root card fails the
catalog.

**PurposeTags**: `string[]` (private) — `tutorial`, `reference`, and `troubleshooting`; the closed
tag vocabulary. Every card carries one of these tags and no other tag.

**Cards**: `HelpSource` (private, static) — the cards embedded in the tool assembly. Creating the
source reads nothing.

**Default**: `HelpCommand` (static) — the instance used by the tool; it writes to the console.

**_cli**: `HelpCli` — the command-line helper of HelpCards, created from `CreateOptions`. It is
immutable and keeps no state between calls.

**Help cards**: the Markdown files under `src/DemaConsulting.SpdxTool/Help/`, embedded by the
HelpCards build targets. The root cards are the topics; the `commands` card has one child card per
command registered in CommandsRegistry, under `Help/commands/`, which holds the command-line and
workflow-step usage of that command and is printed by `spdx-tool help commands <command>`. The
rules for writing the cards are in the *Help Cards* section of `CONTRIBUTING.md`.

### Key Methods

**TryRun**: Runs the help command when the arguments start with the `help` verb.

- *Parameters*: `string[] args` — the command-line arguments of the tool; `out int exitCode` —
  receives the exit code when the command was answered.
- *Returns*: `bool` — `true` when the command was answered; `false` when the tool should continue
  with its own option parser.
- *Preconditions*: None.
- *Post-conditions*: When `false` is returned, nothing was written. When `true` is returned, the
  answer was written to the console and the exit code was set (see *Help Command* in the system
  design).

Delegates to `HelpCli.TryRun`, which builds the catalog only after the verb was recognized.

**Footer**: Gets the one-line pointer to the help command that ends the usage text, without
building the catalog.

**CreateOptions**: Creates the options of the command-line helper.

- *Parameters*: optional `TextWriter` output and error writers (`null` for the console).
- *Returns*: `HelpCliOptions` — the tool name, the version from `Program.Version`, the catalog
  configuration (the embedded cards, `PurposeTags`, and `RootCardIds`), the usage lines, the
  index preamble, and the writers.
- *Preconditions*: None.
- *Post-conditions*: `GetCatalogConfiguration` of the options gives the tests the composition that
  the tool ships.

**Create**: Creates a help command that writes to the given writers; used by tests.

### Error Handling

`TryRun` does not throw for user input; the library writes errors to the error writer and sets the
exit code (see *Help Command* in the system design). The validation test reports every card defect
at build time, so a defective catalog is not shipped.

The command is answered before the options are parsed, so no Context exists and HelpCommand writes
its answers to the console itself. It is the one exception to Context owning all output: routing
the answers through a Context would mean parsing the options a second time just for help.

### Dependencies

- **DemaConsulting.HelpCards** (OTS) — `HelpCli`, `HelpCliOptions`, `HelpSource`, and
  `HelpCatalogBuilder`; see *HelpCards Integration Design*.
- **Program** — `Program.Version` supplies the version shown in the index and the `{{version}}`
  token.

### Callers

- **Program** — `Program.Main` calls `HelpCommand.Default.TryRun` before `Context.Create`, and
  `Program.PrintUsage` ends the usage text with `HelpCommand.Default.Footer`.
