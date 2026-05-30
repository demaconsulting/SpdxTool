## YamlDotNet

### Verification Approach

YamlDotNet is verified through integration tests that exercise the RunWorkflow command with YAML
workflow files exercising its parsing capabilities. These tests are part of
`test/DemaConsulting.SpdxTool.Tests/Commands/RunWorkflowTests.cs` and
`test/DemaConsulting.SpdxTool.Tests/SelfTest/`. They confirm that YAML mapping nodes, sequence
nodes, and scalar nodes are correctly deserialized from workflow files into the structures that
command dispatch expects.

The SelfTest subsystem provides additional end-to-end coverage: the `--validate` flag exercises
all registered commands, many of which read YAML workflow steps, confirming that YamlDotNet
functions correctly in the deployed tool environment. The self-validation test
`SpdxTool_SelfTest_ValidateFlag_Succeeds` covers this scenario.

No vendor test results or third-party compliance reports are required; sufficient coverage is
provided by the integration tests and self-validation suite described above.
