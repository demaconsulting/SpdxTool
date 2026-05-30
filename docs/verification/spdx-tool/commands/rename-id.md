### RenameId

#### Verification Approach

`RenameId` is verified with direct command tests in
`test/DemaConsulting.SpdxTool.Tests/Commands/RenameIdTests.cs`. The tests cover argument validation,
file validation, and full document updates when an SPDX identifier is renamed.

#### Test Environment

The tests use local SPDX JSON fixtures in the standard xUnit v3 environment. No external service is
required.

#### Acceptance Criteria

Verification is acceptable when missing inputs are rejected and successful execution updates all
affected references to the renamed SPDX identifier.

#### Test Scenarios

**MissingArguments**: the unit reports a usage error when required rename arguments are omitted.
This scenario is tested by `RenameId_Run_MissingArguments_ReportsError`.

**MissingInputFile**: the unit reports an error when the input SPDX file does not exist. This
scenario is tested by `RenameId_Run_MissingFile_ReportsError`.

**ReferenceWideRename**: the unit renames the target SPDX identifier across the full document. This
scenario is tested by `RenameId_Run_ValidSpdxFile_RenamesId`.

**AllCollectionsRename**: the unit renames the target SPDX identifier across all element collections,
including file IDs, snippet from-file references, package HasFiles entries, and relationship
from-element IDs. This scenario is tested by `RenameId_Run_ValidSpdxFile_RenamesAllCollections`.

**EmptyOldId**: the unit throws a `CommandUsageException` when the old ID argument is an empty
string. This scenario is tested by `RenameId_Rename_EmptyOldId_ThrowsException`.

**OldIdIsDocument**: the unit throws a `CommandUsageException` when the old ID is the reserved
identifier `SPDXRef-DOCUMENT`. This scenario is tested by
`RenameId_Rename_OldIdIsDocument_ThrowsException`.

**EmptyNewId**: the unit throws a `CommandUsageException` when the new ID argument is an empty
string. This scenario is tested by `RenameId_Rename_EmptyNewId_ThrowsException`.

**NewIdIsDocument**: the unit throws a `CommandUsageException` when the new ID is the reserved
identifier `SPDXRef-DOCUMENT`. This scenario is tested by
`RenameId_Rename_NewIdIsDocument_ThrowsException`.

**NewIdAlreadyInUse**: the unit throws a `CommandErrorException` when the new ID is already used by
an existing package, file, or snippet in the document. This scenario is tested by
`RenameId_Rename_NewIdAlreadyInUse_ThrowsException`.

**SameId**: the unit performs no document mutation when the old and new IDs are identical. This
scenario is tested by `RenameId_Rename_SameId_NoOp`.

**WorkflowInvocation**: the unit renames an SPDX element ID when invoked from a workflow YAML step
using the `spdx`, `old`, and `new` inputs. This scenario is tested by
`RenameId_Run_WorkflowInvocation_RenamesId`.

**SnippetPointerReferences**: the unit correctly updates snippet range pointer `reference` fields
(derived from `SnippetFromFile` at serialization time) when a referenced file ID is renamed. This
scenario is tested by `RenameId_Rename_SnippetPointerReferences_UpdatesReferences`.
