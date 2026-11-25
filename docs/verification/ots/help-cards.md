## HelpCards

### Verification Approach

DemaConsulting.HelpCards is a runtime library with no self-validation mode, so it is verified by
tests in `test/DemaConsulting.SpdxTool.Tests/`, which use the real package and the cards embedded
in the tool assembly. No mocking is applied. No vendor test results or third-party compliance
reports are required; the tests described below provide sufficient evidence.

### Test Scenarios

**HelpCommand**: The help verb is answered from the embedded cards, in-process and by the built
tool. This scenario is tested by `HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero` and
`SpdxTool_HelpTopics_HelpVerb_ListsTopics`.

**TestHelpers**: The validator accepts the shipped cards, and the example runner runs every command
example against the built tool in a scratch folder per card. This scenario is tested by
`HelpCommand_CreateOptions_ShippedCatalog_PassesValidation` and
`SpdxTool_HelpExamples_AllCommandExamples_GiveStatedResults`.
