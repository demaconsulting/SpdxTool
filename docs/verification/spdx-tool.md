# DemaConsulting.SpdxTool

## Verification Approach

DemaConsulting.SpdxTool is verified with automated command, subsystem, utility, and integration
tests in `test/DemaConsulting.SpdxTool.Tests/`. The repository uses direct command tests for the
command implementations and a built-in self-validation workflow driven by the `--validate` flag
to provide installed-tool evidence across the command surface.

System-level verification also relies on the self-test subsystem to exercise end-to-end command
behavior against representative SPDX fixtures and to emit CI-consumable TRX or JUnit results.

The built-in `help` command is verified by running the compiled tool with `help` arguments. The
command examples of the help cards are verified with the example runner of
`DemaConsulting.HelpCards`: it gives each card a fresh scratch folder and calls a test delegate for
each command example. The delegate copies the SPDX documents that the examples read from
`test/DemaConsulting.SpdxTool.Tests/TestData/HelpExamples/` into that folder, runs the compiled
tool there with `Runner.RunInDirectory`, and the runner compares the exit code and the output with
the `exit` and `expect` values of the example.

## Test Environment

The system tests run on .NET 10 and .NET 11 with file-system access to local SPDX JSON
fixtures and workflow YAML files. Query and self-test scenarios require `dotnet` on the system
path, and the NuGet workflow scenario additionally requires either a populated NuGet cache or
network access to restore the referenced workflow package.

## Acceptance Criteria

Verification is acceptable when the automated tests in `test/DemaConsulting.SpdxTool.Tests/`
pass for the supported target frameworks, when direct command invocation reports the expected
results or errors, and when `--validate` completes successfully and can emit result files in the
requested format. The `help` command must list the topics with exit code 0 and exit with code 2
for an unknown topic, and every command example in the help cards must give the exit code and
output that its card states.

## Test Scenarios

**VersionReporting**: the tool reports its build version through the program entry point. This
scenario is tested by `SpdxTool_Program_Run_VersionContext_WritesVersion` (unit test that calls
Program.Run directly with a version context) and by `SpdxTool_Version_ShortFlag_DisplaysVersion`
and `SpdxTool_Version_LongFlag_DisplaysVersion` (integration tests that invoke the full tool).

**HelpAndUsage**: the tool prints command-line usage for help requests. This scenario is tested by
`SpdxTool_Program_Run_HelpContext_WritesUsage` (unit test) and by
`SpdxTool_Usage_ShortHelpFlag_DisplaysUsage`, `SpdxTool_Usage_LongHelpFlag_DisplaysUsage`, and
`SpdxTool_Usage_QuestionMarkHelpFlag_DisplaysUsage` (integration tests).

**MissingArgumentHandling**: the tool reports a usage error when invoked without a command or
option. This scenario is tested by `SpdxTool_Program_Run_NoArguments_WritesErrorAndUsage` (unit
test) and by `SpdxTool_Usage_NoArguments_DisplaysError` (integration test).

**SelfValidationExecution**: the installed tool executes the full self-validation suite through the
`--validate` flag. This scenario is tested by `SpdxTool_SelfTest_ValidateFlag_Succeeds`.

**TRXResultEmission**: the installed tool writes self-validation results in TRX format when
requested. This scenario is tested by `SpdxTool_SelfTest_ValidateFlagWithResults_GeneratesTrxFile`.

**JUnitResultEmission**: the installed tool writes self-validation results in JUnit XML format when
requested. This scenario is tested by `SpdxTool_SelfTest_ValidateFlagWithResults_GeneratesJUnitFile`.

**SilentFlagSuppression**: the tool suppresses console output when the silent flag is set. This
scenario is tested by `SpdxTool_Silent_ShortFlag_SuppressesOutput`
and `SpdxTool_Silent_LongFlag_SuppressesOutput`.

**LogFileOutput**: the tool writes output to a log file when the log flag is set. This scenario
is tested by `SpdxTool_Log_ShortFlag_WritesOutputToFile` and
`SpdxTool_Log_LongFlag_WritesOutputToFile`.

**DepthControl**: the tool controls self-validation output depth when the depth flag is set. This
scenario is tested by
`SpdxTool_SelfTest_ValidateFlagWithDepth_ShowsDepth`.

**ShortResultFlagAlias**: the tool accepts -r as a short alias for the --result flag. This scenario
is tested by
`SpdxTool_SelfTest_ValidateFlagWithResults_ShortFlag_GeneratesTrxFile`.

**SupportedCommands**: the tool registers and dispatches all supported SPDX manipulation commands.
This scenario is tested by `SpdxTool_Usage_NoArguments_DisplaysError` and, at the Commands
subsystem level, by `CommandsRegistry_Commands_AllExpectedNames_ArePresent`.

**WorkflowExecution**: the tool executes SPDX manipulation workflows defined in YAML files. This
scenario is tested by
`RunWorkflow_Run_ValidWorkflowFile_ExecutesWorkflow`.

**CrossPlatformSupport**: the tool runs successfully on Windows, Linux, and macOS. This scenario
is tested by `SpdxTool_Basic` on each supported
platform in the CI matrix (`windows@SpdxTool_Basic`, `ubuntu@SpdxTool_Basic`,
`macos@SpdxTool_Basic`).

**MultiFrameworkSupport**: the tool runs on .NET 10 and .NET 11. This scenario is tested by
`SpdxTool_Version_ShortFlag_DisplaysVersion`
on each supported target framework.

**UnsupportedResultExtension**: the tool reports an error when an unsupported extension is supplied to
the `--result` flag. This scenario is tested by
`SpdxTool_SelfTest_UnsupportedResultExtension_ReportsError`.

**SilentLogCombination**: the tool writes output to the log file but not to the console when both
`--silent` and `--log` are supplied together. This scenario is tested by
`SpdxTool_Log_SilentFlag_WritesToLogButNotConsole`.

**HelpTopics**: the tool lists its help topics when invoked with `help` as the sole argument, with
exit code 0 and without the banner. This scenario is tested by
`SpdxTool_HelpTopics_HelpVerb_ListsTopics`.

**HelpUnknownTopic**: the tool exits with code 2 and names the topic when invoked with
`help no-such-topic`. This scenario is tested by
`SpdxTool_HelpTopics_UnknownTopic_ReturnsUsageError`.

**HelpExamples**: every command example of every help card, run against the compiled tool in a
scratch folder that holds the example SPDX documents, gives the exit code and output fragments that
its card states, and at least one command example exists. This scenario is tested by
`SpdxTool_HelpExamples_AllCommandExamples_GiveStatedResults`.
