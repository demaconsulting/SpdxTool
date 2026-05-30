## SpdxModel

### Verification Approach

DemaConsulting.SpdxModel is verified through integration tests in
`test/DemaConsulting.SpdxTool.Tests/Spdx/SpdxHelpersTests.cs`. These tests exercise
`SpdxHelpers.LoadJsonDocument` and `SpdxHelpers.SaveJsonDocument`, confirming that
`Spdx2JsonDeserializer` correctly parses valid SPDX 2.x JSON documents and that
`Spdx2JsonSerializer` round-trips documents while stamping the creator tool field. The
`Diagram_ValidSpdxFile_GeneratesDiagram` test in
`test/DemaConsulting.SpdxTool.Tests/Commands/DiagramTests.cs` provides additional coverage of the
document model types by exercising package and relationship traversal.

The SelfTest subsystem exercises the full range of SPDX model operations end-to-end on every
pipeline run, providing regression coverage across all commands that load or modify SPDX
documents. The self-validation tests
`SpdxHelpers_LoadJsonDocument_ValidFile_ReturnsDocument`,
`SpdxHelpers_SaveJsonDocument_ValidDocument_StampsCreator`, and
`Diagram_ValidSpdxFile_GeneratesDiagram` cover these scenarios.

No vendor test results or third-party compliance reports are required; the integration tests and
self-validation suite described above provide sufficient evidence.
