## HelpCommand

### Verification Approach

`HelpCommand` is verified with unit tests in
`test/DemaConsulting.SpdxTool.Tests/HelpCommandTests.cs`. Each test creates a help command with
`HelpCommand.Create`, which writes to in-memory writers instead of the console. No mocking is
applied: the tests use the real help cards embedded in the tool assembly and the real HelpCards
library, a documented dependency of the unit.

### Test Environment

Standard xUnit v3 test runner with `dotnet test`. No files, external services, or network access
are required; the cards are read from the tool assembly.

### Acceptance Criteria

All `HelpCommandTests` test methods pass, confirming that the help verb prints the topic index,
every registered command has a card that `help <command>` and `help commands <command>` print,
the shipped catalog passes validation with warnings treated as errors, and every card prints in
lines of at most 120 characters.

### Test Scenarios

**HelpVerb**: `TryRun` called with `["help"]` answers with exit code 0, and the output lists the
command of every root topic. This scenario is tested by
`HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero`.

**CommandCards**: `TryRun` called with `["help", <command>]` and with
`["help", "commands", <command>]` for every command registered in `CommandsRegistry` answers with
exit code 0 and prints the card of that command. This scenario is tested by
`HelpCommand_TryRun_EveryRegisteredCommand_PrintsCommandCard`.

**CommandNameAfterOption**: `TryRun` called with `["help", "--width", "100", "to-markdown"]`
answers with exit code 0 and prints the card of `to-markdown`, not the commands overview, which
shows that the value of a help option is not taken for the command name. This scenario is tested
by `HelpCommand_TryRun_CommandNameAfterValueOption_PrintsCommandCard`.

**ValidCards**: the catalog configuration from `CreateOptions`, validated with warnings treated as
errors, reports no defect, which proves that the root cards are exactly `RootCardIds`, that every
card carries one purpose tag, and that every command example parses as a command line of the
tool. This scenario is tested by `HelpCommand_CreateOptions_ShippedCatalog_PassesValidation`.

**LineWidth**: every card of the shipped catalog, printed with its links and tokens expanded and a
fixed, long prerelease version in place of the build's version, exits with code 0 and has no line
longer than 120 characters. This scenario is tested by
`HelpCommand_TryRun_EveryCard_FitsMaximumLineWidth`.
