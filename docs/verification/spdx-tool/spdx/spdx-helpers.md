### SpdxHelpers

#### Verification Approach

`SpdxHelpers` is verified indirectly through command tests in
`test/DemaConsulting.SpdxTool.Tests/`. Every command that loads or saves an SPDX JSON file
exercises SpdxHelpers.LoadJsonDocument and SpdxHelpers.SaveJsonDocument. The Diagram and
FindPackage tests provide direct evidence of SPDX file loading behavior.

#### Test Environment

Tests use local SPDX JSON fixtures in the standard xUnit v3 environment. No external service
is required.

#### Acceptance Criteria

Verification is acceptable when SPDX JSON files are loaded and saved correctly, missing files
are rejected with a usage error, and the tool creator entry is stamped on every saved
document.

#### Test Scenarios

**DocumentLoading**: SpdxHelpers loads a valid SPDX JSON file and returns a populated
SpdxDocument. This scenario is exercised by `Diagram_ValidSpdxFile_GeneratesDiagram`.

**MissingFileRejection**: SpdxHelpers throws CommandUsageException when the specified file
does not exist. This scenario is exercised by
`FindPackage_OnCommandLine_ReportsWorkflowOnlyError`.
