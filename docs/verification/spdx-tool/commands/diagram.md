### Diagram

#### Verification Approach

`Diagram` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/DiagramTests.cs`. The tests cover argument validation,
missing-file handling, option validation, and generation of Mermaid output with and without tool
relationships.

#### Test Environment

The tests run in the standard xUnit v3 environment with local SPDX JSON input and Mermaid output
files. No external service is required.

#### Acceptance Criteria

Verification is acceptable when invalid invocations are rejected, valid SPDX inputs produce Mermaid
diagrams, and the optional tools mode changes the rendered relationship set as intended.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when required arguments are omitted. This
scenario is tested by `Diagram_Run_MissingArguments_ReportsError`.

**InsufficientArguments**: the unit reports a usage error when fewer than the required two
arguments are provided. This scenario is tested by `Diagram_Run_InsufficientArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the input SPDX file is absent. This scenario is
tested by `Diagram_Run_MissingSpdxFile_ReportsError`.

**InvalidOption**: the unit rejects unsupported command options. This scenario is tested by
`Diagram_Run_InvalidOption_ReportsError`.

**DiagramGeneration**: the unit generates Mermaid output from a valid SPDX document. This scenario
is tested by `Diagram_Run_ValidSpdxFile_GeneratesDiagram`.

**ToolRelationshipRendering**: the unit includes tool relationships when the tools option is
requested. This scenario is tested by `Diagram_Run_WithToolsOption_GeneratesDiagramWithTools`.

**DefaultToolExclusion**: the unit excludes tool-related relationships from the generated diagram
when the `tools` option is not specified. This scenario is tested by
`Diagram_Run_WithoutToolsOption_ExcludesToolRelationships`.

**WorkflowStepInvocation**: the unit accepts spdx, mermaid, and optional tools inputs when invoked
from a workflow step and produces a Mermaid diagram file. This scenario is tested by
`Diagram_Run_InWorkflow_GeneratesDiagram`.

**WorkflowMissingSpdxInput**: the unit reports an error when the `spdx` input is absent from the
workflow step. This scenario is tested by `Diagram_Run_MissingSpdxInput_ReportsError`.

**WorkflowMissingMermaidInput**: the unit reports an error when the `mermaid` input is absent from
the workflow step. This scenario is tested by `Diagram_Run_MissingMermaidInput_ReportsError`.

**WorkflowInvalidToolsInput**: the unit reports an error when the `tools` input cannot be parsed as
a boolean. This scenario is tested by `Diagram_Run_InvalidToolsInput_ReportsError`.

**UnspecifiedVersionFallback**: the unit uses `"unspecified"` as the version label in the generated
diagram when a package has no `versionInfo`. This scenario is tested by
`Diagram_Run_PackageWithoutVersion_UsesUnspecifiedFallback`.
